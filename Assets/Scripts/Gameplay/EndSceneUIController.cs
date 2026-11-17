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
        private sealed class EndUiRefs
        {
            public TMP_Text Title;
            public TMP_Text Results;
            public TMP_Text Status;
            public Button Restart;
            public Button EndSession;
            public Button Leave;
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

        private EndUiRefs _ui;
        private TcpGameClient _client;
        private bool _endingSession;

        private void Awake()
        {
            _client = GetOrCreateTcpClient();
            _ui = BuildUi();
            var isTeacher = ClassroomQuizSessionState.IsTeacher;
            _ui.Title.text = isTeacher ? "Kết quả buổi học" : "Kết quả của bạn";
            _ui.Results.text = isTeacher
                ? (string.IsNullOrWhiteSpace(ClassroomQuizSessionState.LastTeacherLeaderboard)
                    ? "Đang chờ bảng xếp hạng từ server…"
                    : ClassroomQuizSessionState.LastTeacherLeaderboard)
                : ClassroomQuizSessionState.LastPersonalResult;
            _ui.EndSession.gameObject.SetActive(isTeacher);
            _ui.Restart.gameObject.SetActive(isTeacher);
            _ui.Leave.gameObject.SetActive(!isTeacher);
            _ui.Status.text = isTeacher
                ? "Kết thúc buổi học để đưa tất cả người chơi về StartScene."
                : "Kết quả này chỉ hiển thị cho bạn. Đang chờ giáo viên kết thúc buổi học.";
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

        private EndUiRefs BuildUi()
        {
            var refs = new EndUiRefs();
            var canvas = RuntimeUiFactory.CreateCanvas("ClassroomQuiz Results Canvas");
            var background = RuntimeUiFactory.Panel(canvas.transform, "Background", Vector2.zero, Vector2.zero, RuntimeUiFactory.Background);
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.sizeDelta = Vector2.zero;
            RuntimeUiFactory.Panel(background.transform, "Card", new Vector2(1380, 900), Vector2.zero, RuntimeUiFactory.Surface);
            refs.Title = RuntimeUiFactory.Label(background.transform, "Title", "Kết quả", new Vector2(1220, 70), new Vector2(0, 380), 42, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.Results = RuntimeUiFactory.Label(background.transform, "Results", "", new Vector2(1180, 560), new Vector2(0, 25), 27, RuntimeUiFactory.Navy, TextAlignmentOptions.TopLeft);
            refs.Status = RuntimeUiFactory.Label(background.transform, "Status", "", new Vector2(1180, 64), new Vector2(0, -315), 18, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            refs.Restart = RuntimeUiFactory.Button(background.transform, "Restart Room", "Chơi lại", new Vector2(300, 66), new Vector2(-190, -390), RuntimeUiFactory.Green, null, fontSize: 22);
            refs.EndSession = RuntimeUiFactory.Button(background.transform, "End Session", "Kết thúc buổi học", new Vector2(360, 66), new Vector2(230, -390), RuntimeUiFactory.Red, null, fontSize: 22);
            refs.Leave = RuntimeUiFactory.Button(background.transform, "Leave", "Quay về menu", new Vector2(220, 54), new Vector2(-820, 478), RuntimeUiFactory.Navy, null, fontSize: 20);
            return refs;
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
            if (message.Type == "ReturnToMenu")
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
