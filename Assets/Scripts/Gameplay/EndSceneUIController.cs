using System;
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
    public sealed class EndSceneUIController : MonoBehaviour
    {
        [Serializable]
        public sealed class EndUiRefs
        {
            public TMP_Text Title;
            public TMP_Text Results;
            public TMP_Text Status;
            public PodiumSlotUiRefs[] Podium;
            public RectTransform LeaderboardContent;
            public Button Restart;
            public Button EndSession;
            public Button Leave;
        }

        [Serializable]
        public sealed class PodiumSlotUiRefs
        {
            public TMP_Text Rank;
            public TMP_Text Name;
            public TMP_Text Score;
            public Image Medal;
        }

        [Serializable]
        private sealed class CountdownData
        {
            public string roomCode;
            public string roomName;
            public int lessonId;
            public string lessonTitle;
            public string mode;
            public long serverNowUnixMs;
            public long startAtUnixMs;
        }

        [Serializable]
        private sealed class RoomStateData
        {
            public RoomPlayerData[] players;
        }

        [Serializable]
        private sealed class RoomPlayerData
        {
            public string playerId;
            public string displayName;
            public string avatarKey;
            public string role;
            public int seatIndex;
            public long joinSequence;
        }

        private EndUiRefs _ui;
        private TcpGameClient _client;
        private bool _endingSession;

        private void Awake()
        {
            _client = GetOrCreateTcpClient();
            _ui = BuildUi();
            var isTeacher = ClassroomQuizSessionState.IsTeacher;
            _ui.Title.text = isTeacher ? "Kết quả buổi học" : "Kết quả của bạn";
            var standings = ClassroomQuizSessionState.LastFinalStandings ?? Array.Empty<ClassroomQuizStandingSnapshot>();
            _ui.Results.text = isTeacher
                ? $"{ClassroomQuizSessionState.LessonTitle}  ·  {(ClassroomQuizSessionState.Mode == "Exam" ? "Thi" : "Học")}  ·  {ClassroomQuizSessionState.QuestionCount} câu  ·  {standings.Length} học sinh"
                : BuildPersonalSummary(ClassroomQuizSessionState.LastPersonalResult);
            _ui.EndSession.gameObject.SetActive(isTeacher);
            _ui.Restart.gameObject.SetActive(isTeacher);
            _ui.Leave.gameObject.SetActive(!isTeacher);
            _ui.Status.text = isTeacher
                ? "Kết thúc buổi học để đưa tất cả người chơi về StartScene."
                : "Bảng điểm tổng kết của lớp và thống kê cá nhân của bạn.";
            RenderFinalStandings(standings);
        }

        private void Start()
        {
            _ui.EndSession.onClick.AddListener(EndSession);
            _ui.Restart.onClick.AddListener(RestartRoom);
            _ui.Leave.onClick.AddListener(LeaveResultScreen);
            _client.MessageReceived += OnMessageReceived;
            _client.ErrorReceived += OnConnectionError;
        }

        private void OnDestroy()
        {
            if (_client != null)
            {
                _client.MessageReceived -= OnMessageReceived;
                _client.ErrorReceived -= OnConnectionError;
            }
        }

        public static EndUiRefs BuildUi()
        {
            var authoredUi = UnityEngine.Object.FindFirstObjectByType<EndSceneUiReferences>(FindObjectsInactive.Include);
            if (authoredUi != null && authoredUi.References != null)
            {
                var authoredRefs = authoredUi.References;
                BuildResultSections(authoredRefs, authoredRefs.Results.transform.parent);
                return authoredRefs;
            }

            var refs = new EndUiRefs();
            var canvas = RuntimeUiFactory.CreateCanvas("ClassroomQuiz Results Canvas");
            var background = RuntimeUiFactory.Panel(canvas.transform, "Background", Vector2.zero, Vector2.zero, RuntimeUiFactory.Background);
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.sizeDelta = Vector2.zero;
            var bgEnd = Resources.Load<Sprite>("ClassroomQuiz/Sprites/BackgroundEndScene");
            if (bgEnd != null)
            {
                background.sprite = bgEnd;
                background.color = Color.white;
            }

            var card = RuntimeUiFactory.Panel(background.transform, "Card", new Vector2(1380, 900), Vector2.zero, RuntimeUiFactory.Surface);
            var board = Resources.Load<Sprite>("ClassroomQuiz/Sprites/BoardFrame");
            if (board != null)
            {
                card.sprite = board;
                card.type = Image.Type.Sliced;
                card.color = Color.white;
            }

            refs.Title = RuntimeUiFactory.Label(background.transform, "Title", "Kết quả", new Vector2(1220, 70), new Vector2(0, 380), 42, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.Results = RuntimeUiFactory.Label(background.transform, "Results", "", new Vector2(1180, 560), new Vector2(0, 25), 27, RuntimeUiFactory.Navy, TextAlignmentOptions.TopLeft);
            refs.Status = RuntimeUiFactory.Label(background.transform, "Status", "", new Vector2(1180, 64), new Vector2(0, -315), 18, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            refs.Restart = RuntimeUiFactory.Button(background.transform, "Restart Room", "Chơi lại", new Vector2(300, 66), new Vector2(-190, -390), RuntimeUiFactory.Green, null, fontSize: 22);
            refs.EndSession = RuntimeUiFactory.Button(background.transform, "End Session", "Kết thúc buổi học", new Vector2(360, 66), new Vector2(230, -390), RuntimeUiFactory.Red, null, fontSize: 22);
            refs.Leave = RuntimeUiFactory.Button(background.transform, "Leave", "Quay về menu", new Vector2(220, 54), new Vector2(-820, 478), RuntimeUiFactory.Navy, null, fontSize: 20);
            BuildResultSections(refs, background.transform);
            canvas.gameObject.AddComponent<EndSceneUiReferences>().SetReferences(refs);
            return refs;
        }

        private static void BuildResultSections(EndUiRefs refs, Transform parent)
        {
            refs.Results.rectTransform.sizeDelta = new Vector2(1180, 68);
            refs.Results.rectTransform.anchoredPosition = new Vector2(0, 90);
            refs.Results.fontSize = 16;
            refs.Results.alignment = TextAlignmentOptions.Center;

            refs.Podium = new PodiumSlotUiRefs[3];
            var medalColors = new[]
            {
                RuntimeUiFactory.Hex("F1C40F"),
                RuntimeUiFactory.Hex("B7C0C8"),
                RuntimeUiFactory.Hex("B87333")
            };
            var medalNames = new[] { "HẠNG NHẤT", "HẠNG NHÌ", "HẠNG BA" };
            var positions = new[] { new Vector2(0, 238), new Vector2(-360, 218), new Vector2(360, 198) };
            var displayOrder = new[] { 1, 0, 2 };
            for (var visualIndex = 0; visualIndex < displayOrder.Length; visualIndex++)
            {
                var rankIndex = displayOrder[visualIndex];
                var cardColor = Color.Lerp(RuntimeUiFactory.Surface, medalColors[rankIndex], 0.14f);
                var card = RuntimeUiFactory.Panel(parent, $"Podium Card {rankIndex + 1}", new Vector2(330, 150), positions[visualIndex], cardColor);
                var podiumCardNames = new[] { "PodiumCardGold", "PodiumCardSilver", "PodiumCardBronze" };
                var podiumSprite = Resources.Load<Sprite>($"ClassroomQuiz/Sprites/{podiumCardNames[rankIndex]}");
                if (podiumSprite != null)
                {
                    card.sprite = podiumSprite;
                    card.type = Image.Type.Sliced;
                    card.color = Color.white;
                }
                refs.Podium[rankIndex] = new PodiumSlotUiRefs
                {
                    Rank = RuntimeUiFactory.Label(card.transform, "Rank Medal", medalNames[rankIndex], new Vector2(290, 34), new Vector2(0, 48), 19, medalColors[rankIndex], TextAlignmentOptions.Center, true),
                    Name = RuntimeUiFactory.Label(card.transform, "Student Name", "Chưa có học sinh", new Vector2(300, 48), new Vector2(0, 5), 21, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true),
                    Score = RuntimeUiFactory.Label(card.transform, "Student Score", "—", new Vector2(280, 36), new Vector2(0, -43), 19, RuntimeUiFactory.Navy, TextAlignmentOptions.Center),
                    Medal = CreateMedalImage(card.transform, rankIndex)
                };
            }

            RuntimeUiFactory.Label(parent, "Final Leaderboard Heading", "BẢNG XẾP HẠNG CẢ LỚP", new Vector2(1120, 24), new Vector2(0, 20), 18, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.LeaderboardContent = RuntimeUiFactory.ScrollContent(parent, "Final Leaderboard Scroll", new Vector2(1180, 280), new Vector2(0, -150), out _);
        }

        private static Image CreateMedalImage(Transform parent, int rankIndex)
        {
            var medalNames = new[] { "MedalGold", "MedalSilver", "MedalBronze" };
            var medalRect = RuntimeUiFactory.Rect(parent, "Medal Sprite", new Vector2(38, 44), new Vector2(-132, 48));
            var medal = medalRect.gameObject.AddComponent<Image>();
            medal.sprite = Resources.Load<Sprite>($"ClassroomQuiz/Sprites/{medalNames[rankIndex]}");
            medal.preserveAspect = true;
            medal.raycastTarget = false;
            return medal;
        }

        private static string BuildPersonalSummary(string result)
        {
            if (string.IsNullOrWhiteSpace(result))
            {
                return "Kết quả cá nhân chưa có dữ liệu.";
            }

            var lines = result.Split('\n');
            var summary = new System.Collections.Generic.List<string>(3);
            for (var index = 0; index < lines.Length && summary.Count < 3; index++)
            {
                var line = lines[index].Trim();
                if (!string.IsNullOrWhiteSpace(line))
                {
                    summary.Add(line);
                }
            }
            return string.Join("\n", summary);
        }

        private void RenderFinalStandings(ClassroomQuizStandingSnapshot[] standings)
        {
            if (_ui.Podium == null || _ui.LeaderboardContent == null)
            {
                return;
            }

            for (var rankIndex = 0; rankIndex < _ui.Podium.Length; rankIndex++)
            {
                var slot = _ui.Podium[rankIndex];
                var hasStudent = standings != null && rankIndex < standings.Length && standings[rankIndex] != null;
                slot.Name.text = hasStudent ? standings[rankIndex].displayName : "Chưa có học sinh";
                slot.Score.text = hasStudent ? $"{standings[rankIndex].score} điểm" : "—";
            }

            foreach (Transform child in _ui.LeaderboardContent)
            {
                Destroy(child.gameObject);
            }

            if (standings == null || standings.Length == 0)
            {
                CreateStandingRow("Chưa có dữ liệu xếp hạng.", 0);
                return;
            }

            for (var index = 0; index < standings.Length; index++)
            {
                var student = standings[index];
                var line = $"#{student.rank}   {student.displayName}   ·   {student.score} điểm   ·   Đúng {student.correctCount}/{student.totalQuestions}   ·   Sai {student.incorrectCount}   ·   Bỏ qua {student.unansweredCount}   ·   {student.totalAnswerTimeMilliseconds / 1000f:0.0}s";
                CreateStandingRow(line, index);
            }
        }

        private void CreateStandingRow(string text, int index)
        {
            var rowColor = index % 2 == 0 ? RuntimeUiFactory.Hex("F0F6FB") : RuntimeUiFactory.Hex("FAFCFE");
            var row = RuntimeUiFactory.Panel(_ui.LeaderboardContent, $"Final Standing {index + 1}", new Vector2(0, 48), Vector2.zero, rowColor);
            var layout = row.gameObject.AddComponent<LayoutElement>();
            layout.minHeight = 48;
            layout.preferredHeight = 48;
            var label = RuntimeUiFactory.Text(row.transform, "Standing Details", text, 16, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, index == 0);
            label.margin = new Vector4(18, 3, 14, 3);
            label.enableWordWrapping = false;
            label.overflowMode = TextOverflowModes.Ellipsis;
        }

        private static TcpGameClient GetOrCreateTcpClient()
        {
            if (TcpGameClient.Instance != null)
            {
                return TcpGameClient.Instance;
            }

            return new GameObject("ClassroomQuiz TCP Client").AddComponent<TcpGameClient>();
        }

        private async void EndSession()
        {
            if (_endingSession || !_client.IsConnected || !ClassroomQuizSessionState.IsTeacher)
            {
                _ui.Status.text = "Server connection or teacher permission is unavailable.";
                return;
            }

            _endingSession = true;
            _ui.EndSession.interactable = false;
            _ui.Status.text = "Đang gửi yêu cầu kết thúc tới server…";
            try
            {
                await _client.SendAsync(new NetworkMessage
                {
                    Type = "EndSession",
                    RequestId = Guid.NewGuid().ToString("N"),
                    RoomCode = ClassroomQuizSessionState.RoomCode
                });
            }
            catch (Exception exception)
            {
                _endingSession = false;
                _ui.EndSession.interactable = true;
                _ui.Status.text = exception.Message;
            }
        }

        private async void RestartRoom()
        {
            if (_endingSession || !_client.IsConnected || !ClassroomQuizSessionState.IsTeacher)
            {
                _ui.Status.text = "Chỉ giáo viên đang kết nối mới có thể chơi lại phòng.";
                return;
            }

            _endingSession = true;
            _ui.Restart.interactable = false;
            _ui.Status.text = "Đang khởi động lại phòng cho cả lớp…";
            try
            {
                await _client.SendAsync(new NetworkMessage
                {
                    Type = "RestartSession",
                    RequestId = Guid.NewGuid().ToString("N"),
                    RoomCode = ClassroomQuizSessionState.RoomCode
                });
            }
            catch (Exception exception)
            {
                _endingSession = false;
                _ui.Restart.interactable = true;
                _ui.Status.text = exception.Message;
            }
        }

        private async void LeaveResultScreen()
        {
            try
            {
                if (_client.IsConnected)
                {
                    await _client.SendAsync(new NetworkMessage
                    {
                        Type = "LeaveRoomRequest",
                        RequestId = Guid.NewGuid().ToString("N"),
                        RoomCode = ClassroomQuizSessionState.RoomCode
                    });
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not leave the result room: {exception.Message}");
            }

            ClassroomQuizSessionState.ClearRoom();
            SceneManager.LoadScene("StartScene");
        }

        private void OnMessageReceived(NetworkMessage message)
        {
            if (message.Type == "RoomStateUpdate")
            {
                ApplyRoomRoster(message.Data);
            }
            else if (message.Type == "ReturnToMenu")
            {
                ClassroomQuizSessionState.ClearRoom();
                SceneManager.LoadScene("StartScene");
            }
            else if (message.Type == "StartCountdown")
            {
                ApplyCountdownAndReturnToGameplay(message);
            }
            else if (message.Type == "Error")
            {
                _endingSession = false;
                _ui.EndSession.interactable = true;
                _ui.Restart.interactable = true;
                _ui.Status.text = message.Error ?? message.Message ?? "Server báo lỗi.";
            }
        }

        private static void ApplyRoomRoster(string json)
        {
            if (string.IsNullOrWhiteSpace(json)) return;
            try
            {
                var state = JsonUtility.FromJson<RoomStateData>(json);
                ClassroomQuizSessionState.RoomPlayers = Array.ConvertAll(state?.players ?? Array.Empty<RoomPlayerData>(), player => new ClassroomQuizPlayerSnapshot
                {
                    playerId = player.playerId,
                    displayName = player.displayName,
                    avatarKey = string.IsNullOrWhiteSpace(player.avatarKey) ? "A" : player.avatarKey,
                    role = player.role,
                    seatIndex = player.seatIndex,
                    joinSequence = player.joinSequence
                });
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Restart roster could not be read: {exception.Message}");
            }
        }

        private void ApplyCountdownAndReturnToGameplay(NetworkMessage message)
        {
            try
            {
                var countdown = JsonUtility.FromJson<CountdownData>(message.Data);
                if (countdown != null)
                {
                    ClassroomQuizSessionState.RoomCode = countdown.roomCode ?? ClassroomQuizSessionState.RoomCode;
                    ClassroomQuizSessionState.RoomName = countdown.roomName ?? ClassroomQuizSessionState.RoomName;
                    ClassroomQuizSessionState.LessonId = countdown.lessonId;
                    ClassroomQuizSessionState.LessonTitle = countdown.lessonTitle ?? ClassroomQuizSessionState.LessonTitle;
                    ClassroomQuizSessionState.Mode = countdown.mode ?? ClassroomQuizSessionState.Mode;
                    ClassroomQuizSessionState.CountdownEndsAtUnixMs = countdown.startAtUnixMs;
                    ClassroomQuizSessionState.ServerClockOffsetMs = countdown.serverNowUnixMs - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Restart countdown could not be read: {exception.Message}");
            }

            SceneManager.LoadScene("GamePlay");
        }

        private void OnConnectionError(string error)
        {
            _ui.Status.text = error + "  Hãy rời màn hình kết quả hoặc khởi động lại client.";
        }
    }
}
