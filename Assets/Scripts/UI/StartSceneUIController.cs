using System;
using System.Collections.Generic;
using System.Linq;
using ClassroomQuiz.Data;
using ClassroomQuiz.Managers;
using ClassroomQuiz.Network;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

namespace ClassroomQuiz.UI
{
    public sealed class StartSceneUIController : MonoBehaviour
    {
        [Serializable]
        private sealed class RoomStateData
        {
            public string roomCode;
            public string roomName;
            public string selfRole;
            public RoomPlayerData[] players;
            public int count;
            public int studentCapacity;
            public int lessonId;
            public string lessonTitle;
            public string mode;
            public long serverNowUnixMs;
            public long startAtUnixMs;
            public int durationSeconds;
            public int questionCount;
            public bool started;
        }

        [Serializable]
        private sealed class RoomPlayerData
        {
            public string playerId;
            public string displayName;
            public string role;
        }

        [Serializable]
        private sealed class CountdownData
        {
            public int seconds;
            public string roomCode;
            public string roomName;
            public int lessonId;
            public string lessonTitle;
            public string mode;
            public long serverNowUnixMs;
            public long startAtUnixMs;
        }

        [Serializable]
        private sealed class RoomHistoryResponse
        {
            public bool success;
            public string error;
            public RoomHistoryEntry[] rooms;
        }

        [Serializable]
        private sealed class RoomHistoryEntry
        {
            public long roomId;
            public string roomCode;
            public string roomName;
            public string lessonTitle;
            public string mode;
            public string hostName;
            public string createdAt;
            public int rankPosition;
        }

        [Serializable]
        private sealed class RoomDetailsResponse
        {
            public bool success;
            public string error;
            public bool isHost;
            public RoomSummary room;
            public RoomHistoryDetailEntry[] details;
        }

        [Serializable]
        private sealed class RoomSummary
        {
            public long roomId;
            public string roomName;
            public string roomCode;
            public string lessonTitle;
            public string hostName;
        }

        [Serializable]
        private sealed class RoomHistoryDetailEntry
        {
            public string displayName;
            public int rankPosition;
            public int correctCount;
            public int incorrectCount;
            public int unansweredCount;
            public int totalQuestions;
            public int score;
            public long responseTimeMs;
        }

        private JsonLessonRepository _lessonRepository;
        private List<LessonDefinition> _lessons;
        private TcpGameClient _tcpClient;
        private StartSceneUiRefs _ui;
        private readonly List<QuestionEditorRow> _editorRows = new List<QuestionEditorRow>();
        private List<LessonQuestionData> _editingQuestions = new List<LessonQuestionData>();
        private LessonDefinition _editingLesson;
        private GameObject _activePanel;
        private int _waitingStudentCount;
        private bool _waitingRoomStarted;
        private bool _isConnecting;
        private bool _isSendingRoomRequest;
        private bool _historyDetailsOpen;
        private string _pendingAvatar = "A";
        private List<StudyMaterial> _studyMaterials = new List<StudyMaterial>();

        private void Awake()
        {
            _lessonRepository = new JsonLessonRepository();
            _lessons = _lessonRepository.LoadLessons();
            _ui = StartSceneRuntimeUi.Build(transform);
            _tcpClient = GetOrCreateTcpClient();
            ConfigureSettingsDefaults();
            PopulateLessonDropdown();
            RefreshWelcome();
            UpdateLessonSummary();
            SetScreen(_ui.MainPanel);
        }

        private void Start()
        {
            BindButtons();
            _tcpClient.MessageReceived += OnMessageReceived;
            _tcpClient.ErrorReceived += OnConnectionError;
        }

        private void Update()
        {
            if (_isConnecting && _ui != null && _ui.ConnectingProgressFill != null)
            {
                var normalized = 0.12f + Mathf.PingPong(Time.unscaledTime * 0.42f, 0.76f);
                var fillRect = _ui.ConnectingProgressFill.rectTransform;
                fillRect.anchorMax = new Vector2(normalized, 1);
            }
        }

