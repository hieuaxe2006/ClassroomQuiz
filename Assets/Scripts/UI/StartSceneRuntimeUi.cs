using System;
using System.Collections.Generic;
using ClassroomQuiz.Data;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClassroomQuiz.UI
{
    [System.Serializable]
    public sealed class StartSceneUiRefs
    {
        public Canvas Canvas;
        public GameObject MainPanel;
        public GameObject CharacterPanel;
        public GameObject LeaderboardPanel;
        public GameObject HistoryDetailPanel;
        public GameObject StudyPanel;
        public GameObject SettingsPanel;
        public GameObject RoomHubPanel;
        public GameObject CreateRoomPanel;
        public GameObject JoinRoomPanel;
        public GameObject WaitingRoomPanel;
        public GameObject ConnectingPanel;
        public GameObject QuestionEditorPanel;
        public TMP_Text WelcomeText;
        public TMP_Text MainStatusText;
        public TMP_Text HubStatusText;
        public TMP_Text SettingsStatusText;
        public TMP_Text CreateStatusText;
        public TMP_Text JoinStatusText;
        public TMP_Text WaitingStatusText;
        public TMP_Text LessonSummaryText;
        public TMP_Text RoomCodeText;
        public TMP_Text RosterText;
        public TMP_Text LeaderboardText;
        public TMP_Text HistoryStatusText;
        public TMP_Text HistoryColumnsText;
        public TMP_Text HistoryDetailTitle;
        public TMP_Text StudyDetailTitle;
        public TMP_Text StudyDetailText;
        public TMP_Text StudyStatusText;
        public TMP_Text ConnectingStatusText;
        public TMP_Text EditorStatusText;
        public TMP_Dropdown ModeDropdown;
        public TMP_Dropdown DurationDropdown;
        public TMP_Dropdown LessonDropdown;
        public TMP_Dropdown QuestionSetDropdown;
        public TMP_InputField RoomCodeInput;
        public TMP_InputField RoomNameInput;
        public TMP_InputField ServerAddressInput;
        public TMP_InputField ServerPortInput;
        public TMP_InputField LessonTitleInput;
        public Slider VolumeSlider;
        public Button CreateRoomButton;
        public Button JoinRoomButton;
        public Button SubmitCreateRoomButton;
        public Button SubmitJoinRoomButton;
        public Button EditQuestionsButton;
        public Button SaveSettingsButton;
        public Button StartGameButton;
        public Button QuestionEditorAddButton;
        public Button QuestionEditorSaveButton;
        public Button QuestionEditorCancelButton;
        public RectTransform RosterContent;
        public RectTransform QuestionEditorContent;
        public TMP_Text AvatarPreview;
        public Image ConnectingProgressFill;
        public Button PlayButton;
        public Button CharacterButton;
        public Button LeaderboardButton;
        public Button SettingsButton;
        public Button ExitButton;
        public Button CharacterBackButton;
        public Button CharacterConfirmButton;
        public Button[] AvatarChoiceButtons;
        public string[] AvatarChoices;
        public Button LeaderboardBackButton;
        public Button SettingsBackButton;
        public Button RoomHubBackButton;
        public Button CreateRoomHubButton;
        public Button FindRoomHubButton;
        public Button CreateBackButton;
        public Button JoinBackButton;
        public Button CopyRoomCodeButton;
        public Button WaitingLeaveButton;
        public Button ConnectingRetryButton;
        public Button ConnectingBackButton;
        public Button StudyButton;
        public Button StudyBackButton;
        public RectTransform HistoryContent;
        public RectTransform HistoryDetailsContent;
        public RectTransform StudyListContent;
    }

    public sealed class QuestionEditorRow
    {
        public TMP_InputField Prompt;
        public TMP_InputField Explanation;
        public TMP_Dropdown CorrectAnswer;
        public List<TMP_InputField> Options = new List<TMP_InputField>(4);
        public Button RemoveButton;

        public LessonQuestionData Build(int lessonId, string lessonTitle, int sequenceIndex)
        {
            var question = new LessonQuestionData
            {
                lessonId = lessonId,
                lessonTitle = lessonTitle,
                questionId = $"lesson-{lessonId}-question-{sequenceIndex}",
                sequenceIndex = sequenceIndex,
                prompt = Prompt.text.Trim(),
                correctIndex = CorrectAnswer.value,
                explanation = Explanation.text.Trim(),
                options = new List<string>(4)
            };
            foreach (var option in Options)
            {
                question.options.Add(option.text.Trim());
            }
            return question;
        }
    }

    public static class StartSceneRuntimeUi
    {
        public static StartSceneUiRefs Build(Transform owner)
        {
            var authoredUi = UnityEngine.Object.FindFirstObjectByType<StartSceneUiReferences>(FindObjectsInactive.Include);
            if (authoredUi != null && authoredUi.References != null)
            {
                return authoredUi.References;
            }

            var refs = new StartSceneUiRefs();
            refs.Canvas = RuntimeUiFactory.CreateCanvas("ClassroomQuiz Start Canvas");
            BuildMain(refs);
            BuildCharacter(refs);
            BuildLeaderboard(refs);
            BuildStudy(refs);
            BuildSettings(refs);
            BuildRoomHub(refs);
            BuildCreateRoom(refs);
            BuildJoinRoom(refs);
            BuildWaitingRoom(refs);
            BuildConnecting(refs);
            BuildQuestionEditor(refs);
            ShowOnly(refs, refs.MainPanel);
            refs.Canvas.gameObject.AddComponent<StartSceneUiReferences>().SetReferences(refs);
            return refs;
        }

        private static GameObject Screen(StartSceneUiRefs refs, string name, Color color)
        {
            var image = RuntimeUiFactory.Panel(refs.Canvas.transform, name, Vector2.zero, Vector2.zero, color);
            image.rectTransform.anchorMin = Vector2.zero;
            image.rectTransform.anchorMax = Vector2.one;
            image.rectTransform.offsetMin = Vector2.zero;
            image.rectTransform.offsetMax = Vector2.zero;
            return image.gameObject;
        }

        private static Image Card(Transform parent, string name, Vector2 size, Vector2 position)
        {
            var image = RuntimeUiFactory.Panel(parent, name, size, position, RuntimeUiFactory.Surface);
            image.raycastTarget = true;
            return image;
        }

        private static void BuildMain(StartSceneUiRefs refs)
        {
            refs.MainPanel = Screen(refs, "Main Menu", RuntimeUiFactory.Background);
            Card(refs.MainPanel.transform, "Main Card", new Vector2(910, 900), Vector2.zero);
            RuntimeUiFactory.Label(refs.MainPanel.transform, "Brand", "CLASSROOMQUIZ", new Vector2(740, 46), new Vector2(0, 365), 26, RuntimeUiFactory.Cyan, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Label(refs.MainPanel.transform, "Title", "Lớp học của bạn, cùng một nhịp", new Vector2(800, 74), new Vector2(0, 295), 42, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Label(refs.MainPanel.transform, "Subtitle", "Học tập tương tác qua TCP trong mạng LAN", new Vector2(780, 46), new Vector2(0, 244), 22, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            refs.WelcomeText = RuntimeUiFactory.Label(refs.MainPanel.transform, "Welcome", "Xin chào Guest", new Vector2(760, 44), new Vector2(0, 192), 22, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.PlayButton = RuntimeUiFactory.Button(refs.MainPanel.transform, "Play", "Chơi / Kết nối", new Vector2(660, 62), new Vector2(0, 142), RuntimeUiFactory.Blue, null);
            refs.StudyButton = RuntimeUiFactory.Button(refs.MainPanel.transform, "Study", "Học tập và ôn luyện", new Vector2(660, 62), new Vector2(0, 66), RuntimeUiFactory.Cyan, null);
            refs.CharacterButton = RuntimeUiFactory.Button(refs.MainPanel.transform, "Character", "Chọn nhân vật", new Vector2(660, 62), new Vector2(0, -10), RuntimeUiFactory.Purple, null);
            refs.LeaderboardButton = RuntimeUiFactory.Button(refs.MainPanel.transform, "Played Rooms", "Phòng đã chơi", new Vector2(660, 62), new Vector2(0, -86), RuntimeUiFactory.Purple, null);
            refs.SettingsButton = RuntimeUiFactory.Button(refs.MainPanel.transform, "Settings", "Cài đặt máy chủ", new Vector2(660, 62), new Vector2(0, -162), RuntimeUiFactory.Navy, null);
            refs.ExitButton = RuntimeUiFactory.Button(refs.MainPanel.transform, "Exit", "Thoát", new Vector2(660, 62), new Vector2(0, -238), RuntimeUiFactory.Red, null);
            refs.MainStatusText = RuntimeUiFactory.Label(refs.MainPanel.transform, "Status", "", new Vector2(800, 54), new Vector2(0, -350), 18, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
        }

        private static void BuildCharacter(StartSceneUiRefs refs)
        {
            refs.CharacterPanel = Screen(refs, "Character Screen", RuntimeUiFactory.Background);
            Card(refs.CharacterPanel.transform, "Card", new Vector2(1000, 720), Vector2.zero);
            RuntimeUiFactory.Label(refs.CharacterPanel.transform, "Title", "Chọn avatar", new Vector2(820, 72), new Vector2(0, 270), 42, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Label(refs.CharacterPanel.transform, "Hint", "Avatar được lưu vào tài khoản của bạn.", new Vector2(820, 44), new Vector2(0, 218), 20, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            refs.AvatarPreview = RuntimeUiFactory.Label(refs.CharacterPanel.transform, "Avatar Preview", "A", new Vector2(220, 190), new Vector2(0, 90), 96, RuntimeUiFactory.Blue, TextAlignmentOptions.Center, true);
            var avatars = new[] { "A", "B", "C", "D" };
            refs.AvatarChoices = avatars;
            refs.AvatarChoiceButtons = new Button[avatars.Length];
            for (var i = 0; i < avatars.Length; i++)
            {
                var avatar = avatars[i];
                var button = RuntimeUiFactory.Button(refs.CharacterPanel.transform, $"Avatar {avatar}", avatar, new Vector2(120, 70), new Vector2(-240 + i * 160, -50), RuntimeUiFactory.Purple, null, fontSize: 30);
                var outline = button.gameObject.AddComponent<Outline>();
                outline.effectColor = RuntimeUiFactory.Hex("39FF88");
                outline.effectDistance = new Vector2(5, -5);
                outline.enabled = false;
                refs.AvatarChoiceButtons[i] = button;
            }
            refs.CharacterConfirmButton = RuntimeUiFactory.Button(refs.CharacterPanel.transform, "Confirm Avatar", "Xác nhận nhân vật", new Vector2(360, 62), new Vector2(0, -178), RuntimeUiFactory.Green, null);
            refs.CharacterBackButton = RuntimeUiFactory.Button(refs.CharacterPanel.transform, "Back", "Quay lại", new Vector2(220, 54), new Vector2(-820, 478), RuntimeUiFactory.Navy, null, fontSize: 20);
        }

        private static void BuildLeaderboard(StartSceneUiRefs refs)
        {
            refs.LeaderboardPanel = Screen(refs, "Played Rooms", RuntimeUiFactory.Background);
            Card(refs.LeaderboardPanel.transform, "Card", new Vector2(1680, 930), Vector2.zero);
            RuntimeUiFactory.Label(refs.LeaderboardPanel.transform, "Title", "Phòng đã chơi", new Vector2(1400, 70), new Vector2(0, 410), 38, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.HistoryStatusText = RuntimeUiFactory.Label(refs.LeaderboardPanel.transform, "History Status", "", new Vector2(1420, 40), new Vector2(0, 360), 18, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            refs.HistoryColumnsText = RuntimeUiFactory.Label(refs.LeaderboardPanel.transform, "History Columns", "TÊN PHÒNG                         CHỦ PHÒNG                     MÃ THAM GIA                NGÀY TẠO                 HẠNG CỦA BẠN", new Vector2(1500, 48), new Vector2(0, 310), 17, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.HistoryContent = RuntimeUiFactory.ScrollContent(refs.LeaderboardPanel.transform, "Room History Scroll", new Vector2(1520, 700), new Vector2(0, -48), out _);
            refs.HistoryDetailPanel = new GameObject("Room History Detail", typeof(RectTransform));
            refs.HistoryDetailPanel.transform.SetParent(refs.LeaderboardPanel.transform, false);
            refs.HistoryDetailPanel.GetComponent<RectTransform>().sizeDelta = new Vector2(1560, 770);
            refs.HistoryDetailPanel.GetComponent<RectTransform>().anchoredPosition = new Vector2(0, -10);
            refs.HistoryDetailTitle = RuntimeUiFactory.Label(refs.HistoryDetailPanel.transform, "Room Detail Title", "Chi tiết phòng", new Vector2(1450, 56), new Vector2(0, 350), 28, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Label(refs.HistoryDetailPanel.transform, "Detail Columns", "TÊN HIỂN THỊ                       TOP                ĐÚNG / SAI / BỎ QUA                THỜI GIAN                ĐIỂM", new Vector2(1480, 46), new Vector2(0, 300), 17, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.HistoryDetailsContent = RuntimeUiFactory.ScrollContent(refs.HistoryDetailPanel.transform, "Room Detail Scroll", new Vector2(1500, 650), new Vector2(0, -35), out _);
            refs.HistoryDetailPanel.SetActive(false);
            refs.LeaderboardBackButton = RuntimeUiFactory.Button(refs.LeaderboardPanel.transform, "Back", "Quay lại", new Vector2(220, 54), new Vector2(-820, 478), RuntimeUiFactory.Navy, null, fontSize: 20);
        }

        private static void BuildStudy(StartSceneUiRefs refs)
        {
            refs.StudyPanel = Screen(refs, "Study Library", RuntimeUiFactory.Background);
            Card(refs.StudyPanel.transform, "Study Card", new Vector2(1680, 930), Vector2.zero);
            RuntimeUiFactory.Label(refs.StudyPanel.transform, "Study Title", "Thư viện học tập", new Vector2(1400, 70), new Vector2(0, 410), 38, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.StudyStatusText = RuntimeUiFactory.Label(refs.StudyPanel.transform, "Study Status", "Không giới hạn thời gian. Các ví dụ ở đây tách riêng khỏi câu hỏi trong phòng.", new Vector2(1420, 40), new Vector2(0, 360), 18, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            RuntimeUiFactory.Label(refs.StudyPanel.transform, "Study List Heading", "CHỦ ĐỀ", new Vector2(430, 42), new Vector2(-570, 315), 19, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.StudyListContent = RuntimeUiFactory.ScrollContent(refs.StudyPanel.transform, "Study Topics Scroll", new Vector2(500, 700), new Vector2(-570, -48), out _);
            Card(refs.StudyPanel.transform, "Study Article Card", new Vector2(990, 748), new Vector2(315, -38));
            refs.StudyDetailTitle = RuntimeUiFactory.Label(refs.StudyPanel.transform, "Study Article Title", "Chọn một chủ đề để học", new Vector2(900, 64), new Vector2(315, 292), 28, RuntimeUiFactory.Blue, TextAlignmentOptions.Center, true);
            refs.StudyDetailText = RuntimeUiFactory.Label(refs.StudyPanel.transform, "Study Article Text", "", new Vector2(870, 610), new Vector2(315, -28), 23, RuntimeUiFactory.Navy, TextAlignmentOptions.TopLeft);
            refs.StudyDetailText.overflowMode = TextOverflowModes.Overflow;
            refs.StudyBackButton = RuntimeUiFactory.Button(refs.StudyPanel.transform, "Back", "Quay lại", new Vector2(220, 54), new Vector2(-820, 478), RuntimeUiFactory.Navy, null, fontSize: 20);
        }

        private static void BuildSettings(StartSceneUiRefs refs)
        {
            refs.SettingsPanel = Screen(refs, "Settings Screen", RuntimeUiFactory.Background);
            Card(refs.SettingsPanel.transform, "Card", new Vector2(1040, 800), Vector2.zero);
            RuntimeUiFactory.Label(refs.SettingsPanel.transform, "Title", "Cài đặt kết nối", new Vector2(900, 70), new Vector2(0, 310), 40, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Label(refs.SettingsPanel.transform, "Address Label", "Địa chỉ server", new Vector2(360, 40), new Vector2(-230, 225), 20, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.ServerAddressInput = RuntimeUiFactory.Input(refs.SettingsPanel.transform, "Server Address", "127.0.0.1", new Vector2(500, 58), new Vector2(170, 225));
            RuntimeUiFactory.Label(refs.SettingsPanel.transform, "Port Label", "Cổng TCP", new Vector2(360, 40), new Vector2(-230, 140), 20, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.ServerPortInput = RuntimeUiFactory.Input(refs.SettingsPanel.transform, "Server Port", "7777", new Vector2(500, 58), new Vector2(170, 140), TMP_InputField.ContentType.IntegerNumber);
            RuntimeUiFactory.Label(refs.SettingsPanel.transform, "Volume Label", "Âm lượng", new Vector2(360, 40), new Vector2(-230, 48), 20, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.VolumeSlider = RuntimeUiFactory.Slider(refs.SettingsPanel.transform, "Volume Slider", new Vector2(500, 40), new Vector2(170, 48), 0, 1, 1);
            RuntimeUiFactory.Label(refs.SettingsPanel.transform, "Lan Note", "Mặc định 127.0.0.1. Máy khác trong LAN cần dùng IPv4 của máy chạy server.", new Vector2(820, 86), new Vector2(0, -50), 18, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            refs.SettingsStatusText = RuntimeUiFactory.Label(refs.SettingsPanel.transform, "Settings Status", "", new Vector2(820, 42), new Vector2(0, -112), 18, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            refs.SaveSettingsButton = RuntimeUiFactory.Button(refs.SettingsPanel.transform, "Save Settings", "Lưu cài đặt", new Vector2(320, 62), new Vector2(-180, -175), RuntimeUiFactory.Blue, null);
            refs.SettingsBackButton = RuntimeUiFactory.Button(refs.SettingsPanel.transform, "Back", "Quay lại", new Vector2(220, 54), new Vector2(-820, 478), RuntimeUiFactory.Navy, null, fontSize: 20);
        }

        private static void BuildRoomHub(StartSceneUiRefs refs)
        {
            refs.RoomHubPanel = Screen(refs, "Room Hub", RuntimeUiFactory.Background);
            Card(refs.RoomHubPanel.transform, "Card", new Vector2(1040, 760), Vector2.zero);
            RuntimeUiFactory.Label(refs.RoomHubPanel.transform, "Title", "Chọn cách tham gia", new Vector2(900, 72), new Vector2(0, 270), 40, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Label(refs.RoomHubPanel.transform, "Connected", "Đã kết nối TCP với server", new Vector2(800, 44), new Vector2(0, 210), 20, RuntimeUiFactory.Green, TextAlignmentOptions.Center, true);
            refs.CreateRoomHubButton = RuntimeUiFactory.Button(refs.RoomHubPanel.transform, "Create Room", "Tạo phòng mới", new Vector2(700, 82), new Vector2(0, 90), RuntimeUiFactory.Blue, null, fontSize: 30);
            refs.FindRoomHubButton = RuntimeUiFactory.Button(refs.RoomHubPanel.transform, "Find Room", "Tìm / tham gia phòng", new Vector2(700, 82), new Vector2(0, -18), RuntimeUiFactory.Cyan, null, fontSize: 30);
            refs.RoomHubBackButton = RuntimeUiFactory.Button(refs.RoomHubPanel.transform, "Back To Menu", "Quay lại", new Vector2(220, 54), new Vector2(-820, 478), RuntimeUiFactory.Navy, null, fontSize: 20);
            refs.HubStatusText = RuntimeUiFactory.Label(refs.RoomHubPanel.transform, "Status", "", new Vector2(880, 65), new Vector2(0, -260), 18, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
        }

        private static void BuildCreateRoom(StartSceneUiRefs refs)
        {
            refs.CreateRoomPanel = Screen(refs, "Create Room", RuntimeUiFactory.Background);
            Card(refs.CreateRoomPanel.transform, "Card", new Vector2(1480, 900), Vector2.zero);
            RuntimeUiFactory.Label(refs.CreateRoomPanel.transform, "Title", "Tạo phòng học", new Vector2(1300, 66), new Vector2(0, 382), 38, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            var left = Card(refs.CreateRoomPanel.transform, "Left Column", new Vector2(620, 550), new Vector2(-330, 50));
            var right = Card(refs.CreateRoomPanel.transform, "Right Column", new Vector2(620, 550), new Vector2(330, 50));
            left.color = RuntimeUiFactory.Hex("F8FBFE");
            right.color = RuntimeUiFactory.Hex("F8FBFE");

            refs.RoomNameInput = RuntimeUiFactory.Input(left.transform, "Room Name", "Tên phòng (mặc định theo bài học)", new Vector2(500, 54), new Vector2(0, 255));
            RuntimeUiFactory.Label(left.transform, "Mode Label", "Chế độ", new Vector2(500, 40), new Vector2(0, 190), 21, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.ModeDropdown = RuntimeUiFactory.Dropdown(left.transform, "Mode Dropdown", new[] { "Học", "Thi" }, new Vector2(500, 58), new Vector2(0, 142));
            RuntimeUiFactory.Label(left.transform, "Duration Label", "Thời gian mỗi câu", new Vector2(500, 40), new Vector2(0, 52), 21, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.DurationDropdown = RuntimeUiFactory.Dropdown(left.transform, "Duration Dropdown", new[] { "5 giây", "10 giây", "15 giây", "20 giây" }, new Vector2(500, 58), new Vector2(0, 4), 2);
            RuntimeUiFactory.Label(left.transform, "Teacher Note", "Giáo viên tạo phòng sẽ là người điều khiển buổi học.", new Vector2(500, 100), new Vector2(0, -112), 19, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);

            RuntimeUiFactory.Label(right.transform, "Lesson Label", "Bài 1–42", new Vector2(500, 40), new Vector2(0, 190), 21, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.LessonDropdown = RuntimeUiFactory.Dropdown(right.transform, "Lesson Dropdown", new[] { "Đang tải bài học…" }, new Vector2(500, 58), new Vector2(0, 142));
            RuntimeUiFactory.Label(right.transform, "Question Set Label", "Bộ câu hỏi đã lưu", new Vector2(500, 40), new Vector2(0, 52), 21, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.QuestionSetDropdown = RuntimeUiFactory.Dropdown(right.transform, "Question Set Dropdown", new[] { "Bộ câu hỏi của bài" }, new Vector2(500, 58), new Vector2(0, 4));
            refs.EditQuestionsButton = RuntimeUiFactory.Button(right.transform, "Edit Questions", "Chỉnh sửa câu hỏi", new Vector2(500, 56), new Vector2(0, -82), RuntimeUiFactory.Purple, null, fontSize: 22);
            refs.LessonSummaryText = RuntimeUiFactory.Label(refs.CreateRoomPanel.transform, "Lesson Summary", "", new Vector2(1200, 60), new Vector2(0, -280), 20, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            refs.CreateStatusText = RuntimeUiFactory.Label(refs.CreateRoomPanel.transform, "Status", "", new Vector2(1200, 54), new Vector2(0, -330), 18, RuntimeUiFactory.Orange, TextAlignmentOptions.Center);
            refs.SubmitCreateRoomButton = RuntimeUiFactory.Button(refs.CreateRoomPanel.transform, "Create", "Tạo phòng", new Vector2(340, 62), new Vector2(210, -390), RuntimeUiFactory.Blue, null);
            refs.CreateBackButton = RuntimeUiFactory.Button(refs.CreateRoomPanel.transform, "Back", "Quay lại", new Vector2(220, 54), new Vector2(-820, 478), RuntimeUiFactory.Navy, null, fontSize: 20);
        }

        private static void BuildJoinRoom(StartSceneUiRefs refs)
        {
            refs.JoinRoomPanel = Screen(refs, "Join Room", RuntimeUiFactory.Background);
            Card(refs.JoinRoomPanel.transform, "Card", new Vector2(1040, 700), Vector2.zero);
            RuntimeUiFactory.Label(refs.JoinRoomPanel.transform, "Title", "Tham gia phòng", new Vector2(900, 70), new Vector2(0, 250), 40, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Label(refs.JoinRoomPanel.transform, "Room Code Label", "Nhập mã phòng 6 ký tự", new Vector2(750, 50), new Vector2(0, 140), 24, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.RoomCodeInput = RuntimeUiFactory.Input(refs.JoinRoomPanel.transform, "Room Code", "Ví dụ: ABC234", new Vector2(640, 72), new Vector2(0, 62));
            refs.SubmitJoinRoomButton = RuntimeUiFactory.Button(refs.JoinRoomPanel.transform, "Join", "Tham gia", new Vector2(300, 64), new Vector2(170, -65), RuntimeUiFactory.Cyan, null);
            refs.JoinBackButton = RuntimeUiFactory.Button(refs.JoinRoomPanel.transform, "Back", "Quay lại", new Vector2(220, 54), new Vector2(-820, 478), RuntimeUiFactory.Navy, null, fontSize: 20);
            refs.JoinStatusText = RuntimeUiFactory.Label(refs.JoinRoomPanel.transform, "Status", "", new Vector2(850, 70), new Vector2(0, -180), 18, RuntimeUiFactory.Orange, TextAlignmentOptions.Center);
        }

        private static void BuildWaitingRoom(StartSceneUiRefs refs)
        {
            refs.WaitingRoomPanel = Screen(refs, "Waiting Room", RuntimeUiFactory.Background);
            Card(refs.WaitingRoomPanel.transform, "Card", new Vector2(1120, 880), Vector2.zero);
            RuntimeUiFactory.Label(refs.WaitingRoomPanel.transform, "Title", "Phòng chờ", new Vector2(900, 62), new Vector2(0, 365), 38, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.RoomCodeText = RuntimeUiFactory.Label(refs.WaitingRoomPanel.transform, "Room Code", "------", new Vector2(620, 100), new Vector2(0, 275), 64, RuntimeUiFactory.Blue, TextAlignmentOptions.Center, true);
            refs.CopyRoomCodeButton = RuntimeUiFactory.Button(refs.WaitingRoomPanel.transform, "Copy Code", "Sao chép mã", new Vector2(250, 54), new Vector2(0, 205), RuntimeUiFactory.Cyan, null, fontSize: 20);
            RuntimeUiFactory.Label(refs.WaitingRoomPanel.transform, "Roster Heading", "Danh sách người tham gia", new Vector2(800, 52), new Vector2(0, 145), 24, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.RosterContent = RuntimeUiFactory.ScrollContent(refs.WaitingRoomPanel.transform, "Roster Scroll", new Vector2(820, 390), new Vector2(0, -62), out _);
            refs.RosterText = RuntimeUiFactory.Label(refs.RosterContent, "Roster", "Đang tải danh sách…", new Vector2(760, 140), Vector2.zero, 21, RuntimeUiFactory.Navy, TextAlignmentOptions.TopLeft);
            refs.StartGameButton = RuntimeUiFactory.Button(refs.WaitingRoomPanel.transform, "Start Game", "Bắt đầu", new Vector2(330, 64), new Vector2(200, -340), RuntimeUiFactory.Green, null);
            refs.WaitingLeaveButton = RuntimeUiFactory.Button(refs.WaitingRoomPanel.transform, "Leave Room", "Rời phòng", new Vector2(220, 54), new Vector2(-820, 478), RuntimeUiFactory.Navy, null, fontSize: 20);
            refs.WaitingStatusText = RuntimeUiFactory.Label(refs.WaitingRoomPanel.transform, "Status", "", new Vector2(880, 46), new Vector2(0, -400), 18, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
        }

        private static void BuildConnecting(StartSceneUiRefs refs)
        {
            refs.ConnectingPanel = Screen(refs, "Connecting Overlay", new Color(0.06f, 0.13f, 0.22f, 0.52f));
            var card = Card(refs.ConnectingPanel.transform, "Connecting Card", new Vector2(680, 340), Vector2.zero);
            refs.ConnectingStatusText = RuntimeUiFactory.Label(card.transform, "Status", "Connecting…", new Vector2(600, 76), new Vector2(0, 72), 28, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.ConnectingProgressFill = RuntimeUiFactory.ProgressBar(card.transform, "Progress", new Vector2(500, 20), new Vector2(0, -25), RuntimeUiFactory.Cyan);
            refs.ConnectingRetryButton = RuntimeUiFactory.Button(card.transform, "Retry", "Thử lại", new Vector2(220, 54), new Vector2(-120, -110), RuntimeUiFactory.Blue, null, fontSize: 21);
            refs.ConnectingBackButton = RuntimeUiFactory.Button(card.transform, "Back", "Quay lại", new Vector2(200, 50), new Vector2(-220, 120), RuntimeUiFactory.Navy, null, fontSize: 20);
            refs.ConnectingPanel.SetActive(false);
        }

        private static void BuildQuestionEditor(StartSceneUiRefs refs)
        {
            refs.QuestionEditorPanel = Screen(refs, "Question Editor", RuntimeUiFactory.Hex("EAF1F7"));
            Card(refs.QuestionEditorPanel.transform, "Card", new Vector2(1500, 1000), Vector2.zero);
            RuntimeUiFactory.Label(refs.QuestionEditorPanel.transform, "Title", "Chỉnh sửa bộ câu hỏi", new Vector2(1320, 62), new Vector2(0, 440), 36, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Label(refs.QuestionEditorPanel.transform, "Lesson Name Label", "Tên bài", new Vector2(230, 50), new Vector2(-510, 376), 20, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.LessonTitleInput = RuntimeUiFactory.Input(refs.QuestionEditorPanel.transform, "Lesson Title", "Tên bài học", new Vector2(850, 54), new Vector2(90, 376));
            refs.QuestionEditorContent = RuntimeUiFactory.ScrollContent(refs.QuestionEditorPanel.transform, "Question Editor Scroll", new Vector2(1320, 650), new Vector2(0, 18), out _);
            refs.QuestionEditorAddButton = RuntimeUiFactory.Button(refs.QuestionEditorPanel.transform, "Add Question", "+ Thêm câu", new Vector2(270, 55), new Vector2(-460, -380), RuntimeUiFactory.Purple, null, fontSize: 21);
            refs.EditorStatusText = RuntimeUiFactory.Label(refs.QuestionEditorPanel.transform, "Editor Status", "", new Vector2(720, 54), new Vector2(0, -380), 17, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            refs.QuestionEditorSaveButton = RuntimeUiFactory.Button(refs.QuestionEditorPanel.transform, "Save Questions", "Lưu", new Vector2(210, 55), new Vector2(350, -380), RuntimeUiFactory.Green, null, fontSize: 21);
            refs.QuestionEditorCancelButton = RuntimeUiFactory.Button(refs.QuestionEditorPanel.transform, "Cancel Questions", "Quay lại", new Vector2(220, 54), new Vector2(-820, 478), RuntimeUiFactory.Navy, null, fontSize: 20);
        }

        public static QuestionEditorRow BuildQuestionRow(RectTransform content, LessonQuestionData question, int index, Action onRemove)
        {
            var card = RuntimeUiFactory.Rect(content, $"Question {index + 1}", new Vector2(1280, 320), Vector2.zero);
            card.gameObject.AddComponent<Image>().color = RuntimeUiFactory.Hex("F8FBFE");
            card.gameObject.AddComponent<LayoutElement>().preferredHeight = 320;
            var row = new QuestionEditorRow();
            RuntimeUiFactory.Label(card, "Question Number", $"Câu {index + 1}", new Vector2(220, 42), new Vector2(-500, 132), 20, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            row.RemoveButton = RuntimeUiFactory.Button(card, "Remove", "Xóa", new Vector2(110, 38), new Vector2(535, 132), RuntimeUiFactory.Red, () => onRemove?.Invoke(), fontSize: 18);
            row.Prompt = RuntimeUiFactory.Input(card, "Prompt", "Nội dung câu hỏi", new Vector2(1160, 54), new Vector2(0, 82));
            row.Prompt.text = question != null ? question.prompt : string.Empty;
            for (var optionIndex = 0; optionIndex < 4; optionIndex++)
            {
                var column = optionIndex % 2;
                var rowIndex = optionIndex / 2;
                var position = new Vector2(column == 0 ? -300 : 300, 20 - rowIndex * 62);
                var option = RuntimeUiFactory.Input(card, $"Option {optionIndex + 1}", $"Đáp án {(char)('A' + optionIndex)}", new Vector2(550, 50), position);
                option.text = question != null && question.options != null && optionIndex < question.options.Count ? question.options[optionIndex] : string.Empty;
                row.Options.Add(option);
            }
            RuntimeUiFactory.Label(card, "Correct Label", "Đáp án đúng", new Vector2(180, 42), new Vector2(-425, -98), 18, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            row.CorrectAnswer = RuntimeUiFactory.Dropdown(card, "Correct Answer", new[] { "A", "B", "C", "D" }, new Vector2(170, 46), new Vector2(-280, -98), question != null ? question.correctIndex : 0);
            row.Explanation = RuntimeUiFactory.Input(card, "Explanation", "Giải thích", new Vector2(700, 50), new Vector2(260, -98));
            row.Explanation.text = question != null ? question.explanation : string.Empty;
            return row;
        }

        public static void ShowOnly(StartSceneUiRefs refs, GameObject panel)
        {
            RuntimeUiFactory.SetVisible(refs.MainPanel, refs.MainPanel == panel);
            RuntimeUiFactory.SetVisible(refs.CharacterPanel, refs.CharacterPanel == panel);
            RuntimeUiFactory.SetVisible(refs.LeaderboardPanel, refs.LeaderboardPanel == panel);
            RuntimeUiFactory.SetVisible(refs.StudyPanel, refs.StudyPanel == panel);
            RuntimeUiFactory.SetVisible(refs.SettingsPanel, refs.SettingsPanel == panel);
            RuntimeUiFactory.SetVisible(refs.RoomHubPanel, refs.RoomHubPanel == panel);
            RuntimeUiFactory.SetVisible(refs.CreateRoomPanel, refs.CreateRoomPanel == panel);
            RuntimeUiFactory.SetVisible(refs.JoinRoomPanel, refs.JoinRoomPanel == panel);
            RuntimeUiFactory.SetVisible(refs.WaitingRoomPanel, refs.WaitingRoomPanel == panel);
            RuntimeUiFactory.SetVisible(refs.QuestionEditorPanel, refs.QuestionEditorPanel == panel);
            RuntimeUiFactory.SetVisible(refs.ConnectingPanel, false);
        }
    }
}
