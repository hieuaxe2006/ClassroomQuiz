using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using ClassroomQuiz.Managers;
using ClassroomQuiz.Network;
using ClassroomQuiz.UI;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ClassroomQuiz.Gameplay
{
    public sealed class TcpGameplayController : MonoBehaviour
    {
        [Serializable]
        private sealed class QuestionStartedData
        {
            public string questionId;
            public int sequenceIndex;
            public string prompt;
            public string[] options;
            public int questionCount;
            public long serverNowUnixMs;
            public long endsAtUnixMs;
        }

        [Serializable]
        private sealed class AnswerResultData
        {
            public bool accepted;
            public bool correct;
            public int scoreAwarded;
            public int totalScore;
            public string explanation;
            public int correctIndex = -1;
            public int answeredCount;
            public int correctCount;
            public int incorrectCount;
        }

        [Serializable]
        private sealed class RevealData
        {
            public int correctIndex = -1;
            public string explanation;
        }

        [Serializable]
        private sealed class QuestionEndedData
        {
            public int answeredCount;
            public int correctCount;
            public int incorrectCount;
            public RevealData answerReveal;
            public QuestionPointData[] points;
        }

        [Serializable]
        private sealed class QuestionPointData
        {
            public string playerId;
            public string displayName;
            public string avatarKey;
            public int pointsAwarded;
        }

        [Serializable]
        private sealed class WinnerData
        {
            public string winnerPlayerId;
            public string winnerName;
            public string winnerAvatarKey;
            public int winnerScore;
        }

        [Serializable]
        private sealed class PersonalResultData
        {
            public int rankPosition;
            public int totalStudents;
            public int score;
            public int correctCount;
            public int incorrectCount;
            public int unansweredCount;
            public int totalQuestions;
            public long totalAnswerTimeMilliseconds;
            public string topStudentName;
            public int topStudentScore;
            public string encouragement;
        }

        [Serializable]
        private sealed class LeaderboardData
        {
            public LeaderEntry[] students;
            public int totalQuestions;
            public int questionIndex;
            public int answeredCount;
            public int correctCount;
        }

        [Serializable]
        private sealed class LeaderEntry
        {
            public int rank;
            public string displayName;
            public int score;
            public int correctCount;
            public int totalQuestions;
            public long totalAnswerTimeMilliseconds;
        }

        private GameplayUiRefs _ui;
        private TcpGameClient _client;
        private string[] _currentOptions = Array.Empty<string>();
        private string _role;
        private string _mode;
        private int _questionIndex = -1;
        private int _selectedAnswerIndex = -1;
        private int _questionCount;
        private long _questionEndsAtUnixMs;
        private long _serverClockOffsetMs;
        private bool _answeredThisQuestion;
        private bool _countdownComplete;
        private float _countdownOverlayUntil;
        private bool _transitioningToResults;

        private void Awake()
        {
            _ui = GameplayRuntimeUi.Build();
            _client = GetOrCreateTcpClient();
            _role = ClassroomQuizSessionState.Role;
            _mode = ClassroomQuizSessionState.Mode;
            _serverClockOffsetMs = ClassroomQuizSessionState.ServerClockOffsetMs;
            _ui.StudentPanel.SetActive(_role != "Teacher");
            _ui.TeacherPanel.SetActive(_role == "Teacher");
            _ui.FeedbackPanel.SetActive(false);
            _ui.WinnerOverlay.SetActive(false);
            _ui.LessonTitle.text = string.IsNullOrWhiteSpace(ClassroomQuizSessionState.LessonTitle)
                ? "Toán lớp 6"
                : ClassroomQuizSessionState.LessonTitle;
            _ui.ModeLabel.text = _mode == "Exam" ? "THI" : "HỌC";
            _ui.StudentScoreText.text = "Điểm: 0";
            _ui.TeacherStatusText.text = "Bảng xếp hạng sẽ cập nhật khi học sinh trả lời.";
            RenderClassroom(ClassroomQuizSessionState.RoomPlayers);
            if (ClassroomQuizSessionState.CountdownEndsAtUnixMs <= 0)
            {
                _ui.CountdownOverlay.SetActive(false);
                _ui.Status.text = "Đang chờ câu hỏi từ server…";
            }
            else
            {
                _ui.CountdownOverlay.SetActive(true);
                _ui.CountdownText.text = "3";
                _ui.CountdownSubtext.text = "Server đang khởi động buổi học…";
            }
        }

        private void Start()
        {
            foreach (var answerButton in _ui.Answers)
            {
                var index = Array.IndexOf(_ui.Answers, answerButton);
                answerButton.onClick.AddListener(() => SubmitAnswer(index));
            }

            _client.MessageReceived += OnMessageReceived;
            _client.ErrorReceived += OnConnectionError;
        }

        private void Update()
        {
            UpdateCountdown();
            UpdateTimer();
        }

        private void OnDestroy()
        {
            if (_client != null)
            {
                _client.MessageReceived -= OnMessageReceived;
                _client.ErrorReceived -= OnConnectionError;
            }
        }

        private static TcpGameClient GetOrCreateTcpClient()
        {
            if (TcpGameClient.Instance != null)
            {
                return TcpGameClient.Instance;
            }

            var clientObject = new GameObject("ClassroomQuiz TCP Client");
            return clientObject.AddComponent<TcpGameClient>();
        }

        private void UpdateCountdown()
        {
            if (_countdownComplete || ClassroomQuizSessionState.CountdownEndsAtUnixMs <= 0)
            {
                return;
            }

            var serverNow = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _serverClockOffsetMs;
            var remainingMilliseconds = ClassroomQuizSessionState.CountdownEndsAtUnixMs - serverNow;
            if (remainingMilliseconds > 0)
            {
                var seconds = Mathf.CeilToInt(remainingMilliseconds / 1000f);
                _ui.CountdownText.text = seconds.ToString();
                _ui.CountdownSubtext.text = "Câu hỏi đầu tiên sắp bắt đầu";
                return;
            }

            if (_countdownOverlayUntil <= 0)
            {
                _ui.CountdownText.text = "BẮT ĐẦU!";
                _ui.CountdownText.fontSize = 90;
                _countdownOverlayUntil = Time.unscaledTime + 0.45f;
                return;
            }

            if (Time.unscaledTime >= _countdownOverlayUntil)
            {
                _countdownComplete = true;
                _ui.CountdownOverlay.SetActive(false);
            }
        }

        private void UpdateTimer()
        {
            if (_questionEndsAtUnixMs <= 0 || _questionCount <= 0)
            {
                return;
            }

            var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() + _serverClockOffsetMs;
            var remaining = Math.Max(0, _questionEndsAtUnixMs - now);
            var total = Math.Max(1, ClassroomQuizSessionState.DurationSeconds * 1000L);
            var seconds = Mathf.CeilToInt(remaining / 1000f);
            _ui.TimerText.text = $"{seconds}s";
            _ui.TeacherTimerText.text = $"{seconds}s";
            _ui.ProgressFill.rectTransform.anchorMax = new Vector2(Mathf.Clamp01((float)remaining / total), 1);
            _ui.TimerText.color = seconds <= 3 ? RuntimeUiFactory.Red : RuntimeUiFactory.Blue;
            if (remaining == 0 && !_answeredThisQuestion)
            {
                _ui.Status.text = "Hết giờ — đang chờ server chuyển câu.";
                SetAnswerButtonsInteractable(false);
            }
        }

        private void OnMessageReceived(NetworkMessage message)
        {
            switch (message.Type)
            {
                case "StartCountdown":
                    // The countdown is also delivered in StartScene before it loads this scene.
                    break;
                case "QuestionStarted":
                    BeginQuestion(message);
                    break;
                case "AnswerResult":
                    ApplyAnswerResult(message);
                    break;
                case "QuestionEnded":
                    EndQuestion(message);
                    break;
                case "StudentPersonalResult":
                    SaveStudentResult(message);
                    break;
                case "TeacherLeaderboard":
                    SaveTeacherLeaderboard(message);
                    break;
                case "GameEnded":
                    ShowWinnerAndLoadResults(message);
                    break;
                case "ReturnToMenu":
                    ClassroomQuizSessionState.ClearRoom();
                    SceneManager.LoadScene("StartScene");
                    break;
                case "Error":
                    var error = message.Error ?? message.Message ?? "Server báo lỗi.";
                    if (_role == "Teacher")
                    {
                        _ui.TeacherStatusText.text = error;
                    }
                    else
                    {
                        _ui.Status.text = error;
                    }
                    break;
            }
        }

        private void BeginQuestion(NetworkMessage message)
        {
            QuestionStartedData data;
            try
            {
                data = JsonUtility.FromJson<QuestionStartedData>(message.Data);
            }
            catch (Exception exception)
            {
                _ui.Status.text = $"Dữ liệu câu hỏi không đọc được: {exception.Message}";
                return;
            }
            if (data == null || string.IsNullOrWhiteSpace(data.prompt) || data.options == null || data.options.Length != 4)
            {
                _ui.Status.text = "Server gửi câu hỏi chưa hợp lệ.";
                return;
            }

            _questionIndex = message.QuestionIndex ?? 0;
            _questionCount = data.questionCount;
            _currentOptions = data.options;
            _questionEndsAtUnixMs = data.endsAtUnixMs;
            _serverClockOffsetMs = data.serverNowUnixMs - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
            _mode = message.Mode ?? _mode;
            ClassroomQuizSessionState.Mode = _mode;
            ClassroomQuizSessionState.QuestionCount = _questionCount;
            ClassroomQuizSessionState.DurationSeconds = message.DurationSeconds ?? ClassroomQuizSessionState.DurationSeconds;
            _answeredThisQuestion = false;
            _selectedAnswerIndex = -1;
            _ui.FeedbackPanel.SetActive(false);
            HideQuestionPoints();
            _ui.QuestionCount.text = $"Câu {_questionIndex + 1} / {_questionCount}";
            _ui.ModeLabel.text = _mode == "Exam" ? "THI" : "HỌC";
            _ui.Prompt.text = data.prompt;
            _ui.TeacherQuestionText.text = $"Câu {_questionIndex + 1}/{_questionCount}  ·  {data.prompt}";
            for (var i = 0; i < 4; i++)
            {
                _ui.AnswerLabels[i].text = $"{(char)('A' + i)}. {data.options[i]}";
                _ui.Answers[i].interactable = _role != "Teacher";
                _ui.Answers[i].colors = DefaultAnswerColors(i);
            }
            _ui.CountdownOverlay.SetActive(false);
            _ui.Status.text = "Chọn đáp án đúng nhanh nhất. Mỗi câu chỉ nộp một lần.";
            _ui.TeacherStatusText.text = $"Đang mở câu {_questionIndex + 1} trong {_questionCount}. Chọn đúng nhanh để được nhiều điểm hơn.";
            UpdateTimer();
        }

        private void ApplyAnswerResult(NetworkMessage message)
        {
            if (_role == "Teacher")
            {
                return;
            }

            AnswerResultData data = null;
            if (!string.IsNullOrWhiteSpace(message.Data))
            {
                try
                {
                    data = JsonUtility.FromJson<AnswerResultData>(message.Data);
                }
                catch (Exception)
                {
                    data = null;
                }
            }

            if (message.Success != true || data == null || !data.accepted)
            {
                _answeredThisQuestion = false;
                SetAnswerButtonsInteractable(true);
                _ui.FeedbackPanel.SetActive(false);
                for (var i = 0; i < _ui.Answers.Length; i++) _ui.Answers[i].colors = DefaultAnswerColors(i);
                _ui.Status.text = message.Error ?? message.Message ?? "Server chưa nhận đáp án. Bạn có thể thử lại.";
                return;
            }

            _answeredThisQuestion = true;
            SetAnswerButtonsInteractable(false);
            _ui.StudentScoreText.text = $"Điểm: {data.totalScore} ";
            _ui.FeedbackTitle.text = data.correct
                ? $"CHÍNH XÁC!  +{data.scoreAwarded} ĐIỂM"
                : "CHƯA CHÍNH XÁC  ·  +0 ĐIỂM";
            var correctAnswer = data.correctIndex >= 0 && data.correctIndex < _currentOptions.Length
                ? $"{(char)('A' + data.correctIndex)}. {_currentOptions[data.correctIndex]}"
                : string.Empty;
            _ui.FeedbackBody.text = string.IsNullOrWhiteSpace(correctAnswer)
                ? data.explanation ?? string.Empty
                : $"Đáp án đúng: {correctAnswer}\n{data.explanation}";
            _ui.FeedbackPanel.SetActive(true);
            _ui.Status.text = "Phần hướng dẫn sẽ ở đây đến hết thời gian của câu hỏi.";
            ShowAnswerReview(_selectedAnswerIndex, data.correctIndex);
        }

        private void EndQuestion(NetworkMessage message)
        {
            QuestionEndedData data = null;
            if (!string.IsNullOrWhiteSpace(message.Data))
            {
                try
                {
                    data = JsonUtility.FromJson<QuestionEndedData>(message.Data);
                }
                catch (Exception)
                {
                    data = null;
                }
            }

            SetAnswerButtonsInteractable(false);
            ShowQuestionPoints(data?.points);
            _ui.FeedbackPanel.SetActive(false);
            if (_role == "Teacher")
            {
                if (data != null)
                {
                    _ui.TeacherStatusText.text = $"Câu {_questionIndex + 1} đã kết thúc · {data.answeredCount} lượt trả lời · {data.correctCount} đúng.";
                }
                return;
            }
            if (_mode == "Study" && data != null && data.answerReveal != null)
            {
                ShowCorrectAnswer(data.answerReveal.correctIndex);
                var explanation = string.IsNullOrWhiteSpace(data.answerReveal.explanation) ? string.Empty : $"  {data.answerReveal.explanation}";
                _ui.Status.text = $"Hết giờ. Đáp án đúng: {(char)('A' + Mathf.Clamp(data.answerReveal.correctIndex, 0, 3))}.{explanation}";
            }
            else
            {
                _ui.Status.text = "Câu hỏi đã hết thời gian. Đang chờ câu tiếp theo…";
            }
        }

        private void RenderClassroom(ClassroomQuizPlayerSnapshot[] players)
        {
            players ??= Array.Empty<ClassroomQuizPlayerSnapshot>();
            var teacher = Array.Find(players, player => player != null && player.role == "Teacher");
            _ui.TeacherAvatar.text = string.IsNullOrWhiteSpace(teacher?.avatarKey) ? "A" : teacher.avatarKey;
            _ui.TeacherName.text = string.IsNullOrWhiteSpace(teacher?.displayName) ? "Giáo viên" : teacher.displayName;

            foreach (var seat in _ui.StudentSeats)
            {
                seat.Avatar.text = "—";
                seat.Name.text = "Chỗ trống";
                seat.Root.SetActive(true);
                seat.Points.gameObject.SetActive(false);
            }

            foreach (var student in players)
            {
                if (student == null || student.role == "Teacher" || student.seatIndex < 0 || student.seatIndex >= _ui.StudentSeats.Length) continue;
                var seat = _ui.StudentSeats[student.seatIndex];
                seat.Avatar.text = string.IsNullOrWhiteSpace(student.avatarKey) ? "A" : student.avatarKey;
                seat.Name.text = student.displayName ?? "Học sinh";
            }
        }

        private void ShowQuestionPoints(QuestionPointData[] points)
        {
            HideQuestionPoints();
            foreach (var student in ClassroomQuizSessionState.RoomPlayers)
            {
                if (student == null || student.role == "Teacher" || student.seatIndex < 0 || student.seatIndex >= _ui.StudentSeats.Length) continue;
                QuestionPointData result = null;
                if (points != null)
                {
                    foreach (var point in points)
                    {
                        if (point != null && point.playerId == student.playerId)
                        {
                            result = point;
                            break;
                        }
                    }
                }

                var popup = _ui.StudentSeats[student.seatIndex].Points;
                var awarded = result?.pointsAwarded ?? 0;
                popup.text = awarded > 0 ? $"+{awarded}" : "+0";
                popup.color = awarded > 0 ? RuntimeUiFactory.Green : RuntimeUiFactory.Red;
                popup.gameObject.SetActive(true);
            }
            StartCoroutine(HideQuestionPointsAfterDelay());
        }

        private IEnumerator HideQuestionPointsAfterDelay()
        {
            yield return new WaitForSecondsRealtime(3f);
            HideQuestionPoints();
        }

        private void HideQuestionPoints()
        {
            if (_ui?.StudentSeats == null) return;
            foreach (var seat in _ui.StudentSeats)
            {
                if (seat?.Points != null) seat.Points.gameObject.SetActive(false);
            }
        }

        private void ShowWinnerAndLoadResults(NetworkMessage message)
        {
            if (_transitioningToResults) return;
            _transitioningToResults = true;
            WinnerData winner = null;
            if (!string.IsNullOrWhiteSpace(message.Data))
            {
                try
                {
                    winner = JsonUtility.FromJson<WinnerData>(message.Data);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Winner result data was invalid: {exception.Message}");
                }
            }

            _ui.CountdownOverlay.SetActive(false);
            _ui.WinnerAvatar.text = string.IsNullOrWhiteSpace(winner?.winnerAvatarKey) ? "A" : winner.winnerAvatarKey;
            _ui.WinnerName.text = string.IsNullOrWhiteSpace(winner?.winnerName) ? "Chưa có học sinh" : winner.winnerName;
            _ui.WinnerOverlay.SetActive(true);
            StartCoroutine(LoadResultsAfterWinnerDisplay());
        }

        private IEnumerator LoadResultsAfterWinnerDisplay()
        {
            yield return new WaitForSecondsRealtime(3f);
            SceneManager.LoadScene("EndScene");
        }

        private async void SubmitAnswer(int answerIndex)
        {
            if (_role == "Teacher" || _answeredThisQuestion || _questionIndex < 0 || !_client.IsConnected)
            {
                return;
            }

            _selectedAnswerIndex = answerIndex;
            _answeredThisQuestion = true;
            SetAnswerButtonsInteractable(false);
            _ui.Status.text = "Đang gửi đáp án tới server…";
            try
            {
                await _client.SendAsync(new NetworkMessage
                {
                    Type = "SubmitAnswerRequest",
                    RequestId = Guid.NewGuid().ToString("N"),
                    RoomCode = ClassroomQuizSessionState.RoomCode,
                    QuestionIndex = _questionIndex,
                    AnswerIndex = answerIndex
                });
            }
            catch (Exception exception)
            {
                _answeredThisQuestion = false;
                SetAnswerButtonsInteractable(true);
                _ui.Status.text = exception.Message;
            }
        }

        private void SaveStudentResult(NetworkMessage message)
        {
            try
            {
                var result = JsonUtility.FromJson<PersonalResultData>(message.Data);
                if (result == null)
                {
                    return;
                }
                ClassroomQuizSessionState.LastPersonalResult =
                    $"Hạng của bạn: #{result.rankPosition} / {result.totalStudents}\n" +
                    $"Đúng: {result.correctCount}  ·  Sai: {result.incorrectCount}  ·  Bỏ qua: {result.unansweredCount}\n" +
                    $"Điểm: {result.score}  ·  Thời gian trả lời: {result.totalAnswerTimeMilliseconds / 1000f:0.0} giây\n\n" +
                    $"TOP 1: {result.topStudentName}  ·  {result.topStudentScore} điểm\n\n{result.encouragement}";
                ClassroomQuizPreferences.SetString("ClassroomQuiz.LastLocalResult", ClassroomQuizSessionState.LastPersonalResult);
                ClassroomQuizPreferences.Save();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Student result data was invalid: {exception.Message}");
            }
        }

        private void SaveTeacherLeaderboard(NetworkMessage message)
        {
            try
            {
                var result = JsonUtility.FromJson<LeaderboardData>(message.Data);
                if (result == null)
                {
                    return;
                }
                RenderTeacherLeaderboard(result);
                var lines = new List<string> { $"BẢNG XẾP HẠNG GIÁO VIÊN · {result.totalQuestions} câu", string.Empty };
                if (result.students == null || result.students.Length == 0)
                {
                    lines.Add("Chưa có học sinh tham gia.");
                }
                else
                {
                    foreach (var student in result.students)
                    {
                        lines.Add($"#{student.rank}  {student.displayName}  ·  {student.score} điểm  ·  {student.correctCount}/{student.totalQuestions} đúng  ·  {student.totalAnswerTimeMilliseconds / 1000f:0.0}s");
                    }
                }
                ClassroomQuizSessionState.LastTeacherLeaderboard = string.Join("\n", lines);
                ClassroomQuizPreferences.SetString("ClassroomQuiz.LastLocalResult", ClassroomQuizSessionState.LastTeacherLeaderboard);
                ClassroomQuizPreferences.Save();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Teacher leaderboard data was invalid: {exception.Message}");
            }
        }

        private void RenderTeacherLeaderboard(LeaderboardData result)
        {
            if (_role != "Teacher" || _ui.TeacherLeaderboardContent == null)
            {
                return;
            }

            foreach (Transform child in _ui.TeacherLeaderboardContent)
            {
                child.gameObject.SetActive(false);
                Destroy(child.gameObject);
            }

            var students = result.students ?? Array.Empty<LeaderEntry>();
            if (students.Length == 0)
            {
                CreateLeaderboardRow("Chưa có học sinh trong phòng.", 0);
            }
            else
            {
                for (var i = 0; i < students.Length; i++)
                {
                    var student = students[i];
                    var line = $"#{student.rank}    {student.displayName}    ·    {student.score} điểm    ·    {student.correctCount}/{student.totalQuestions} đúng    ·    {student.totalAnswerTimeMilliseconds / 1000f:0.0}s";
                    CreateLeaderboardRow(line, i);
                }
            }

            var questionLabel = result.questionIndex >= 0
                ? $"Câu {result.questionIndex + 1}/{Math.Max(1, result.totalQuestions)}"
                : "Kết quả cuối buổi";
            _ui.TeacherStatusText.text = $"{questionLabel}  ·  {result.answeredCount} lượt trả lời  ·  {result.correctCount} đáp án đúng.";
        }

        private void CreateLeaderboardRow(string text, int index)
        {
            var color = index % 2 == 0 ? RuntimeUiFactory.Hex("F0F6FB") : RuntimeUiFactory.Hex("FAFCFE");
            var row = RuntimeUiFactory.Panel(_ui.TeacherLeaderboardContent, $"Leaderboard Row {index + 1}", new Vector2(0, 58), Vector2.zero, color);
            var layoutElement = row.gameObject.AddComponent<LayoutElement>();
            layoutElement.minHeight = 58;
            layoutElement.preferredHeight = 58;
            var label = RuntimeUiFactory.Text(row.transform, "Entry", text, 22, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, index == 0);
            label.margin = new Vector4(20, 4, 16, 4);
        }

        private void OnConnectionError(string error)
        {
            if (_role == "Teacher")
            {
                _ui.TeacherStatusText.text = error;
            }
            else
            {
                _ui.Status.text = error;
                SetAnswerButtonsInteractable(false);
            }
        }

        private void ShowCorrectAnswer(int correctIndex)
        {
            if (correctIndex < 0 || correctIndex > 3)
            {
                return;
            }
            ShowAnswerReview(-1, correctIndex);
        }

        private void ShowAnswerReview(int selectedIndex, int correctIndex)
        {
            if (correctIndex < 0 || correctIndex > 3) return;
            for (var i = 0; i < _ui.Answers.Length; i++)
            {
                var color = i == correctIndex
                    ? RuntimeUiFactory.Green
                    : i == selectedIndex
                        ? RuntimeUiFactory.Red
                        : RuntimeUiFactory.Hex("77879A");
                _ui.Answers[i].colors = Tint(color);
            }
        }

        private void SetAnswerButtonsInteractable(bool interactable)
        {
            foreach (var button in _ui.Answers)
            {
                button.interactable = interactable && _role != "Teacher" && !_answeredThisQuestion;
            }
        }

        private static ColorBlock DefaultAnswerColors(int index)
        {
            var color = index switch
            {
                0 => RuntimeUiFactory.Blue,
                1 => RuntimeUiFactory.Purple,
                2 => RuntimeUiFactory.Cyan,
                _ => RuntimeUiFactory.Navy
            };
            return Tint(color);
        }

        private static ColorBlock Tint(Color color)
        {
            return new ColorBlock
            {
                normalColor = color,
                highlightedColor = Color.Lerp(color, Color.white, 0.12f),
                pressedColor = Color.Lerp(color, Color.black, 0.13f),
                selectedColor = color,
                disabledColor = color,
                colorMultiplier = 1,
                fadeDuration = 0.12f
            };
        }
    }
}