        private void OnDestroy()
        {
            if (_tcpClient != null)
            {
                _tcpClient.MessageReceived -= OnMessageReceived;
                _tcpClient.ErrorReceived -= OnConnectionError;
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

        private void BindButtons()
        {
            _ui.PlayButton.onClick.AddListener(OnPlayClicked);
            _ui.CharacterButton.onClick.AddListener(ShowCharacterPanel);
            _ui.LeaderboardButton.onClick.AddListener(ShowLeaderboardPanel);
            _ui.StudyButton.onClick.AddListener(ShowStudyPanel);
            _ui.SettingsButton.onClick.AddListener(ShowSettingsPanel);
            _ui.ExitButton.onClick.AddListener(OnExitClicked);
            _ui.CharacterBackButton.onClick.AddListener(ShowMainMenu);
            _ui.CharacterConfirmButton.onClick.AddListener(ConfirmAvatar);
            for (var i = 0; i < _ui.AvatarChoiceButtons.Length; i++)
            {
                var avatar = _ui.AvatarChoices[i];
                _ui.AvatarChoiceButtons[i].onClick.AddListener(() => SelectAvatar(avatar));
            }
            _ui.LeaderboardBackButton.onClick.AddListener(HandleHistoryBack);
            _ui.StudyBackButton.onClick.AddListener(ShowMainMenu);
            _ui.SettingsBackButton.onClick.AddListener(ShowMainMenu);
            _ui.RoomHubBackButton.onClick.AddListener(ShowMainMenu);
            _ui.CreateRoomHubButton.onClick.AddListener(ShowCreateRoomPanel);
            _ui.FindRoomHubButton.onClick.AddListener(ShowJoinRoomPanel);
            _ui.CreateBackButton.onClick.AddListener(ShowRoomHub);
            _ui.JoinBackButton.onClick.AddListener(ShowRoomHub);
            _ui.SubmitCreateRoomButton.onClick.AddListener(OnCreateRoomClicked);
            _ui.SubmitJoinRoomButton.onClick.AddListener(OnJoinRoomClicked);
            _ui.EditQuestionsButton.onClick.AddListener(OpenQuestionEditor);
            _ui.SaveSettingsButton.onClick.AddListener(SaveSettings);
            _ui.StartGameButton.onClick.AddListener(OnStartGameClicked);
            _ui.QuestionEditorAddButton.onClick.AddListener(AddEditorQuestion);
            _ui.QuestionEditorSaveButton.onClick.AddListener(SaveEditedQuestions);
            _ui.QuestionEditorCancelButton.onClick.AddListener(ShowCreateRoomPanel);
            _ui.CopyRoomCodeButton.onClick.AddListener(CopyRoomCode);
            _ui.WaitingLeaveButton.onClick.AddListener(LeaveRoom);
            _ui.ConnectingRetryButton.onClick.AddListener(OnPlayClicked);
            _ui.ConnectingBackButton.onClick.AddListener(() =>
            {
                _isConnecting = false;
                RuntimeUiFactory.SetVisible(_ui.ConnectingPanel, false);
                ShowMainMenu();
            });

            _ui.VolumeSlider.onValueChanged.AddListener(value => AudioListener.volume = value);
            _ui.LessonDropdown.onValueChanged.AddListener(_ => UpdateLessonSummary());
        }

        private void ConfigureSettingsDefaults()
        {
            _ui.ServerAddressInput.text = PlayerPrefs.GetString("ClassroomQuiz.ServerAddress", "127.0.0.1");
            _ui.ServerPortInput.text = PlayerPrefs.GetInt("ClassroomQuiz.ServerPort", 7777).ToString();
            var volume = PlayerPrefs.GetFloat("ClassroomQuiz.Volume", 1f);
            _ui.VolumeSlider.SetValueWithoutNotify(volume);
            AudioListener.volume = volume;
            _ui.AvatarPreview.text = PlayerPrefs.GetString("ClassroomQuiz.Avatar", "A");
            _ui.ConnectingRetryButton.gameObject.SetActive(false);
        }

        private void PopulateLessonDropdown()
        {
            var options = new List<string>(_lessons.Count);
            for (var i = 0; i < _lessons.Count; i++)
            {
                var lesson = _lessons[i];
                var marker = lesson.status == LessonStatus.Ready && lesson.IsReadyForRoom ? "Sẵn sàng" : "Bản nháp";
                var demo = lesson.isDemo ? " · DEMO" : string.Empty;
                options.Add($"{lesson.lessonId:00} · {lesson.title} — {marker}{demo}");
            }

            _ui.LessonDropdown.ClearOptions();
            _ui.LessonDropdown.AddOptions(options);
            _ui.LessonDropdown.SetValueWithoutNotify(0);
            _ui.QuestionSetDropdown.ClearOptions();
            _ui.QuestionSetDropdown.AddOptions(new List<string> { "Bộ câu hỏi đang lưu của bài" });
            _ui.QuestionSetDropdown.SetValueWithoutNotify(0);
            _ui.QuestionSetDropdown.RefreshShownValue();
            _ui.LessonDropdown.RefreshShownValue();
        }

        private void RefreshWelcome()
        {
            var name = PlayerPrefs.GetString("ClassroomQuiz.DisplayName", "Guest");
            var avatar = PlayerPrefs.GetString("ClassroomQuiz.Avatar", "A");
            _ui.WelcomeText.text = $"Xin chào {name}  ·  Avatar {avatar}";
        }

        private void UpdateLessonSummary()
        {
            var lesson = SelectedLesson();
            if (lesson == null)
            {
                _ui.LessonSummaryText.text = "Chưa chọn được bài học.";
                return;
            }

            var ready = lesson.IsReadyForRoom;
            var demo = lesson.isDemo ? "DEMO · " : string.Empty;
            var status = ready ? "Sẵn sàng tạo phòng" : "Bản nháp — cần chỉnh sửa câu hỏi";
            var count = lesson.questions != null ? lesson.questions.Count : 0;
            _ui.LessonSummaryText.text = $"{demo}{lesson.title}  ·  {count} câu  ·  {status}\n{lesson.summary}";
            _ui.SubmitCreateRoomButton.interactable = ready;
        }

        private LessonDefinition SelectedLesson()
        {
            if (_lessons == null || _lessons.Count == 0 || _ui == null || _ui.LessonDropdown == null)
            {
                return null;
            }

            return _lessons[Mathf.Clamp(_ui.LessonDropdown.value, 0, _lessons.Count - 1)];
        }

        public async void OnPlayClicked()
        {
            if (_isConnecting)
            {
                return;
            }

            if (_tcpClient.IsConnected)
            {
                SetScreen(_ui.RoomHubPanel);
                return;
            }

            _isConnecting = true;
            _ui.ConnectingRetryButton.gameObject.SetActive(false);
            _ui.ConnectingStatusText.text = "Đang kết nối TCP với server…";
            RuntimeUiFactory.SetVisible(_ui.ConnectingPanel, true);
            var address = _ui.ServerAddressInput.text.Trim();
            var portText = _ui.ServerPortInput.text.Trim();
            if (!int.TryParse(portText, out var port) || port < 1 || port > 65535)
            {
                _isConnecting = false;
                ShowConnectionFailure("Cổng server phải là số từ 1 đến 65535.");
                return;
            }

            PlayerPrefs.SetString("ClassroomQuiz.ServerAddress", address);
            PlayerPrefs.SetInt("ClassroomQuiz.ServerPort", port);
            PlayerPrefs.Save();

            try
            {
                var name = PlayerPrefs.GetString("ClassroomQuiz.DisplayName", "Guest");
                var connected = await _tcpClient.ConnectAsync(address, port, name);
                _isConnecting = false;
                if (!connected)
                {
                    ShowConnectionFailure("Không kết nối được. Kiểm tra server, IP và cổng rồi thử lại.");
                    return;
                }

                RuntimeUiFactory.SetVisible(_ui.ConnectingPanel, false);
                _ui.HubStatusText.text = $"Đã kết nối {address}:{port}. Tạo phòng hoặc nhập mã phòng.";
                SetScreen(_ui.RoomHubPanel);
            }
            catch (Exception exception)
            {
                _isConnecting = false;
                ShowConnectionFailure(exception.Message);
            }
        }

        public async void OnCreateRoomClicked()
        {
            if (_isSendingRoomRequest)
            {
                return;
            }

            var lesson = SelectedLesson();
            if (lesson == null || !lesson.IsReadyForRoom)
            {
                SetStatus("Bài học cần có 2–5 câu hợp lệ. Hãy chọn bài Sẵn sàng hoặc chỉnh sửa bài.", RuntimeUiFactory.Orange);
                return;
            }
            if (!_tcpClient.IsConnected)
            {
                ShowConnectionFailure("Chưa kết nối server. Hãy nhấn Chơi / Kết nối trước.");
                return;
            }

            var payload = BuildRoomPayload(lesson);
            var message = new NetworkMessage
            {
                Type = "CreateRoomRequest",
                RequestId = Guid.NewGuid().ToString("N"),
                RoomName = _ui.RoomNameInput.text.Trim(),
                Mode = _ui.ModeDropdown.value == 0 ? "Study" : "Exam",
                LessonId = lesson.lessonId,
                LessonTitle = lesson.title,
                DurationSeconds = DurationForIndex(_ui.DurationDropdown.value),
                PlayerName = PlayerPrefs.GetString("ClassroomQuiz.DisplayName", "Guest"),
                Payload = JsonUtility.ToJson(payload)
            };

            _isSendingRoomRequest = true;
            _ui.SubmitCreateRoomButton.interactable = false;
            SetStatus("Đang tạo phòng trên server…", RuntimeUiFactory.Muted);
            try
            {
                await _tcpClient.SendAsync(message);
            }
            catch (Exception exception)
            {
                SetStatus(exception.Message, RuntimeUiFactory.Red);
                _isSendingRoomRequest = false;
                UpdateLessonSummary();
                return;
            }
            _isSendingRoomRequest = false;
            UpdateLessonSummary();
        }

        public async void OnJoinRoomClicked()
        {
            if (!_tcpClient.IsConnected)
            {
                ShowConnectionFailure("Chưa kết nối server. Hãy nhấn Chơi / Kết nối trước.");
                return;
            }

            var code = (_ui.RoomCodeInput.text ?? string.Empty).Trim().ToUpperInvariant();
            if (code.Length != 6)
            {
                SetStatus("Mã phòng cần có 6 ký tự.", RuntimeUiFactory.Orange);
                return;
            }

            SetStatus("Đang tìm phòng…", RuntimeUiFactory.Muted);
            try
            {
                await _tcpClient.SendAsync(new NetworkMessage
                {
                    Type = "JoinRoomRequest",
                    RequestId = Guid.NewGuid().ToString("N"),
                    RoomCode = code,
                    PlayerName = PlayerPrefs.GetString("ClassroomQuiz.DisplayName", "Guest")
                });
            }
            catch (Exception exception)
            {
                SetStatus(exception.Message, RuntimeUiFactory.Red);
            }
        }

        private async void OnStartGameClicked()
        {
            if (!_tcpClient.IsConnected || !ClassroomQuizSessionState.IsTeacher)
            {
                SetStatus("Chỉ giáo viên đã kết nối mới được bắt đầu.", RuntimeUiFactory.Orange);
                return;
            }
            if (_waitingRoomStarted || _waitingStudentCount < 2)
            {
                SetStatus("Cần giáo viên và ít nhất 2 học sinh trong phòng mới có thể bắt đầu.", RuntimeUiFactory.Orange);
                return;
            }

            SetStatus("Đang yêu cầu server bắt đầu…", RuntimeUiFactory.Muted);
            try
            {
                await _tcpClient.SendAsync(new NetworkMessage
                {
                    Type = "StartGameRequest",
                    RequestId = Guid.NewGuid().ToString("N"),
                    RoomCode = ClassroomQuizSessionState.RoomCode
                });
            }
            catch (Exception exception)
            {
                SetStatus(exception.Message, RuntimeUiFactory.Red);
            }
        }

        private async void LeaveRoom()
        {
            try
            {
                if (_tcpClient.IsConnected)
                {
                    await _tcpClient.SendAsync(new NetworkMessage
                    {
                        Type = "LeaveRoomRequest",
                        RequestId = Guid.NewGuid().ToString("N"),
                        RoomCode = ClassroomQuizSessionState.RoomCode
                    });
                }
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Could not send LeaveRoom: {exception.Message}");
            }

            ClassroomQuizSessionState.ClearRoom();
            SetScreen(_ui.RoomHubPanel);
        }

        private void OnMessageReceived(NetworkMessage message)
        {
            if (message == null)
            {
                return;
            }

            switch (message.Type)
            {
                case "Welcome":
                    _ui.HubStatusText.text = message.Message;
                    break;
                case "CreateRoomResult":
                case "JoinRoomResult":
                    if (message.Success == true)
                    {
                        ClassroomQuizSessionState.RoomCode = message.RoomCode ?? string.Empty;
                        ClassroomQuizSessionState.Role = message.Role ?? (message.Type == "CreateRoomResult" ? "Teacher" : "Student");
                        OpenWaitingRoom();
                    }
                    else
                    {
                        SetStatus(message.Error ?? message.Message ?? "Không thể xử lý phòng.", RuntimeUiFactory.Red);
                    }
                    break;
                case "RoomStateUpdate":
                    ApplyRoomState(message);
                    break;
                case "PlayerDisconnected":
                    SetStatus(message.Message ?? "Một người đã rời phòng.", RuntimeUiFactory.Orange);
                    break;
                case "StartCountdown":
                    ApplyCountdown(message);
                    break;
                case "Error":
                    SetStatus(message.Error ?? message.Message ?? "Server báo lỗi.", RuntimeUiFactory.Red);
                    break;
            }
        }

        private void ApplyRoomState(NetworkMessage message)
        {
            RoomStateData state = null;
            if (!string.IsNullOrWhiteSpace(message.Data))
            {
                try
                {
                    state = JsonUtility.FromJson<RoomStateData>(message.Data);
                }
                catch (Exception exception)
                {
                    Debug.LogWarning($"Room state JSON could not be read: {exception.Message}");
                }
            }
            if (state == null)
            {
                return;
            }

            ClassroomQuizSessionState.RoomCode = state.roomCode ?? message.RoomCode ?? string.Empty;
            ClassroomQuizSessionState.RoomName = state.roomName ?? message.RoomName ?? string.Empty;
            ClassroomQuizSessionState.Role = state.selfRole ?? message.Role ?? "Student";
            ClassroomQuizSessionState.Mode = state.mode ?? "Study";
            ClassroomQuizSessionState.LessonId = state.lessonId;
            ClassroomQuizSessionState.LessonTitle = state.lessonTitle ?? string.Empty;
            ClassroomQuizSessionState.DurationSeconds = state.durationSeconds > 0 ? state.durationSeconds : 15;
            ClassroomQuizSessionState.QuestionCount = state.questionCount;
            OpenWaitingRoom();

            var players = state.players ?? Array.Empty<RoomPlayerData>();
            _waitingStudentCount = players.Count(player => !string.Equals(player.role, "Teacher", StringComparison.Ordinal));
            _waitingRoomStarted = state.started;
            var lines = new List<string>(players.Length + 1)
            {
                $"{players.Length} người tham gia · {_waitingStudentCount} học sinh{(state.studentCapacity > 0 ? $"  ·  tối đa {state.studentCapacity} học sinh" : string.Empty)}"
            };
            foreach (var player in players)
            {
                var roleText = player.role == "Teacher" ? "Giáo viên" : "Học sinh";
                lines.Add($"[{(player.role == "Teacher" ? "GV" : "HS")}]  {player.displayName}   —   {roleText}");
            }

            _ui.RosterText.text = string.Join("\n", lines);
            _ui.RosterText.rectTransform.sizeDelta = new Vector2(740, Mathf.Max(120, lines.Count * 38));
            _ui.RoomCodeText.text = ClassroomQuizSessionState.RoomCode;
            _ui.StartGameButton.gameObject.SetActive(ClassroomQuizSessionState.IsTeacher);
            _ui.StartGameButton.interactable = ClassroomQuizSessionState.IsTeacher && _waitingStudentCount >= 2 && !state.started;
            _ui.WaitingStatusText.text = state.started ? "Đang bắt đầu…" : ClassroomQuizSessionState.IsTeacher
                ? _waitingStudentCount < 2
                    ? $"Cần thêm {2 - _waitingStudentCount} học sinh nữa để bắt đầu."
                    : "Đã đủ người. Nhấn Bắt đầu để vào bảng theo dõi."
                : "Đang chờ giáo viên bắt đầu buổi học.";
        }

        private void ApplyCountdown(NetworkMessage message)
        {
            if (!string.IsNullOrWhiteSpace(message.Data))
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
                    Debug.LogWarning($"Countdown data could not be read: {exception.Message}");
                }
            }
            SceneManager.LoadScene("GamePlay");
        }

