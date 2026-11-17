using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ClassroomQuiz.Server;

internal static class DatabaseApi
{
    private static readonly HttpClient Http = new() { Timeout = TimeSpan.FromSeconds(5) };
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true
    };

    private static string ApiUrl => Environment.GetEnvironmentVariable("CLASSROOM_QUIZ_DB_API")
        ?? "http://127.0.0.1/ClassroomQuizApi/api.php";

    private static string ServiceKey => Environment.GetEnvironmentVariable("CLASSROOM_QUIZ_SERVICE_KEY")
        ?? "ClassroomQuiz-local-change-this-key";

    public static async Task<VerifiedAccount?> VerifyTokenAsync(string token)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { action = "verifyToken", token }, JsonOptions);
            using var response = await Http.PostAsync(ApiUrl, new StringContent(json, Encoding.UTF8, "application/json")).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode) return null;
            return JsonSerializer.Deserialize<VerifiedAccount>(body, JsonOptions) is { Success: true } account
                ? account
                : null;
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Could not verify account token through XAMPP API: {exception.Message}");
            return null;
        }
    }

    public static async Task SaveRoomAsync(RoomHistoryRecord record)
    {
        try
        {
            var json = JsonSerializer.Serialize(new { action = "saveRoom", room = record.Room, details = record.Details }, JsonOptions);
            using var request = new HttpRequestMessage(HttpMethod.Post, ApiUrl)
            {
                Content = new StringContent(json, Encoding.UTF8, "application/json")
            };
            request.Headers.Add("X-ClassroomQuiz-Service-Key", ServiceKey);
            using var response = await Http.SendAsync(request).ConfigureAwait(false);
            var body = await response.Content.ReadAsStringAsync().ConfigureAwait(false);
            if (!response.IsSuccessStatusCode)
            {
                Console.WriteLine($"Room history was not saved to XAMPP: {(int)response.StatusCode} {body}");
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Room history was not saved to XAMPP: {exception.Message}");
        }
    }
}

internal sealed class VerifiedAccount
{
    public bool Success { get; set; }
    public long AccountId { get; set; }
    public string DisplayName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
}

internal sealed class RoomHistoryRecord
{
    public required RoomHistoryRoom Room { get; init; }
    public required List<RoomHistoryDetail> Details { get; init; }
}

internal sealed class RoomHistoryRoom
{
    public string RoomCode { get; init; } = string.Empty;
    public string RoomName { get; init; } = string.Empty;
    public string LessonTitle { get; init; } = string.Empty;
    public string Mode { get; init; } = string.Empty;
    public int QuestionCount { get; init; }
    public long HostAccountId { get; init; }
    public string HostName { get; init; } = string.Empty;
}

internal sealed class RoomHistoryDetail
{
    public long AccountId { get; init; }
    public string DisplayName { get; init; } = string.Empty;
    public int RankPosition { get; init; }
    public int CorrectCount { get; init; }
    public int IncorrectCount { get; init; }
    public int UnansweredCount { get; init; }
    public int TotalQuestions { get; init; }
    public int Score { get; init; }
    public long ResponseTimeMs { get; init; }
}
