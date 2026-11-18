using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClassroomQuiz.UI
{
    [Serializable]
    public sealed class ClassroomSeatUiRefs
    {
        public GameObject Root;
        public TMP_Text Avatar;
        public TMP_Text Name;
        public TMP_Text Points;
    }

    [Serializable]
    public sealed class GameplayUiRefs
    {
        public Canvas Canvas;
        public TMP_Text LessonTitle;
        public TMP_Text QuestionCount;
        public TMP_Text ModeLabel;
        public TMP_Text TimerText;
        public TMP_Text StudentScoreText;
        public TMP_Text Prompt;
        public TMP_Text Status;
        public TMP_Text CountdownText;
        public TMP_Text CountdownSubtext;
        public TMP_Text FeedbackTitle;
        public TMP_Text FeedbackBody;
        public TMP_Text TeacherAvatar;
        public TMP_Text TeacherName;
        public TMP_Text TeacherQuestionText;
        public TMP_Text TeacherTimerText;
        public TMP_Text TeacherStatusText;
        public TMP_Text WinnerAvatar;
        public TMP_Text WinnerName;
        public GameObject CountdownOverlay;
        public GameObject FeedbackPanel;
        public GameObject StudentPanel;
        public GameObject TeacherPanel;
        public GameObject WinnerOverlay;
        public RectTransform TeacherLeaderboardContent;
        public Image ProgressFill;
        public Button[] Answers;
        public TMP_Text[] AnswerLabels;
        public ClassroomSeatUiRefs[] StudentSeats;
    }

    public static class GameplayRuntimeUi
    {
        private static readonly float[] SeatX = { -355, -118, 118, 355 };
        private static readonly float[] SeatY = { 160, -15, -190, -365 };

        public static GameplayUiRefs Build()
        {
            var authoredUi = UnityEngine.Object.FindFirstObjectByType<GameplayUiReferences>(FindObjectsInactive.Include);
            if (authoredUi != null && authoredUi.References != null)
            {
                return authoredUi.References;
            }

            var refs = new GameplayUiRefs();
            refs.Canvas = RuntimeUiFactory.CreateCanvas("ClassroomQuiz Gameplay Canvas");
            var background = RuntimeUiFactory.Panel(refs.Canvas.transform, "Gameplay Background", Vector2.zero, Vector2.zero, RuntimeUiFactory.Background);
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.offsetMin = Vector2.zero;
            background.rectTransform.offsetMax = Vector2.zero;

            var classroom = RuntimeUiFactory.Panel(background.transform, "Classroom Half", Vector2.zero, Vector2.zero, RuntimeUiFactory.Hex("E8F4FA"));
            classroom.rectTransform.anchorMin = Vector2.zero;
            classroom.rectTransform.anchorMax = new Vector2(0.5f, 1);
            classroom.rectTransform.offsetMin = Vector2.zero;
            classroom.rectTransform.offsetMax = Vector2.zero;
            BuildClassroom(classroom.transform, refs);

            var gameHalf = RuntimeUiFactory.Panel(background.transform, "Question Half", Vector2.zero, Vector2.zero, RuntimeUiFactory.Hex("F3F7FB"));
            gameHalf.rectTransform.anchorMin = new Vector2(0.5f, 0);
            gameHalf.rectTransform.anchorMax = Vector2.one;
            gameHalf.rectTransform.offsetMin = Vector2.zero;
            gameHalf.rectTransform.offsetMax = Vector2.zero;
            BuildStudentQuestionUi(gameHalf.transform, refs);
            BuildTeacherDashboard(gameHalf.transform, refs);

            BuildCountdownAndWinnerOverlays(background.transform, refs);
            refs.Canvas.gameObject.AddComponent<GameplayUiReferences>().SetReferences(refs);
            return refs;
        }

        private static void BuildClassroom(Transform parent, GameplayUiRefs refs)
        {
            var rearWall = RuntimeUiFactory.Panel(parent, "Rear Wall", new Vector2(930, 470), new Vector2(0, 300), RuntimeUiFactory.Hex("D9EFF6"));
            rearWall.raycastTarget = false;
            RuntimeUiFactory.Panel(parent, "Floor", new Vector2(960, 400), new Vector2(0, -345), RuntimeUiFactory.Hex("B8DFF0")).raycastTarget = false;
            RuntimeUiFactory.Panel(parent, "Front Board Frame", new Vector2(500, 104), new Vector2(0, 425), RuntimeUiFactory.Hex("A66B42")).raycastTarget = false;
            RuntimeUiFactory.Panel(parent, "Classroom Board", new Vector2(470, 78), new Vector2(0, 425), RuntimeUiFactory.Hex("248D69")).raycastTarget = false;
            RuntimeUiFactory.Label(parent, "Board Title", "LỚP HỌC TOÁN", new Vector2(420, 42), new Vector2(0, 425), 25, Color.white, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Panel(parent, "Left Window", new Vector2(120, 170), new Vector2(-390, 402), RuntimeUiFactory.Hex("A8DBF1")).raycastTarget = false;
            RuntimeUiFactory.Panel(parent, "Right Window", new Vector2(120, 170), new Vector2(390, 402), RuntimeUiFactory.Hex("A8DBF1")).raycastTarget = false;

            RuntimeUiFactory.Label(parent, "Classroom Room Label", "PHÒNG HỌC  ·  16 CHỖ", new Vector2(850, 40), new Vector2(0, 515), 22, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);

            var teacherDesk = RuntimeUiFactory.Panel(parent, "Teacher Desk", new Vector2(272, 82), new Vector2(0, 313), RuntimeUiFactory.Hex("B97D4C"));
            teacherDesk.raycastTarget = false;
            refs.TeacherAvatar = RuntimeUiFactory.Label(parent, "Teacher Avatar", "A", new Vector2(68, 68), new Vector2(-64, 328), 36, RuntimeUiFactory.Blue, TextAlignmentOptions.Center, true);
            refs.TeacherName = RuntimeUiFactory.Label(parent, "Teacher Name", "Giáo viên", new Vector2(190, 36), new Vector2(66, 328), 17, Color.white, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Label(parent, "Teacher Desk Caption", "BÀN GIÁO VIÊN", new Vector2(260, 30), new Vector2(0, 257), 15, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);

            refs.StudentSeats = new ClassroomSeatUiRefs[16];
            for (var index = 0; index < refs.StudentSeats.Length; index++)
            {
                var seat = new ClassroomSeatUiRefs();
                var row = index / 4;
                var column = index % 4;
                var cell = new GameObject($"Student Desk {index + 1}", typeof(RectTransform));
                cell.transform.SetParent(parent, false);
                var cellRect = cell.GetComponent<RectTransform>();
                cellRect.anchorMin = cellRect.anchorMax = new Vector2(0.5f, 0.5f);
                cellRect.sizeDelta = new Vector2(206, 154);
                cellRect.anchoredPosition = new Vector2(SeatX[column], SeatY[row]);
                seat.Root = cell;

                RuntimeUiFactory.Panel(cell.transform, "Chair", new Vector2(92, 22), new Vector2(0, -52), RuntimeUiFactory.Hex("844F43")).raycastTarget = false;
                RuntimeUiFactory.Panel(cell.transform, "Desk", new Vector2(184, 68), new Vector2(0, -5), RuntimeUiFactory.Hex("D39A62")).raycastTarget = false;
                var avatarPlate = RuntimeUiFactory.Panel(cell.transform, "Avatar Plate", new Vector2(48, 48), new Vector2(0, 24), RuntimeUiFactory.Hex("EAF6FF"));
                avatarPlate.raycastTarget = false;
                seat.Avatar = RuntimeUiFactory.Label(cell.transform, "Avatar Letter", "—", new Vector2(45, 42), new Vector2(0, 24), 24, RuntimeUiFactory.Blue, TextAlignmentOptions.Center, true);
                seat.Name = RuntimeUiFactory.Label(cell.transform, "Student Name", "Chỗ trống", new Vector2(178, 32), new Vector2(0, -28), 14, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
                seat.Points = RuntimeUiFactory.Label(cell.transform, "Points Popup", "", new Vector2(190, 36), new Vector2(0, 68), 22, RuntimeUiFactory.Green, TextAlignmentOptions.Center, true);
                seat.Points.gameObject.SetActive(false);
                refs.StudentSeats[index] = seat;
            }
        }

        private static void BuildStudentQuestionUi(Transform parent, GameplayUiRefs refs)
        {
            var studentPanel = RuntimeUiFactory.Panel(parent, "Student Question Interface", Vector2.zero, Vector2.zero, RuntimeUiFactory.Hex("F3F7FB"));
            studentPanel.rectTransform.anchorMin = Vector2.zero;
            studentPanel.rectTransform.anchorMax = Vector2.one;
            studentPanel.rectTransform.offsetMin = Vector2.zero;
            studentPanel.rectTransform.offsetMax = Vector2.zero;
            refs.StudentPanel = studentPanel.gameObject;

            var header = RuntimeUiFactory.Panel(studentPanel.transform, "Question Header", new Vector2(930, 128), new Vector2(0, 458), RuntimeUiFactory.Surface);
            refs.LessonTitle = RuntimeUiFactory.Label(header.transform, "Lesson Title", "Toán lớp 6", new Vector2(610, 42), new Vector2(-130, 34), 23, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.QuestionCount = RuntimeUiFactory.Label(header.transform, "Question Count", "Câu 0 / 0", new Vector2(200, 42), new Vector2(315, 34), 21, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.ModeLabel = RuntimeUiFactory.Label(header.transform, "Mode", "HỌC", new Vector2(150, 34), new Vector2(-356, -14), 17, RuntimeUiFactory.Purple, TextAlignmentOptions.Center, true);
            refs.TimerText = RuntimeUiFactory.Label(header.transform, "Timer", "--", new Vector2(110, 42), new Vector2(340, -14), 25, RuntimeUiFactory.Blue, TextAlignmentOptions.Center, true);
            refs.StudentScoreText = RuntimeUiFactory.Label(header.transform, "Student Score", "Điểm: 0", new Vector2(200, 40), new Vector2(120, -14), 21, RuntimeUiFactory.Green, TextAlignmentOptions.Center, true);
            refs.ProgressFill = RuntimeUiFactory.ProgressBar(header.transform, "Progress", new Vector2(870, 10), new Vector2(0, -51), RuntimeUiFactory.Cyan);

            var promptCard = RuntimeUiFactory.Panel(studentPanel.transform, "Question Card", new Vector2(900, 264), new Vector2(0, 260), RuntimeUiFactory.Surface);
            refs.Prompt = RuntimeUiFactory.Label(promptCard.transform, "Question Prompt", "Đang chờ câu hỏi từ server…", new Vector2(830, 210), Vector2.zero, 34, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);

            refs.Answers = new Button[4];
            refs.AnswerLabels = new TMP_Text[4];
            var positions = new[]
            {
                new Vector2(-220, 62), new Vector2(220, 62),
                new Vector2(-220, -95), new Vector2(220, -95)
            };
            var colors = new[] { RuntimeUiFactory.Blue, RuntimeUiFactory.Purple, RuntimeUiFactory.Cyan, RuntimeUiFactory.Navy };
            for (var i = 0; i < 4; i++)
            {
                var button = RuntimeUiFactory.Button(studentPanel.transform, $"Answer {i + 1}", string.Empty,
                    new Vector2(420, 132), positions[i], colors[i], null, fontSize: 24);
                refs.Answers[i] = button;
                refs.AnswerLabels[i] = button.GetComponentInChildren<TMP_Text>();
                refs.AnswerLabels[i].text = $"{(char)('A' + i)}. —";
                refs.Answers[i].interactable = false;
            }

            var feedback = RuntimeUiFactory.Panel(studentPanel.transform, "Answer Feedback", new Vector2(900, 170), new Vector2(0, -360), RuntimeUiFactory.Hex("FFF0B8"));
            refs.FeedbackPanel = feedback.gameObject;
            refs.FeedbackTitle = RuntimeUiFactory.Label(feedback.transform, "Feedback Title", "", new Vector2(850, 42), new Vector2(0, 52), 28, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.FeedbackBody = RuntimeUiFactory.Label(feedback.transform, "Feedback Explanation", "", new Vector2(850, 98), new Vector2(0, -22), 22, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.FeedbackPanel.SetActive(false);
            refs.Status = RuntimeUiFactory.Label(studentPanel.transform, "Student Status", "", new Vector2(850, 34), new Vector2(0, -490), 15, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
        }

        private static void BuildTeacherDashboard(Transform parent, GameplayUiRefs refs)
        {
            var teacherPanel = RuntimeUiFactory.Panel(parent, "Teacher Dashboard", Vector2.zero, Vector2.zero, RuntimeUiFactory.Hex("F3F7FB"));
            teacherPanel.rectTransform.anchorMin = Vector2.zero;
            teacherPanel.rectTransform.anchorMax = Vector2.one;
            teacherPanel.rectTransform.offsetMin = Vector2.zero;
            teacherPanel.rectTransform.offsetMax = Vector2.zero;
            refs.TeacherPanel = teacherPanel.gameObject;
            RuntimeUiFactory.Label(teacherPanel.transform, "Teacher Dashboard Title", "BẢNG THEO DÕI LỚP HỌC", new Vector2(880, 54), new Vector2(0, 478), 30, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.TeacherQuestionText = RuntimeUiFactory.Label(teacherPanel.transform, "Teacher Question", "Đang chờ câu hỏi…", new Vector2(820, 84), new Vector2(-30, 390), 20, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.TeacherTimerText = RuntimeUiFactory.Label(teacherPanel.transform, "Teacher Timer", "--", new Vector2(90, 48), new Vector2(390, 390), 24, RuntimeUiFactory.Blue, TextAlignmentOptions.Center, true);
            var leaderboardCard = RuntimeUiFactory.Panel(teacherPanel.transform, "Live Leaderboard Card", new Vector2(900, 710), new Vector2(0, -40), RuntimeUiFactory.Surface);
            RuntimeUiFactory.Label(leaderboardCard.transform, "Leaderboard Heading", "XẾP HẠNG TRỰC TIẾP", new Vector2(850, 44), new Vector2(0, 315), 22, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.TeacherLeaderboardContent = RuntimeUiFactory.ScrollContent(leaderboardCard.transform, "Live Leaderboard Scroll", new Vector2(830, 600), new Vector2(0, -22), out _);
            refs.TeacherStatusText = RuntimeUiFactory.Label(teacherPanel.transform, "Teacher Live Status", "Cần ít nhất 2 học sinh để bắt đầu.", new Vector2(860, 44), new Vector2(0, -495), 18, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            refs.TeacherPanel.SetActive(false);
        }

        private static void BuildCountdownAndWinnerOverlays(Transform parent, GameplayUiRefs refs)
        {
            var countdown = RuntimeUiFactory.Panel(parent, "Countdown Overlay", Vector2.zero, Vector2.zero, new Color(0.06f, 0.13f, 0.22f, 0.82f));
            countdown.rectTransform.anchorMin = Vector2.zero;
            countdown.rectTransform.anchorMax = Vector2.one;
            countdown.rectTransform.offsetMin = Vector2.zero;
            countdown.rectTransform.offsetMax = Vector2.zero;
            refs.CountdownText = RuntimeUiFactory.Label(countdown.transform, "Countdown", "3", new Vector2(600, 300), new Vector2(0, 70), 140, Color.white, TextAlignmentOptions.Center, true);
            refs.CountdownSubtext = RuntimeUiFactory.Label(countdown.transform, "Countdown Hint", "Chuẩn bị bắt đầu…", new Vector2(900, 80), new Vector2(0, -120), 30, Color.white, TextAlignmentOptions.Center);
            refs.CountdownOverlay = countdown.gameObject;
            refs.CountdownOverlay.SetActive(false);

            var winner = RuntimeUiFactory.Panel(parent, "Top Student Overlay", Vector2.zero, Vector2.zero, new Color(0.04f, 0.11f, 0.19f, 0.88f));
            winner.rectTransform.anchorMin = Vector2.zero;
            winner.rectTransform.anchorMax = Vector2.one;
            winner.rectTransform.offsetMin = Vector2.zero;
            winner.rectTransform.offsetMax = Vector2.zero;
            refs.WinnerOverlay = winner.gameObject;
            RuntimeUiFactory.Label(winner.transform, "Winner Caption", "TOP 1 CỦA PHÒNG", new Vector2(1100, 70), new Vector2(0, 245), 38, Color.white, TextAlignmentOptions.Center, true);
            var avatarPlate = RuntimeUiFactory.Panel(winner.transform, "Winner Avatar Plate", new Vector2(220, 220), new Vector2(0, 70), RuntimeUiFactory.Hex("E5F8FF"));
            avatarPlate.raycastTarget = false;
            refs.WinnerAvatar = RuntimeUiFactory.Label(winner.transform, "Winner Avatar", "A", new Vector2(200, 190), new Vector2(0, 70), 108, RuntimeUiFactory.Blue, TextAlignmentOptions.Center, true);
            refs.WinnerName = RuntimeUiFactory.Label(winner.transform, "Winner Name", "Học sinh xuất sắc", new Vector2(1000, 80), new Vector2(0, -105), 36, Color.white, TextAlignmentOptions.Center, true);
            RuntimeUiFactory.Label(winner.transform, "Winner Transition Hint", "Đang chuyển đến kết quả…", new Vector2(900, 48), new Vector2(0, -195), 21, RuntimeUiFactory.Hex("B9D5E8"), TextAlignmentOptions.Center);
            refs.WinnerOverlay.SetActive(false);
        }
    }
}