        private void OnConnectionError(string error)
        {
            _isConnecting = false;
            if (_activePanel == _ui.ConnectingPanel || _activePanel == _ui.MainPanel)
            {
                ShowConnectionFailure(error);
            }
            else
            {
                SetStatus(error, RuntimeUiFactory.Red);
            }
        }

        private void ShowConnectionFailure(string message)
        {
            _isConnecting = false;
            RuntimeUiFactory.SetVisible(_ui.ConnectingPanel, true);
            _ui.ConnectingStatusText.text = message;
            _ui.ConnectingRetryButton.gameObject.SetActive(true);
            _ui.ConnectingProgressFill.rectTransform.anchorMax = new Vector2(1, 1);
        }

        private void SetStatus(string message, Color color)
        {
            if (_activePanel == _ui.CreateRoomPanel)
            {
                _ui.CreateStatusText.text = message;
                _ui.CreateStatusText.color = color;
            }
            else if (_activePanel == _ui.JoinRoomPanel)
            {
                _ui.JoinStatusText.text = message;
                _ui.JoinStatusText.color = color;
            }
            else if (_activePanel == _ui.WaitingRoomPanel)
            {
                _ui.WaitingStatusText.text = message;
                _ui.WaitingStatusText.color = color;
            }
            else if (_activePanel == _ui.RoomHubPanel)
            {
                _ui.HubStatusText.text = message;
                _ui.HubStatusText.color = color;
            }
            else if (_activePanel == _ui.SettingsPanel)
            {
                _ui.SettingsStatusText.text = message;
                _ui.SettingsStatusText.color = color;
            }
            else
            {
                _ui.MainStatusText.text = message;
                _ui.MainStatusText.color = color;
            }
        }

