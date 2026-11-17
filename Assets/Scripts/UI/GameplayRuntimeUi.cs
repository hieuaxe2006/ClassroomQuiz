using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace ClassroomQuiz.UI
{
    internal sealed class GameplayUiRefs
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
        public TMP_Text TeacherQuestionText;
        public TMP_Text TeacherTimerText;
        public TMP_Text TeacherStatusText;
        public GameObject CountdownOverlay;
        public GameObject FeedbackPanel;
        public GameObject TeacherPanel;
        public RectTransform TeacherLeaderboardContent;
        public Image ProgressFill;
        public Button[] Answers;
        public TMP_Text[] AnswerLabels;
    }

    internal static class GameplayRuntimeUi
    {
        public static GameplayUiRefs Build()
        {
            var refs = new GameplayUiRefs();
            refs.Canvas = RuntimeUiFactory.CreateCanvas("ClassroomQuiz Gameplay Canvas");
            var background = RuntimeUiFactory.Panel(refs.Canvas.transform, "Background", Vector2.zero, Vector2.zero, RuntimeUiFactory.Background);
            background.rectTransform.anchorMin = Vector2.zero;
            background.rectTransform.anchorMax = Vector2.one;
            background.rectTransform.sizeDelta = Vector2.zero;

            var header = RuntimeUiFactory.Panel(background.transform, "Header", new Vector2(1760, 120), new Vector2(0, 438), RuntimeUiFactory.Surface);
            refs.LessonTitle = RuntimeUiFactory.Label(header.transform, "Lesson Title", "Toán lớp 6", new Vector2(700, 48), new Vector2(-500, 22), 25, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.QuestionCount = RuntimeUiFactory.Label(header.transform, "Question Count", "Câu 0 / 0", new Vector2(310, 48), new Vector2(90, 22), 24, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.ModeLabel = RuntimeUiFactory.Label(header.transform, "Mode", "HỌC", new Vector2(180, 48), new Vector2(380, 22), 20, RuntimeUiFactory.Purple, TextAlignmentOptions.Center, true);
            refs.TimerText = RuntimeUiFactory.Label(header.transform, "Timer", "--", new Vector2(140, 50), new Vector2(760, 22), 28, RuntimeUiFactory.Blue, TextAlignmentOptions.Center, true);
            refs.StudentScoreText = RuntimeUiFactory.Label(header.transform, "Student Score", "Điểm: 0", new Vector2(220, 46), new Vector2(570, 22), 22, RuntimeUiFactory.Green, TextAlignmentOptions.Center, true);
            refs.ProgressFill = RuntimeUiFactory.ProgressBar(header.transform, "Progress", new Vector2(1580, 12), new Vector2(0, -40), RuntimeUiFactory.Cyan);

            var promptCard = RuntimeUiFactory.Panel(background.transform, "Question Card", new Vector2(1480, 270), new Vector2(0, 230), RuntimeUiFactory.Surface);
            refs.Prompt = RuntimeUiFactory.Label(promptCard.transform, "Question Prompt", "Đang chờ câu hỏi từ server…", new Vector2(1320, 210), Vector2.zero, 42, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);

            refs.Answers = new Button[4];
            refs.AnswerLabels = new TMP_Text[4];
            var positions = new[]
            {
                new Vector2(-400, 25), new Vector2(400, 25),
                new Vector2(-400, -150), new Vector2(400, -150)
            };
            var colors = new[] { RuntimeUiFactory.Blue, RuntimeUiFactory.Purple, RuntimeUiFactory.Cyan, RuntimeUiFactory.Navy };
            for (var i = 0; i < 4; i++)
            {
                var index = i;
                var button = RuntimeUiFactory.Button(background.transform, $"Answer {i + 1}", string.Empty,
                    new Vector2(720, 145), positions[i], colors[i], null, fontSize: 28);
                refs.Answers[i] = button;
                refs.AnswerLabels[i] = button.GetComponentInChildren<TMP_Text>();
                refs.AnswerLabels[i].text = $"{(char)('A' + i)}. —";
                refs.Answers[i].interactable = false;
            }

            var feedback = RuntimeUiFactory.Panel(background.transform, "Answer Feedback", new Vector2(1320, 190), new Vector2(0, -350), RuntimeUiFactory.Hex("FFF4C2"));
            refs.FeedbackPanel = feedback.gameObject;
            refs.FeedbackTitle = RuntimeUiFactory.Label(feedback.transform, "Feedback Title", "", new Vector2(1240, 46), new Vector2(0, 63), 32, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.FeedbackBody = RuntimeUiFactory.Label(feedback.transform, "Feedback Explanation", "", new Vector2(1240, 110), new Vector2(0, -30), 25, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.FeedbackPanel.SetActive(false);
            refs.Status = RuntimeUiFactory.Label(background.transform, "Status", "", new Vector2(1100, 34), new Vector2(0, -488), 16, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);

            var teacherPanel = RuntimeUiFactory.Panel(background.transform, "Teacher Dashboard", Vector2.zero, Vector2.zero, RuntimeUiFactory.Background);
            teacherPanel.rectTransform.anchorMin = Vector2.zero;
            teacherPanel.rectTransform.anchorMax = Vector2.one;
            teacherPanel.rectTransform.sizeDelta = Vector2.zero;
            refs.TeacherPanel = teacherPanel.gameObject;
            RuntimeUiFactory.Label(teacherPanel.transform, "Teacher Dashboard Title", "BẢNG THEO DÕI LỚP HỌC", new Vector2(1500, 58), new Vector2(0, 465), 36, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.TeacherQuestionText = RuntimeUiFactory.Label(teacherPanel.transform, "Teacher Question", "Đang chờ câu hỏi…", new Vector2(1430, 76), new Vector2(-70, 390), 23, RuntimeUiFactory.Navy, TextAlignmentOptions.MidlineLeft, true);
            refs.TeacherTimerText = RuntimeUiFactory.Label(teacherPanel.transform, "Teacher Timer", "--", new Vector2(160, 54), new Vector2(780, 390), 28, RuntimeUiFactory.Blue, TextAlignmentOptions.Center, true);
            var leaderboardCard = RuntimeUiFactory.Panel(teacherPanel.transform, "Live Leaderboard Card", new Vector2(1540, 690), new Vector2(0, -38), RuntimeUiFactory.Surface);
            RuntimeUiFactory.Label(leaderboardCard.transform, "Leaderboard Heading", "XẾP HẠNG CẬP NHẬT TRỰC TIẾP", new Vector2(1450, 46), new Vector2(0, 300), 24, RuntimeUiFactory.Navy, TextAlignmentOptions.Center, true);
            refs.TeacherLeaderboardContent = RuntimeUiFactory.ScrollContent(leaderboardCard.transform, "Live Leaderboard Scroll", new Vector2(1430, 560), new Vector2(0, -20), out _);
            refs.TeacherStatusText = RuntimeUiFactory.Label(teacherPanel.transform, "Teacher Live Status", "Cần giáo viên và ít nhất 2 học sinh để bắt đầu.", new Vector2(1500, 46), new Vector2(0, -475), 20, RuntimeUiFactory.Muted, TextAlignmentOptions.Center);
            refs.TeacherPanel.SetActive(false);

            var countdown = RuntimeUiFactory.Panel(background.transform, "Countdown Overlay", new Vector2(1920, 1080), Vector2.zero, new Color(0.06f, 0.13f, 0.22f, 0.82f));
            countdown.rectTransform.anchorMin = Vector2.zero;
            countdown.rectTransform.anchorMax = Vector2.one;
            countdown.rectTransform.sizeDelta = Vector2.zero;
            refs.CountdownText = RuntimeUiFactory.Label(countdown.transform, "Countdown", "3", new Vector2(600, 300), new Vector2(0, 70), 140, Color.white, TextAlignmentOptions.Center, true);
            refs.CountdownSubtext = RuntimeUiFactory.Label(countdown.transform, "Countdown Hint", "Chuẩn bị bắt đầu…", new Vector2(900, 80), new Vector2(0, -120), 30, Color.white, TextAlignmentOptions.Center);
            countdown.gameObject.name = "Countdown Overlay";
            refs.CountdownOverlay = countdown.gameObject;
            return refs;
        }
    }
}
