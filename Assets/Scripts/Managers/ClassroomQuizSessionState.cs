namespace ClassroomQuiz.Managers
{
    [System.Serializable]
    public sealed class ClassroomQuizStandingSnapshot
    {
        public int rank;
        public string playerId;
        public string displayName;
        public string avatarKey;
        public int score;
        public int correctCount;
        public int incorrectCount;
        public int unansweredCount;
        public int totalQuestions;
        public long totalAnswerTimeMilliseconds;
    }

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
        public static long AccountId { get; private set; }
        public static string AccountEmail { get; private set; } = string.Empty;
        public static string DisplayName { get; private set; } = string.Empty;
        public static string AvatarKey { get; private set; } = "A";
        public static string AuthToken { get; private set; } = string.Empty;
        public static string ApiUrl { get; private set; } = string.Empty;
        public static bool IsAuthenticated => AccountId > 0 && !string.IsNullOrWhiteSpace(AuthToken);

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
        public static ClassroomQuizStandingSnapshot[] LastFinalStandings { get; set; } = System.Array.Empty<ClassroomQuizStandingSnapshot>();
        public static ClassroomQuizPlayerSnapshot[] RoomPlayers { get; set; } = System.Array.Empty<ClassroomQuizPlayerSnapshot>();

        public static bool IsTeacher => Role == "Teacher";

        public static void SetAccountSession(long accountId, string email, string displayName,
            string avatarKey, string authToken, string apiUrl)
        {
            AccountId = accountId > 0 ? accountId : 0;
            AccountEmail = email ?? string.Empty;
            DisplayName = displayName ?? string.Empty;
            AvatarKey = string.IsNullOrWhiteSpace(avatarKey) ? "A" : avatarKey;
            AuthToken = authToken ?? string.Empty;
            ApiUrl = apiUrl ?? string.Empty;
        }

        public static void ClearAccountSession()
        {
            AccountId = 0;
            AccountEmail = string.Empty;
            DisplayName = string.Empty;
            AvatarKey = "A";
            AuthToken = string.Empty;
            ApiUrl = string.Empty;
        }

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
            LastFinalStandings = System.Array.Empty<ClassroomQuizStandingSnapshot>();
            RoomPlayers = System.Array.Empty<ClassroomQuizPlayerSnapshot>();
        }
    }
}