        private void SetScreen(GameObject panel)
        {
            _activePanel = panel;
            StartSceneRuntimeUi.ShowOnly(_ui, panel);
        }

        private void ShowMainMenu()
        {
            RefreshWelcome();
            SetScreen(_ui.MainPanel);
        }

        private void ShowCharacterPanel()
        {
            _pendingAvatar = PlayerPrefs.GetString("ClassroomQuiz.Avatar", "A");
            _ui.AvatarPreview.text = _pendingAvatar;
            RefreshAvatarSelection();
            SetScreen(_ui.CharacterPanel);
        }

        private void ShowLeaderboardPanel()
        {
            _historyDetailsOpen = false;
            SetScreen(_ui.LeaderboardPanel);
            SetHistoryListVisible(true);
            ClearContent(_ui.HistoryContent);

            var token = PlayerPrefs.GetString(PhpApiClient.AccountTokenPreferenceKey, string.Empty);
            if (string.IsNullOrWhiteSpace(token))
            {
                _ui.HistoryStatusText.text = "Đăng nhập tài khoản để xem các phòng đã chơi.";
                return;
            }

            _ui.HistoryStatusText.text = "Đang tải lịch sử phòng…";
            StartCoroutine(PhpApiClient.Post(PhpApiClient.ConfiguredUrl,
                new ClassroomQuizApiRequest { action = "listRooms" }, token, OnRoomHistoryReceived));
        }

