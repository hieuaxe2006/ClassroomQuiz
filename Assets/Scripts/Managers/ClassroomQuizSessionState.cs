namespace ClassroomQuiz.Managers
{
    [System.Serializable]
    public sealed class ClassroomQuizPlayerSnapshot
    {
        public string playerId;
        public string displayName;
        public string avatarKey;
        public string role;
        public int seatIndex;
        public long joinSequence;
    }

    public static class ClassroomQuizSessionState
    {
        public static string RoomCode { get; set; } = string.Empty;
        public static string RoomName { get; set; } = string.Empty;
        public static string Role { get; set; } = "Student";
        public static string Mode { get; set; } = "Study";
        public static int LessonId { get; set; }
        public static string LessonTitle { get; set; } = string.Empty;
        public static int DurationSeconds { get; set; } = 15;
        public static int QuestionCount { get; set; }
        public static long CountdownEndsAtUnixMs { get; set; }
        public static long ServerClockOffsetMs { get; set; }
        public static string LastPersonalResult { get; set; } = "Chưa có kết quả.";
        public static string LastTeacherLeaderboard { get; set; } = string.Empty;
        public static ClassroomQuizPlayerSnapshot[] RoomPlayers { get; set; } = System.Array.Empty<ClassroomQuizPlayerSnapshot>();

        public static bool IsTeacher => Role == "Teacher";

        public static void ClearRoom()
        {
            RoomCode = string.Empty;
            RoomName = string.Empty;
            Role = "Student";
            Mode = "Study";
            LessonId = 0;
            LessonTitle = string.Empty;
            DurationSeconds = 15;
            QuestionCount = 0;
            CountdownEndsAtUnixMs = 0;
            ServerClockOffsetMs = 0;
            RoomPlayers = System.Array.Empty<ClassroomQuizPlayerSnapshot>();
        }
    }
}