        private void OnRoomHistoryReceived(ClassroomQuizApiResponse response)
        {
            var history = response != null && !string.IsNullOrWhiteSpace(response.rawJson)
                ? JsonUtility.FromJson<RoomHistoryResponse>(response.rawJson)
                : null;
            if (response == null || !response.success || history == null || !history.success)
            {
                _ui.HistoryStatusText.text = response?.error ?? history?.error ?? "Không tải được lịch sử. Kiểm tra XAMPP API.";
                return;
            }

            var rooms = history.rooms ?? Array.Empty<RoomHistoryEntry>();
            if (rooms.Length == 0)
            {
                _ui.HistoryStatusText.text = "Chưa có phòng nào được lưu. Lịch sử được ghi sau khi một buổi quiz kết thúc.";
                return;
            }

            _ui.HistoryStatusText.text = $"{rooms.Length} phòng gần đây · chọn một phòng để xem kết quả.";
            for (var i = 0; i < rooms.Length; i++)
            {
                var room = rooms[i];
                var rank = room.rankPosition > 0 ? $"#{room.rankPosition}" : "Giáo viên";
                var text = $"{room.roomName}     |     {room.hostName}     |     {room.roomCode}     |     {room.createdAt}     |     {rank}";
                var capturedRoomId = room.roomId;
                var row = RuntimeUiFactory.Button(_ui.HistoryContent, $"Room History {room.roomId}", text,
                    new Vector2(0, 72), Vector2.zero, i % 2 == 0 ? RuntimeUiFactory.Hex("EAF4FC") : RuntimeUiFactory.Hex("F7FAFD"),
                    () => ShowRoomHistoryDetails(capturedRoomId), fontSize: 18);
                row.GetComponentInChildren<TMP_Text>().alignment = TextAlignmentOptions.MidlineLeft;
                var layout = row.gameObject.AddComponent<LayoutElement>();
                layout.minHeight = 72;
                layout.preferredHeight = 72;
            }
        }

        private void ShowRoomHistoryDetails(long roomId)
        {
            var token = PlayerPrefs.GetString(PhpApiClient.AccountTokenPreferenceKey, string.Empty);
            if (string.IsNullOrWhiteSpace(token)) return;
            _historyDetailsOpen = true;
            SetHistoryListVisible(false);
            _ui.HistoryDetailPanel.SetActive(true);
            _ui.HistoryDetailTitle.text = "Đang tải chi tiết phòng…";
            ClearContent(_ui.HistoryDetailsContent);
            StartCoroutine(PhpApiClient.Post(PhpApiClient.ConfiguredUrl,
                new ClassroomQuizApiRequest { action = "getRoomDetails", roomId = roomId }, token, OnRoomDetailsReceived));
        }

        private void OnRoomDetailsReceived(ClassroomQuizApiResponse response)
        {
            var detail = response != null && !string.IsNullOrWhiteSpace(response.rawJson)
                ? JsonUtility.FromJson<RoomDetailsResponse>(response.rawJson)
                : null;
            if (response == null || !response.success || detail == null || !detail.success || detail.room == null)
            {
                _ui.HistoryDetailTitle.text = response?.error ?? detail?.error ?? "Không tải được chi tiết phòng.";
                return;
            }

            _ui.HistoryDetailTitle.text = $"{detail.room.roomName} · Mã {detail.room.roomCode} · Chủ phòng {detail.room.hostName}";
            var entries = detail.details ?? Array.Empty<RoomHistoryDetailEntry>();
            for (var i = 0; i < entries.Length; i++)
            {
                var entry = entries[i];
                var text = $"{entry.displayName}     |     #{entry.rankPosition}     |     {entry.correctCount} đúng / {entry.incorrectCount} sai / {entry.unansweredCount} bỏ qua     |     {entry.responseTimeMs / 1000f:0.0}s     |     {entry.score} điểm";
                var row = RuntimeUiFactory.Panel(_ui.HistoryDetailsContent, $"History Detail {i + 1}", new Vector2(0, 64), Vector2.zero,
                    i % 2 == 0 ? RuntimeUiFactory.Hex("EAF4FC") : RuntimeUiFactory.Hex("F7FAFD"));
                var layout = row.gameObject.AddComponent<LayoutElement>();
                layout.minHeight = 64;
                layout.preferredHeight = 64;
                RuntimeUiFactory.Text(row.transform, "Entry", text, 18, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, i == 0);
            }
        }

        private void SetHistoryListVisible(bool visible)
        {
            _ui.HistoryContent.gameObject.SetActive(visible);
            _ui.HistoryStatusText.gameObject.SetActive(visible);
            _ui.HistoryColumnsText.gameObject.SetActive(visible);
            _ui.HistoryDetailPanel.SetActive(!visible);
        }

        private void HandleHistoryBack()
        {
            if (_historyDetailsOpen)
            {
                _historyDetailsOpen = false;
                SetHistoryListVisible(true);
            }
            else
            {
                ShowMainMenu();
            }
        }

        private void ShowStudyPanel()
        {
            SetScreen(_ui.StudyPanel);
            _studyMaterials = StudyMaterialCatalog.Build(_lessons);
            ClearContent(_ui.StudyListContent);
            _ui.StudyDetailTitle.text = "Chọn một chủ đề để học";
            _ui.StudyDetailText.text = "Nội dung tự học không có giới hạn thời gian, dùng ví dụ và bài tập riêng, không lấy câu hỏi trong phòng.";
            for (var i = 0; i < _studyMaterials.Count; i++)
            {
                var material = _studyMaterials[i];
                var index = i;
                var row = RuntimeUiFactory.Button(_ui.StudyListContent, $"Study Topic {material.LessonId}", material.Title,
                    new Vector2(0, 68), Vector2.zero, i % 2 == 0 ? RuntimeUiFactory.Hex("EAF4FC") : RuntimeUiFactory.Hex("F7FAFD"),
                    () => ShowStudyMaterial(index), fontSize: 17);
                row.GetComponentInChildren<TMP_Text>().alignment = TextAlignmentOptions.MidlineLeft;
                var layout = row.gameObject.AddComponent<LayoutElement>();
                layout.minHeight = 68;
                layout.preferredHeight = 68;
            }
        }

        private void ShowStudyMaterial(int index)
        {
            if (index < 0 || index >= _studyMaterials.Count) return;
            var material = _studyMaterials[index];
            _ui.StudyDetailTitle.text = material.Title;
            _ui.StudyDetailText.text = $"Ý CHÍNH\n{material.Concept}\n\nVÍ DỤ CÓ LỜI GIẢI\n{material.WorkedExample}\n\nTỰ LUYỆN (KHÔNG TÍNH GIỜ)\n{material.Challenge}\n\nBạn có thể quay lại chủ đề này và học lại bất cứ lúc nào.";
        }

        private void SelectAvatar(string avatar)
        {
            _pendingAvatar = avatar;
            _ui.AvatarPreview.text = avatar;
            RefreshAvatarSelection();
        }

        private void ConfirmAvatar()
        {
            PlayerPrefs.SetString("ClassroomQuiz.Avatar", _pendingAvatar);
            PlayerPrefs.Save();
            RefreshWelcome();
            ShowMainMenu();
        }

        private void RefreshAvatarSelection()
        {
            if (_ui.AvatarChoiceButtons == null) return;
            for (var i = 0; i < _ui.AvatarChoiceButtons.Length; i++)
            {
                var button = _ui.AvatarChoiceButtons[i];
                var outline = button.GetComponent<Outline>();
                var selected = _ui.AvatarChoices[i] == _pendingAvatar;
                outline.enabled = selected;
                button.targetGraphic.color = selected ? RuntimeUiFactory.Hex("168A53") : RuntimeUiFactory.Purple;
            }
        }

        private static void ClearContent(Transform content)
        {
            foreach (Transform child in content)
            {
                Destroy(child.gameObject);
            }
        }

        private void ShowSettingsPanel()
        {
            SetScreen(_ui.SettingsPanel);
        }

        private void ShowRoomHub()
        {
            SetScreen(_ui.RoomHubPanel);
        }

        private void ShowCreateRoomPanel()
        {
            UpdateLessonSummary();
            SetScreen(_ui.CreateRoomPanel);
        }

        private void ShowJoinRoomPanel()
        {
            _ui.RoomCodeInput.text = string.Empty;
            SetScreen(_ui.JoinRoomPanel);
        }

        private void OpenWaitingRoom()
        {
            _ui.RoomCodeText.text = ClassroomQuizSessionState.RoomCode;
            _ui.StartGameButton.gameObject.SetActive(ClassroomQuizSessionState.IsTeacher);
            _ui.StartGameButton.interactable = ClassroomQuizSessionState.IsTeacher && _waitingStudentCount >= 2 && !_waitingRoomStarted;
            if (string.IsNullOrEmpty(_ui.RosterText.text))
            {
                _ui.RosterText.text = PlayerPrefs.GetString("ClassroomQuiz.DisplayName", "Guest");
            }
            SetScreen(_ui.WaitingRoomPanel);
        }

        private void OpenQuestionEditor()
        {
            _editingLesson = SelectedLesson();
            if (_editingLesson == null)
            {
                SetStatus("Chưa chọn được bài học để chỉnh sửa.", RuntimeUiFactory.Red);
                return;
            }

            _ui.LessonTitleInput.text = _editingLesson.title;
            _editingQuestions = _editingLesson.questions == null
                ? new List<LessonQuestionData>()
                : _editingLesson.questions.Select(CloneQuestion).ToList();
            while (_editingQuestions.Count < 2)
            {
                _editingQuestions.Add(CreateBlankQuestion(_editingLesson.lessonId, _editingLesson.title, _editingQuestions.Count + 1));
            }
            RebuildEditorRows();
            _ui.EditorStatusText.text = _editingLesson.isDemo ? "DEMO — dữ liệu mẫu có thể được giáo viên chỉnh sửa." : "Mỗi bài cần từ 2 đến 5 câu hợp lệ.";
            SetScreen(_ui.QuestionEditorPanel);
        }

        private void RebuildEditorRows()
        {
            foreach (Transform child in _ui.QuestionEditorContent)
            {
                Destroy(child.gameObject);
            }
            _editorRows.Clear();
            for (var i = 0; i < _editingQuestions.Count; i++)
            {
                var rowIndex = i;
                _editorRows.Add(StartSceneRuntimeUi.BuildQuestionRow(_ui.QuestionEditorContent, _editingQuestions[i], i,
                    () => RemoveEditorQuestion(rowIndex)));
            }
        }

        private void AddEditorQuestion()
        {
            CaptureEditorValues();
            if (_editingQuestions.Count >= 5)
            {
                _ui.EditorStatusText.text = "Mỗi bài tối đa 5 câu.";
                _ui.EditorStatusText.color = RuntimeUiFactory.Orange;
                return;
            }

            _editingQuestions.Add(CreateBlankQuestion(_editingLesson.lessonId, _ui.LessonTitleInput.text, _editingQuestions.Count + 1));
            RebuildEditorRows();
        }

        private void RemoveEditorQuestion(int index)
        {
            CaptureEditorValues();
            if (_editingQuestions.Count <= 2)
            {
                _ui.EditorStatusText.text = "Mỗi bài cần tối thiểu 2 câu.";
                _ui.EditorStatusText.color = RuntimeUiFactory.Orange;
                return;
            }

            if (index >= 0 && index < _editingQuestions.Count)
            {
                _editingQuestions.RemoveAt(index);
                RebuildEditorRows();
            }
        }

        private void CaptureEditorValues()
        {
            var title = _ui.LessonTitleInput.text.Trim();
            var questions = new List<LessonQuestionData>(_editorRows.Count);
            for (var i = 0; i < _editorRows.Count; i++)
            {
                var question = _editorRows[i].Build(_editingLesson.lessonId, title, i + 1);
                if (string.IsNullOrWhiteSpace(question.questionId))
                {
                    question.questionId = $"lesson-{_editingLesson.lessonId}-question-{i + 1}";
                }
                questions.Add(question);
            }
            _editingQuestions = questions;
        }

        private void SaveEditedQuestions()
        {
            if (_editingLesson == null)
            {
                return;
            }

            CaptureEditorValues();
            var title = _ui.LessonTitleInput.text.Trim();
            if (string.IsNullOrWhiteSpace(title))
            {
                _ui.EditorStatusText.text = "Tên bài không được để trống.";
                _ui.EditorStatusText.color = RuntimeUiFactory.Red;
                return;
            }

            var edited = new LessonDefinition
            {
                lessonId = _editingLesson.lessonId,
                title = title,
                status = LessonStatus.Ready,
                isDemo = _editingLesson.isDemo,
                summary = _editingLesson.isDemo
                    ? "DEMO — câu hỏi minh họa đã được giáo viên rà soát/chỉnh sửa."
                    : "Bộ câu hỏi đã được lưu trên thiết bị này.",
                defaultQuestionDurationSeconds = _editingLesson.defaultQuestionDurationSeconds,
                questions = _editingQuestions
            };
            if (!edited.ValidateQuestions(out var validationError))
            {
                _ui.EditorStatusText.text = validationError;
                _ui.EditorStatusText.color = RuntimeUiFactory.Red;
                return;
            }

            foreach (var question in edited.questions)
            {
                question.lessonId = edited.lessonId;
                question.lessonTitle = edited.title;
            }

            var index = _lessons.FindIndex(lesson => lesson.lessonId == edited.lessonId);
            if (index < 0)
            {
                _ui.EditorStatusText.text = "Không tìm thấy slot bài học cần cập nhật.";
                _ui.EditorStatusText.color = RuntimeUiFactory.Red;
                return;
            }

            _lessons[index] = edited;
            if (!_lessonRepository.SaveLessons(_lessons))
            {
                _ui.EditorStatusText.text = "Không lưu được JSON trong persistentDataPath.";
                _ui.EditorStatusText.color = RuntimeUiFactory.Red;
                return;
            }

            PopulateLessonDropdown();
            _ui.LessonDropdown.SetValueWithoutNotify(index);
            _ui.LessonDropdown.RefreshShownValue();
            UpdateLessonSummary();
            _ui.EditorStatusText.text = "Đã lưu vào persistentDataPath/ClassroomQuiz/ClassroomQuizLessons.json.";
            _ui.EditorStatusText.color = RuntimeUiFactory.Green;
            SetScreen(_ui.CreateRoomPanel);
        }

        private void SaveSettings()
        {
            var address = _ui.ServerAddressInput.text.Trim();
            var portText = _ui.ServerPortInput.text.Trim();
            if (string.IsNullOrWhiteSpace(address) || !int.TryParse(portText, out var port) || port < 1 || port > 65535)
            {
                SetStatus("Nhập địa chỉ server và cổng từ 1 đến 65535.", RuntimeUiFactory.Red);
                return;
            }

            PlayerPrefs.SetString("ClassroomQuiz.ServerAddress", address);
            PlayerPrefs.SetInt("ClassroomQuiz.ServerPort", port);
            PlayerPrefs.SetFloat("ClassroomQuiz.Volume", _ui.VolumeSlider.value);
            AudioListener.volume = _ui.VolumeSlider.value;
            PlayerPrefs.Save();
            SetStatus("Đã lưu IP, cổng và âm lượng.", RuntimeUiFactory.Green);
        }

        private QuizRoomPayload BuildRoomPayload(LessonDefinition lesson)
        {
            var payload = new QuizRoomPayload
            {
                schemaVersion = 1,
                lessonTitle = lesson.title,
                summary = lesson.summary,
                questions = new List<LessonQuestionData>(lesson.questions.Count)
            };
            foreach (var source in lesson.questions)
            {
                var question = CloneQuestion(source);
                question.lessonId = lesson.lessonId;
                question.lessonTitle = lesson.title;
                payload.questions.Add(question);
            }
            return payload;
        }

        private void CopyRoomCode()
        {
            GUIUtility.systemCopyBuffer = ClassroomQuizSessionState.RoomCode;
            _ui.WaitingStatusText.text = "Đã sao chép mã phòng.";
        }

        private void OnExitClicked()
        {
#if UNITY_EDITOR
            Debug.Log("Exit selected in the Unity Editor.");
#else
            Application.Quit();
#endif
        }

        private static int DurationForIndex(int selectedIndex)
        {
            var durations = new[] { 5, 10, 15, 20 };
            return durations[Mathf.Clamp(selectedIndex, 0, durations.Length - 1)];
        }

        private static LessonQuestionData CreateBlankQuestion(int lessonId, string lessonTitle, int index)
        {
            return new LessonQuestionData
            {
                lessonId = lessonId,
                lessonTitle = lessonTitle ?? string.Empty,
                questionId = $"lesson-{lessonId}-question-{index}",
                sequenceIndex = index,
                prompt = string.Empty,
                options = new List<string> { string.Empty, string.Empty, string.Empty, string.Empty },
                correctIndex = 0,
                explanation = string.Empty
            };
        }

        private static LessonQuestionData CloneQuestion(LessonQuestionData source)
        {
            return new LessonQuestionData
            {
                lessonId = source.lessonId,
                lessonTitle = source.lessonTitle,
                questionId = source.questionId,
                sequenceIndex = source.sequenceIndex,
                prompt = source.prompt,
                options = source.options == null ? new List<string>() : new List<string>(source.options),
                correctIndex = source.correctIndex,
                explanation = source.explanation
            };
        }
    }
}
