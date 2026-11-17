using System.Collections.Concurrent;
using System.Net;
using System.Net.Sockets;
using System.Security.Cryptography;
using System.Text;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace ClassroomQuiz.Server;

internal static class MessageTypes
{
    public const string Hello = "Hello";
    public const string Welcome = "Welcome";
    public const string Error = "Error";
    public const string CreateRoomRequest = "CreateRoomRequest";
    public const string CreateRoomResult = "CreateRoomResult";
    public const string JoinRoomRequest = "JoinRoomRequest";
    public const string JoinRoomResult = "JoinRoomResult";
    public const string RoomStateUpdate = "RoomStateUpdate";
    public const string LeaveRoomRequest = "LeaveRoomRequest";
    public const string PlayerDisconnected = "PlayerDisconnected";
    public const string StartGameRequest = "StartGameRequest";
    public const string StartCountdown = "StartCountdown";
    public const string QuestionStarted = "QuestionStarted";
    public const string SubmitAnswerRequest = "SubmitAnswerRequest";
    public const string AnswerResult = "AnswerResult";
    public const string QuestionEnded = "QuestionEnded";
    public const string GameEnded = "GameEnded";
    public const string StudentPersonalResult = "StudentPersonalResult";
    public const string TeacherLeaderboard = "TeacherLeaderboard";
    public const string EndSession = "EndSession";
    public const string RestartSession = "RestartSession";
    public const string ReturnToMenu = "ReturnToMenu";
    public const string Ping = "Ping";
    public const string Pong = "Pong";
}

internal sealed class NetworkMessage
{
    public int ProtocolVersion { get; set; } = 1;
    public string Type { get; set; } = string.Empty;
    public string RequestId { get; set; } = string.Empty;
    public string? RoomCode { get; set; }
    public string? RoomName { get; set; }
    public string? PlayerName { get; set; }
    public string? AuthToken { get; set; }
    public string? Role { get; set; }
    public string? Mode { get; set; }
    public string? LessonTitle { get; set; }
    public int? LessonId { get; set; }
    public int? QuestionIndex { get; set; }
    public int? AnswerIndex { get; set; }
    public string? Payload { get; set; }
    public string? Error { get; set; }
    public bool? Success { get; set; }
    public int? DurationSeconds { get; set; }
    public string? ServerAddress { get; set; }
    public int? ServerPort { get; set; }
    public string? Message { get; set; }
    public string? Data { get; set; }
    public long? AccountId { get; set; }
}

internal sealed class RoomPayload
{
    public int SchemaVersion { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string Summary { get; set; } = string.Empty;
    public List<QuestionDefinition>? Questions { get; set; }
}

internal sealed class QuestionDefinition
{
    public int LessonId { get; set; }
    public string LessonTitle { get; set; } = string.Empty;
    public string QuestionId { get; set; } = string.Empty;
    public int SequenceIndex { get; set; }
    public string Prompt { get; set; } = string.Empty;
    public List<string>? Options { get; set; }
    public int CorrectIndex { get; set; }
    public string Explanation { get; set; } = string.Empty;
}

internal sealed class PlayerSession
{
    public required string PlayerId { get; init; }
    public required string DisplayName { get; set; }
    public required TcpClient Client { get; init; }
    public required NetworkStream Stream { get; init; }
    public required SemaphoreSlim WriteLock { get; init; }
    public RoomSession? Room { get; set; }
    public string Role { get; set; } = "Student";
    public long AccountId { get; set; }
    public int CorrectCount { get; set; }
    public long TotalAnswerTimeMilliseconds { get; set; }
    public int SubmittedCount { get; set; }
    public int Score { get; set; }
    public bool HelloReceived { get; set; }
}

internal sealed class AnswerRecord
{
    public required int AnswerIndex { get; init; }
    public required bool IsCorrect { get; init; }
    public required long AnswerTimeMilliseconds { get; init; }
    public required int ScoreAwarded { get; init; }
}

internal sealed class RoomSession
{
    public required string RoomCode { get; init; }
    public required string RoomName { get; init; }
    public required string HostPlayerId { get; set; }
    public long HostAccountId { get; init; }
    public required string Mode { get; init; }
    public required int LessonId { get; init; }
    public required int DurationSeconds { get; init; }
    public required int StudentCapacity { get; init; }
    public required string LessonTitle { get; init; }
    public required List<QuestionDefinition> Questions { get; init; }
    public required Dictionary<string, PlayerSession> Players { get; init; }
    public required SemaphoreSlim Gate { get; init; }
    public Dictionary<string, AnswerRecord> Answers { get; } = new(StringComparer.Ordinal);
    public bool Started { get; set; }
    public bool Ended { get; set; }
    public bool QuestionActive { get; set; }
    public int CurrentQuestionIndex { get; set; } = -1;
    public long CurrentQuestionStartedUnixMs { get; set; }
    public long CurrentQuestionEndsUnixMs { get; set; }
    public CancellationTokenSource GameCancellation { get; } = new();
}

internal sealed class TcpGameServer
{
    private const int ProtocolVersion = 1;
    private const int MaximumMessageBytes = 1_048_576;
    private const string RoomAlphabet = "ABCDEFGHJKLMNPRSTUVWXYZ23456789";
    private readonly TcpListener _listener;
    private readonly int _studentCapacity;
    private readonly ConcurrentDictionary<string, RoomSession> _rooms = new(StringComparer.Ordinal);
    private readonly ConcurrentDictionary<string, PlayerSession> _sessions = new(StringComparer.Ordinal);
    private readonly JsonSerializerOptions _jsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        MaxDepth = 24
    };

    public TcpGameServer(IPAddress ipAddress, int port, int studentCapacity)
    {
        _listener = new TcpListener(ipAddress, port);
        _studentCapacity = Math.Clamp(studentCapacity, 1, 200);
    }

    public async Task StartAsync(CancellationToken cancellationToken)
    {
        _listener.Start();
        Console.WriteLine($"TCP server listening on {_listener.LocalEndpoint}; capacity {_studentCapacity} students per room.");
        try
        {
            while (!cancellationToken.IsCancellationRequested)
            {
                var client = await _listener.AcceptTcpClientAsync(cancellationToken).ConfigureAwait(false);
                client.NoDelay = true;
                _ = HandleClientAsync(client);
            }
        }
        catch (OperationCanceledException)
        {
            Console.WriteLine("Server shutdown requested.");
        }
        catch (ObjectDisposedException) when (cancellationToken.IsCancellationRequested)
        {
        }
        finally
        {
            _listener.Stop();
            foreach (var session in _sessions.Values)
            {
                session.Client.Close();
            }
            Console.WriteLine("TCP server stopped.");
        }
    }

    private async Task HandleClientAsync(TcpClient client)
    {
        var session = new PlayerSession
        {
            PlayerId = Guid.NewGuid().ToString("N"),
            DisplayName = "Guest",
            Client = client,
            Stream = client.GetStream(),
            WriteLock = new SemaphoreSlim(1, 1)
        };
        _sessions[session.PlayerId] = session;

        try
        {
            while (client.Connected)
            {
                var message = await ReadMessageAsync(session.Stream).ConfigureAwait(false);
                if (message == null)
                {
                    break;
                }

                if (message.ProtocolVersion != ProtocolVersion)
                {
                    await SendErrorAsync(session, message, $"Phiên bản giao thức không hỗ trợ: {message.ProtocolVersion}.").ConfigureAwait(false);
                    break;
                }

                await ProcessMessageAsync(session, message).ConfigureAwait(false);
            }
        }
        catch (IOException exception)
        {
            Console.WriteLine($"Client {session.DisplayName} disconnected: {exception.Message}");
        }
        catch (SocketException exception)
        {
            Console.WriteLine($"Client socket error: {exception.Message}");
        }
        catch (JsonException exception)
        {
            Console.WriteLine($"Client sent invalid JSON: {exception.Message}");
            try
            {
                await SendAsync(session, new NetworkMessage { Type = MessageTypes.Error, Error = "JSON không hợp lệ." }).ConfigureAwait(false);
            }
            catch
            {
            }
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Client handler error: {exception}");
            try
            {
                await SendAsync(session, new NetworkMessage { Type = MessageTypes.Error, Error = "Lỗi xử lý yêu cầu trên server." }).ConfigureAwait(false);
            }
            catch
            {
            }
        }
        finally
        {
            await DisconnectAsync(session).ConfigureAwait(false);
        }
    }

    private async Task ProcessMessageAsync(PlayerSession session, NetworkMessage message)
    {
        if (message.Type == MessageTypes.Hello)
        {
            if (string.IsNullOrWhiteSpace(message.AuthToken))
            {
                await SendAsync(session, new NetworkMessage
                {
                    Type = MessageTypes.Welcome,
                    RequestId = message.RequestId,
                    ProtocolVersion = ProtocolVersion,
                    Success = false,
                    Error = "Hãy đăng nhập bằng tài khoản ClassroomQuiz trước khi kết nối."
                }).ConfigureAwait(false);
                return;
            }

            var account = await DatabaseApi.VerifyTokenAsync(message.AuthToken).ConfigureAwait(false);
            if (account == null)
            {
                await SendAsync(session, new NetworkMessage
                {
                    Type = MessageTypes.Welcome,
                    RequestId = message.RequestId,
                    ProtocolVersion = ProtocolVersion,
                    Success = false,
                    Error = "Phiên đăng nhập không hợp lệ hoặc XAMPP API chưa chạy."
                }).ConfigureAwait(false);
                return;
            }

            session.HelloReceived = true;
            session.AccountId = account.AccountId;
            session.DisplayName = NormalizeName(account.DisplayName);
            await SendAsync(session, new NetworkMessage
            {
                Type = MessageTypes.Welcome,
                RequestId = message.RequestId,
                ProtocolVersion = ProtocolVersion,
                Success = true,
                Message = $"Xin chào {session.DisplayName}.",
                Data = SerializeData(new { playerId = session.PlayerId, accountId = session.AccountId, displayName = session.DisplayName })
            }).ConfigureAwait(false);
            return;
        }

        if (!session.HelloReceived)
        {
            await SendErrorAsync(session, message, "Hãy gửi Hello trước khi sử dụng server.").ConfigureAwait(false);
            return;
        }

        switch (message.Type)
        {
            case MessageTypes.CreateRoomRequest:
                await CreateRoomAsync(session, message).ConfigureAwait(false);
                break;
            case MessageTypes.JoinRoomRequest:
                await JoinRoomAsync(session, message).ConfigureAwait(false);
                break;
            case MessageTypes.LeaveRoomRequest:
                await LeaveRoomAsync(session, message).ConfigureAwait(false);
                break;
            case MessageTypes.StartGameRequest:
                await StartGameAsync(session, message).ConfigureAwait(false);
                break;
            case MessageTypes.SubmitAnswerRequest:
                await SubmitAnswerAsync(session, message).ConfigureAwait(false);
                break;
            case MessageTypes.EndSession:
                await EndSessionAsync(session, message).ConfigureAwait(false);
                break;
            case MessageTypes.RestartSession:
                await RestartSessionAsync(session, message).ConfigureAwait(false);
                break;
            case MessageTypes.Ping:
                await SendAsync(session, new NetworkMessage { Type = MessageTypes.Pong, RequestId = message.RequestId }).ConfigureAwait(false);
                break;
            default:
                await SendErrorAsync(session, message, $"Loại message không được hỗ trợ: {message.Type}.").ConfigureAwait(false);
                break;
        }
    }

    private async Task CreateRoomAsync(PlayerSession session, NetworkMessage message)
    {
        if (session.Room != null)
        {
            await SendCreateResultAsync(session, message, false, "Hãy rời phòng hiện tại trước khi tạo phòng mới.").ConfigureAwait(false);
            return;
        }

        var lessonId = message.LessonId ?? 0;
        var duration = message.DurationSeconds ?? 0;
        var mode = NormalizeMode(message.Mode);
        if (lessonId < 1 || lessonId > 42)
        {
            await SendCreateResultAsync(session, message, false, "Bài học không hợp lệ.").ConfigureAwait(false);
            return;
        }
        if (duration is not (5 or 10 or 15 or 20))
        {
            await SendCreateResultAsync(session, message, false, "Thời gian mỗi câu phải là 5, 10, 15 hoặc 20 giây.").ConfigureAwait(false);
            return;
        }
        if (mode == null)
        {
            await SendCreateResultAsync(session, message, false, "Chế độ phòng không hợp lệ.").ConfigureAwait(false);
            return;
        }
        if (string.IsNullOrWhiteSpace(message.Payload) || Encoding.UTF8.GetByteCount(message.Payload) > MaximumMessageBytes / 2)
        {
            await SendCreateResultAsync(session, message, false, "Bộ câu hỏi trống hoặc quá lớn.").ConfigureAwait(false);
            return;
        }

        RoomPayload? payload;
        try
        {
            payload = JsonSerializer.Deserialize<RoomPayload>(message.Payload, _jsonOptions);
        }
        catch (JsonException)
        {
            payload = null;
        }
        if (payload?.Questions == null || payload.Questions.Count is < 2 or > 5 || string.IsNullOrWhiteSpace(payload.LessonTitle))
        {
            await SendCreateResultAsync(session, message, false, "Bài học cần có tên và từ 2 đến 5 câu hợp lệ.").ConfigureAwait(false);
            return;
        }

        var questionError = ValidateQuestions(payload.Questions);
        if (questionError != null)
        {
            await SendCreateResultAsync(session, message, false, questionError).ConfigureAwait(false);
            return;
        }

        if (session.Room != null)
        {
            await SendCreateResultAsync(session, message, false, "Hãy rời phòng hiện tại trước khi tạo phòng mới.").ConfigureAwait(false);
            return;
        }

        RoomSession? room = null;
        for (var attempt = 0; attempt < 20; attempt++)
        {
            var roomCode = GenerateRoomCode();
            var candidate = new RoomSession
            {
                RoomCode = roomCode,
                RoomName = NormalizeRoomName(message.RoomName, payload.LessonTitle),
                HostPlayerId = session.PlayerId,
                HostAccountId = session.AccountId,
                Mode = mode,
                LessonId = lessonId,
                DurationSeconds = duration,
                StudentCapacity = _studentCapacity,
                LessonTitle = payload.LessonTitle.Trim(),
                Questions = payload.Questions.OrderBy(q => q.SequenceIndex).Select(CloneQuestion).ToList(),
                Players = new Dictionary<string, PlayerSession>(StringComparer.Ordinal),
                Gate = new SemaphoreSlim(1, 1)
            };

            await candidate.Gate.WaitAsync().ConfigureAwait(false);
            candidate.Players[session.PlayerId] = session;
            session.Room = candidate;
            session.Role = "Teacher";
            candidate.Gate.Release();
            if (_rooms.TryAdd(roomCode, candidate))
            {
                room = candidate;
                break;
            }

            session.Room = null;
            session.Role = "Student";
            candidate.Gate.Dispose();
        }

        if (room == null)
        {
            await SendCreateResultAsync(session, message, false, "Không thể tạo mã phòng lúc này. Hãy thử lại.").ConfigureAwait(false);
            return;
        }

        Console.WriteLine($"Room {room.RoomCode} created by {session.DisplayName} ({room.Mode}, lesson {room.LessonId}).");
        await BroadcastRoomStateAsync(room).ConfigureAwait(false);
        await SendCreateResultAsync(session, message, true, $"Phòng {room.RoomCode} đã được tạo.", room.RoomCode).ConfigureAwait(false);
    }

    private async Task JoinRoomAsync(PlayerSession session, NetworkMessage message)
    {
        var roomCode = (message.RoomCode ?? string.Empty).Trim().ToUpperInvariant();
        if (roomCode.Length != 6 || roomCode.Any(c => !RoomAlphabet.Contains(c)))
        {
            await SendJoinResultAsync(session, message, false, "Mã phòng phải gồm 6 ký tự hợp lệ.").ConfigureAwait(false);
            return;
        }
        if (!_rooms.TryGetValue(roomCode, out var room))
        {
            await SendJoinResultAsync(session, message, false, "Không tìm thấy phòng. Kiểm tra lại mã phòng.").ConfigureAwait(false);
            return;
        }
        if (room.Started || room.Ended)
        {
            await SendJoinResultAsync(session, message, false, "Phòng đã bắt đầu hoặc đã kết thúc.").ConfigureAwait(false);
            return;
        }

        if (session.Room != null && !ReferenceEquals(session.Room, room))
        {
            await LeaveRoomAsync(session, message).ConfigureAwait(false);
        }

        string? failure = null;
        await room.Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (room.Started || room.Ended)
            {
                failure = "Phòng đã bắt đầu hoặc đã kết thúc.";
            }
            else if (!room.Players.ContainsKey(session.PlayerId) && room.Players.Values.Count(p => p.Role != "Teacher") >= room.StudentCapacity)
            {
                failure = "Phòng đã đủ 40 học sinh.";
            }
            else
            {
                session.Room = room;
                session.Role = "Student";
                session.DisplayName = NormalizeName(message.PlayerName ?? session.DisplayName);
                room.Players[session.PlayerId] = session;
            }
        }
        finally
        {
            room.Gate.Release();
        }

        if (failure != null)
        {
            await SendJoinResultAsync(session, message, false, failure).ConfigureAwait(false);
            return;
        }

        Console.WriteLine($"{session.DisplayName} joined room {room.RoomCode}.");
        await BroadcastRoomStateAsync(room).ConfigureAwait(false);
        await SendJoinResultAsync(session, message, true, $"Đã tham gia phòng {room.RoomCode}.", room.RoomCode).ConfigureAwait(false);
    }

    private async Task LeaveRoomAsync(PlayerSession session, NetworkMessage message)
    {
        var room = session.Room;
        if (room == null)
        {
            return;
        }

        var roomEmpty = false;
        await room.Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            room.Players.Remove(session.PlayerId);
            room.Answers.Remove(session.PlayerId);
            session.Room = null;
            session.Role = "Student";

            if (room.Players.Count == 0)
            {
                roomEmpty = true;
            }
            else if (room.HostPlayerId == session.PlayerId)
            {
                var nextHost = room.Players.Values.First();
                room.HostPlayerId = nextHost.PlayerId;
                nextHost.Role = "Teacher";
            }
        }
        finally
        {
            room.Gate.Release();
        }

        if (roomEmpty)
        {
            RemoveRoom(room);
            return;
        }

        var disconnected = new NetworkMessage
        {
            Type = MessageTypes.PlayerDisconnected,
            RoomCode = room.RoomCode,
            Message = $"{session.DisplayName} đã rời phòng.",
            Data = SerializeData(new { playerId = session.PlayerId, displayName = session.DisplayName })
        };
        await BroadcastAsync(room, disconnected).ConfigureAwait(false);
        await BroadcastRoomStateAsync(room).ConfigureAwait(false);
        if (room.Started && !room.Ended)
        {
            await SendTeacherLeaderboardAsync(room).ConfigureAwait(false);
        }
    }

    private async Task StartGameAsync(PlayerSession session, NetworkMessage message)
    {
        var room = session.Room;
        if (room == null)
        {
            await SendErrorAsync(session, message, "Bạn chưa ở trong phòng.").ConfigureAwait(false);
            return;
        }

        string? failure = null;
        await room.Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (room.HostPlayerId != session.PlayerId || session.Role != "Teacher")
            {
                failure = "Chỉ giáo viên của phòng mới được bắt đầu.";
            }
            else if (room.Started)
            {
                failure = "Phòng đã bắt đầu.";
            }
            else if (room.Players.Values.Count(player => player.Role != "Teacher") < 2)
            {
                failure = "Cần ít nhất 2 học sinh cùng giáo viên trong phòng mới có thể bắt đầu.";
            }
            else if (room.Questions.Count is < 2 or > 5)
            {
                failure = "Phòng không có đủ câu hỏi hợp lệ.";
            }
            else
            {
                room.Started = true;
                room.Ended = false;
                room.CurrentQuestionIndex = -1;
                foreach (var player in room.Players.Values)
                {
                    player.CorrectCount = 0;
                    player.TotalAnswerTimeMilliseconds = 0;
                    player.SubmittedCount = 0;
                    player.Score = 0;
                }
            }
        }
        finally
        {
            room.Gate.Release();
        }

        if (failure != null)
        {
            await SendErrorAsync(session, message, failure).ConfigureAwait(false);
            return;
        }

        await BroadcastCountdownAndStartAsync(room).ConfigureAwait(false);
    }

    private async Task RestartSessionAsync(PlayerSession session, NetworkMessage message)
    {
        var room = session.Room;
        if (room == null)
        {
            await SendErrorAsync(session, message, "Bạn chưa ở trong phòng.").ConfigureAwait(false);
            return;
        }

        string? failure = null;
        await room.Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (room.HostPlayerId != session.PlayerId || session.Role != "Teacher")
            {
                failure = "Chỉ giáo viên của phòng mới được chơi lại.";
            }
            else if (!room.Ended || !room.Started)
            {
                failure = "Chỉ có thể chơi lại sau khi buổi học kết thúc.";
            }
            else if (room.Players.Values.Count(player => player.Role != "Teacher") < 2)
            {
                failure = "Cần ít nhất 2 học sinh vẫn ở trong phòng để chơi lại.";
            }
            else
            {
                room.Started = true;
                room.Ended = false;
                room.QuestionActive = false;
                room.CurrentQuestionIndex = -1;
                room.Answers.Clear();
                foreach (var player in room.Players.Values)
                {
                    player.CorrectCount = 0;
                    player.TotalAnswerTimeMilliseconds = 0;
                    player.SubmittedCount = 0;
                    player.Score = 0;
                }
            }
        }
        finally
        {
            room.Gate.Release();
        }

        if (failure != null)
        {
            await SendErrorAsync(session, message, failure).ConfigureAwait(false);
            return;
        }

        await BroadcastRoomStateAsync(room).ConfigureAwait(false);
        await BroadcastCountdownAndStartAsync(room).ConfigureAwait(false);
    }

    private async Task BroadcastCountdownAndStartAsync(RoomSession room)
    {
        var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
        var countdown = new NetworkMessage
        {
            Type = MessageTypes.StartCountdown,
            RoomCode = room.RoomCode,
            RoomName = room.RoomName,
            Data = SerializeData(new
            {
                seconds = 3,
                roomCode = room.RoomCode,
                roomName = room.RoomName,
                lessonId = room.LessonId,
                lessonTitle = room.LessonTitle,
                mode = room.Mode,
                serverNowUnixMs = now,
                startAtUnixMs = now + 3000
            })
        };
        await BroadcastAsync(room, countdown).ConfigureAwait(false);
        _ = RunGameAsync(room);
    }

    private async Task RunGameAsync(RoomSession room)
    {
        try
        {
            await Task.Delay(TimeSpan.FromSeconds(3), room.GameCancellation.Token).ConfigureAwait(false);

            for (var index = 0; index < room.Questions.Count; index++)
            {
                var now = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds();
                await room.Gate.WaitAsync(room.GameCancellation.Token).ConfigureAwait(false);
                try
                {
                    if (!room.Started || room.Ended)
                    {
                        return;
                    }
                    room.CurrentQuestionIndex = index;
                    room.CurrentQuestionStartedUnixMs = now;
                    room.CurrentQuestionEndsUnixMs = now + room.DurationSeconds * 1000L;
                    room.QuestionActive = true;
                    room.Answers.Clear();
                }
                finally
                {
                    room.Gate.Release();
                }

                var question = room.Questions[index];
                var questionStarted = new NetworkMessage
                {
                    Type = MessageTypes.QuestionStarted,
                    RoomCode = room.RoomCode,
                    RoomName = room.RoomName,
                    QuestionIndex = index,
                    LessonId = room.LessonId,
                    LessonTitle = room.LessonTitle,
                    Mode = room.Mode,
                    DurationSeconds = room.DurationSeconds,
                    Data = SerializeData(new
                    {
                        questionId = question.QuestionId,
                        sequenceIndex = question.SequenceIndex,
                        prompt = question.Prompt,
                        options = question.Options,
                        questionCount = room.Questions.Count,
                        serverNowUnixMs = now,
                        endsAtUnixMs = now + room.DurationSeconds * 1000L
                    })
                };
                await BroadcastAsync(room, questionStarted).ConfigureAwait(false);
                await SendTeacherLeaderboardAsync(room).ConfigureAwait(false);

                var waitMilliseconds = Math.Max(0, (int)Math.Min(int.MaxValue, room.CurrentQuestionEndsUnixMs - DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()));
                await Task.Delay(waitMilliseconds, room.GameCancellation.Token).ConfigureAwait(false);
                await EndQuestionAsync(room, question, index).ConfigureAwait(false);
            }

            await EndGameAsync(room).ConfigureAwait(false);
        }
        catch (OperationCanceledException)
        {
        }
        catch (Exception exception)
        {
            Console.WriteLine($"Game loop for room {room.RoomCode} failed: {exception}");
            await BroadcastAsync(room, new NetworkMessage { Type = MessageTypes.Error, Error = "Có lỗi khi chạy phòng. Hãy thử tạo phòng mới." }).ConfigureAwait(false);
        }
    }

    private async Task EndQuestionAsync(RoomSession room, QuestionDefinition question, int questionIndex)
    {
        PlayerSession[] players;
        int answeredCount;
        int correctCount;
        Dictionary<string, AnswerRecord> answers;
        await room.Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!room.Started || room.CurrentQuestionIndex != questionIndex)
            {
                return;
            }

            room.QuestionActive = false;
            players = room.Players.Values.ToArray();
            answers = new Dictionary<string, AnswerRecord>(room.Answers, StringComparer.Ordinal);
            answeredCount = answers.Count;
            correctCount = answers.Values.Count(answer => answer.IsCorrect);
        }
        finally
        {
            room.Gate.Release();
        }

        foreach (var player in players)
        {
            var data = new
            {
                answeredCount,
                correctCount = player.Role == "Teacher" ? correctCount : (int?)null,
                incorrectCount = player.Role == "Teacher" ? answeredCount - correctCount : (int?)null,
                answerReveal = room.Mode == "Study" ? new { correctIndex = question.CorrectIndex, explanation = question.Explanation } : null
            };
            await SendAsync(player, new NetworkMessage
            {
                Type = MessageTypes.QuestionEnded,
                RoomCode = room.RoomCode,
                QuestionIndex = questionIndex,
                Mode = room.Mode,
                Data = SerializeData(data)
            }).ConfigureAwait(false);
        }
    }

    private async Task SubmitAnswerAsync(PlayerSession session, NetworkMessage message)
    {
        var room = session.Room;
        if (room == null)
        {
            await SendAnswerResultAsync(session, message, false, "Bạn chưa ở trong phòng.").ConfigureAwait(false);
            return;
        }

        var answerIndex = message.AnswerIndex ?? -1;
        var questionIndex = message.QuestionIndex ?? -1;
        string? failure = null;
        bool isCorrect = false;
        long answerTime = 0;
        int scoreAwarded = 0;
        int totalScore = 0;
        await room.Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (session.Role == "Teacher")
            {
                failure = "Giáo viên theo dõi buổi học và không tham gia trả lời câu hỏi.";
            }
            else if (!room.Started || room.Ended || !room.QuestionActive || questionIndex != room.CurrentQuestionIndex
                || DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() >= room.CurrentQuestionEndsUnixMs)
            {
                failure = "Câu hỏi đã kết thúc hoặc không còn hiệu lực.";
            }
            else if (answerIndex < 0 || answerIndex > 3)
            {
                failure = "Đáp án không hợp lệ.";
            }
            else if (room.Answers.ContainsKey(session.PlayerId))
            {
                failure = "Bạn đã nộp đáp án cho câu này rồi.";
            }
            else
            {
                var scoredQuestion = room.Questions[room.CurrentQuestionIndex];
                isCorrect = answerIndex == scoredQuestion.CorrectIndex;
                answerTime = Math.Max(0, DateTimeOffset.UtcNow.ToUnixTimeMilliseconds() - room.CurrentQuestionStartedUnixMs);
                var timeLimitMilliseconds = room.DurationSeconds * 1000L;
                var clampedAnswerTime = Math.Min(answerTime, timeLimitMilliseconds);
                scoreAwarded = isCorrect
                    ? 1000 + (int)(1000L * (timeLimitMilliseconds - clampedAnswerTime) / timeLimitMilliseconds)
                    : 0;
                room.Answers[session.PlayerId] = new AnswerRecord
                {
                    AnswerIndex = answerIndex,
                    IsCorrect = isCorrect,
                    AnswerTimeMilliseconds = answerTime,
                    ScoreAwarded = scoreAwarded
                };
                session.SubmittedCount++;
                session.TotalAnswerTimeMilliseconds += answerTime;
                session.Score += scoreAwarded;
                if (isCorrect)
                {
                    session.CorrectCount++;
                }
                totalScore = session.Score;
            }
        }
        finally
        {
            room.Gate.Release();
        }

        if (failure != null)
        {
            await SendAnswerResultAsync(session, message, false, failure).ConfigureAwait(false);
            return;
        }

        var question = room.Questions[questionIndex];
        var feedbackData = SerializeData(new
        {
            accepted = true,
            correct = isCorrect,
            scoreAwarded,
            totalScore,
            explanation = question.Explanation,
            correctIndex = question.CorrectIndex
        });
        await SendAsync(session, new NetworkMessage
        {
            Type = MessageTypes.AnswerResult,
            RequestId = message.RequestId,
            QuestionIndex = questionIndex,
            Success = true,
            Mode = room.Mode,
            Message = room.Mode == "Study" ? (isCorrect ? "Chính xác!" : "Chưa chính xác.") : "Đã ghi nhận đáp án.",
            Data = feedbackData
        }).ConfigureAwait(false);

        await SendTeacherLeaderboardAsync(room).ConfigureAwait(false);
    }

    private async Task EndGameAsync(RoomSession room)
    {
        PlayerSession[] players;
        PlayerSession[] students;
        PlayerSession? teacher;
        var ranking = new List<object>();
        await room.Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            if (!room.Started || room.Ended)
            {
                return;
            }

            room.QuestionActive = false;
            room.Ended = true;
            players = room.Players.Values.ToArray();
            teacher = players.FirstOrDefault(player => player.Role == "Teacher");
            students = players.Where(player => player.Role != "Teacher")
                .OrderByDescending(player => player.Score)
                .ThenByDescending(player => player.CorrectCount)
                .ThenBy(player => player.TotalAnswerTimeMilliseconds)
                .ThenBy(player => player.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToArray();

            for (var i = 0; i < students.Length; i++)
            {
                ranking.Add(new
                {
                    rank = i + 1,
                    playerId = students[i].PlayerId,
                    displayName = students[i].DisplayName,
                    score = students[i].Score,
                    correctCount = students[i].CorrectCount,
                    totalQuestions = room.Questions.Count,
                    totalAnswerTimeMilliseconds = students[i].TotalAnswerTimeMilliseconds
                });
            }
        }
        finally
        {
            room.Gate.Release();
        }

        var historyRecord = new RoomHistoryRecord
        {
            Room = new RoomHistoryRoom
            {
                RoomCode = room.RoomCode,
                RoomName = room.RoomName,
                LessonTitle = room.LessonTitle,
                Mode = room.Mode,
                QuestionCount = room.Questions.Count,
                HostAccountId = room.HostAccountId,
                HostName = teacher?.DisplayName ?? "Teacher"
            },
            Details = students.Select((student, index) => new RoomHistoryDetail
            {
                AccountId = student.AccountId,
                DisplayName = student.DisplayName,
                RankPosition = index + 1,
                CorrectCount = student.CorrectCount,
                IncorrectCount = Math.Max(0, student.SubmittedCount - student.CorrectCount),
                UnansweredCount = Math.Max(0, room.Questions.Count - student.SubmittedCount),
                TotalQuestions = room.Questions.Count,
                Score = student.Score,
                ResponseTimeMs = student.TotalAnswerTimeMilliseconds
            }).ToList()
        };
        await DatabaseApi.SaveRoomAsync(historyRecord).ConfigureAwait(false);

        foreach (var student in students)
        {
            var topStudent = students.Length > 0 ? students[0] : null;
            var encouragement = student.CorrectCount == room.Questions.Count
                ? "Xuất sắc! Bạn đã hoàn thành tất cả câu hỏi."
                : student.CorrectCount >= (room.Questions.Count + 1) / 2
                    ? "Làm tốt lắm! Hãy tiếp tục luyện tập nhé."
                    : "Cảm ơn bạn đã tham gia. Hãy thử lại ở buổi học sau nhé.";
            await SendAsync(student, new NetworkMessage
            {
                Type = MessageTypes.StudentPersonalResult,
                RoomCode = room.RoomCode,
                Data = SerializeData(new
                {
                    rankPosition = Array.IndexOf(students, student) + 1,
                    totalStudents = students.Length,
                    score = student.Score,
                    correctCount = student.CorrectCount,
                    incorrectCount = Math.Max(0, student.SubmittedCount - student.CorrectCount),
                    unansweredCount = Math.Max(0, room.Questions.Count - student.SubmittedCount),
                    totalQuestions = room.Questions.Count,
                    totalAnswerTimeMilliseconds = student.TotalAnswerTimeMilliseconds,
                    topStudentName = topStudent?.DisplayName ?? "Chưa có người trả lời",
                    topStudentScore = topStudent?.Score ?? 0,
                    encouragement
                })
            }).ConfigureAwait(false);
        }

        if (teacher != null)
        {
            await SendAsync(teacher, new NetworkMessage
            {
                Type = MessageTypes.TeacherLeaderboard,
                RoomCode = room.RoomCode,
                Data = SerializeData(new
                {
                    students = ranking,
                    totalQuestions = room.Questions.Count,
                    questionIndex = -1,
                    answeredCount = students.Sum(student => student.SubmittedCount),
                    correctCount = students.Sum(student => student.CorrectCount)
                })
            }).ConfigureAwait(false);
        }

        await BroadcastAsync(room, new NetworkMessage
        {
            Type = MessageTypes.GameEnded,
            RoomCode = room.RoomCode,
            Message = "Buổi quiz đã kết thúc."
        }).ConfigureAwait(false);
    }

    private async Task EndSessionAsync(PlayerSession session, NetworkMessage message)
    {
        var room = session.Room;
        if (room == null)
        {
            await SendErrorAsync(session, message, "Bạn chưa ở trong phòng.").ConfigureAwait(false);
            return;
        }
        if (session.Role != "Teacher" || room.HostPlayerId != session.PlayerId)
        {
            await SendErrorAsync(session, message, "Chỉ giáo viên mới được kết thúc buổi học.").ConfigureAwait(false);
            return;
        }

        PlayerSession[] players;
        await room.Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            players = room.Players.Values.ToArray();
            room.Ended = true;
            room.Started = false;
            room.QuestionActive = false;
            room.GameCancellation.Cancel();
        }
        finally
        {
            room.Gate.Release();
        }

        var returnMessage = new NetworkMessage
        {
            Type = MessageTypes.ReturnToMenu,
            RoomCode = room.RoomCode,
            Message = "Giáo viên đã kết thúc buổi học."
        };
        foreach (var player in players)
        {
            await SendAsync(player, returnMessage).ConfigureAwait(false);
        }

        foreach (var player in players)
        {
            player.Room = null;
            player.Role = "Student";
        }
        RemoveRoom(room);
        Console.WriteLine($"Teacher {session.DisplayName} ended room {room.RoomCode}.");
    }

    private async Task DisconnectAsync(PlayerSession session)
    {
        _sessions.TryRemove(session.PlayerId, out _);
        if (session.Room != null)
        {
            await LeaveRoomAsync(session, new NetworkMessage()).ConfigureAwait(false);
        }

        try
        {
            session.Stream.Dispose();
            session.Client.Dispose();
        }
        catch
        {
        }
    }

    private async Task BroadcastRoomStateAsync(RoomSession room)
    {
        PlayerSession[] players;
        object[] playerInfos;
        string roomCode;
        string roomName;
        int lessonId;
        string lessonTitle;
        string mode;
        int duration;
        int capacity;
        bool started;
        await room.Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            players = room.Players.Values.ToArray();
            playerInfos = players.Select(player => (object)new { playerId = player.PlayerId, displayName = player.DisplayName, role = player.Role }).ToArray();
            roomCode = room.RoomCode;
            roomName = room.RoomName;
            lessonId = room.LessonId;
            lessonTitle = room.LessonTitle;
            mode = room.Mode;
            duration = room.DurationSeconds;
            capacity = room.StudentCapacity;
            started = room.Started;
        }
        finally
        {
            room.Gate.Release();
        }

        foreach (var player in players)
        {
            await SendAsync(player, new NetworkMessage
            {
                Type = MessageTypes.RoomStateUpdate,
                RoomCode = roomCode,
                RoomName = roomName,
                Role = player.Role,
                LessonId = lessonId,
                LessonTitle = lessonTitle,
                Mode = mode,
                DurationSeconds = duration,
                Data = SerializeData(new
                {
                    roomCode,
                    roomName,
                    selfRole = player.Role,
                    players = playerInfos,
                    count = playerInfos.Length,
                    studentCapacity = capacity,
                    lessonId,
                    lessonTitle,
                    mode,
                    durationSeconds = duration,
                    questionCount = room.Questions.Count,
                    started
                })
            }).ConfigureAwait(false);
        }
    }

    private async Task SendTeacherLeaderboardAsync(RoomSession room)
    {
        PlayerSession? teacher;
        object[] students;
        int answeredCount;
        int correctCount;
        int questionIndex;
        await room.Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            teacher = room.Players.Values.FirstOrDefault(player => player.Role == "Teacher");
            var orderedStudents = room.Players.Values
                .Where(player => player.Role != "Teacher")
                .OrderByDescending(player => player.Score)
                .ThenByDescending(player => player.CorrectCount)
                .ThenBy(player => player.TotalAnswerTimeMilliseconds)
                .ThenBy(player => player.DisplayName, StringComparer.OrdinalIgnoreCase)
                .ToArray();
            students = orderedStudents.Select((player, index) => (object)new
            {
                rank = index + 1,
                playerId = player.PlayerId,
                displayName = player.DisplayName,
                score = player.Score,
                correctCount = player.CorrectCount,
                totalQuestions = room.Questions.Count,
                totalAnswerTimeMilliseconds = player.TotalAnswerTimeMilliseconds
            }).ToArray();
            answeredCount = room.Answers.Count;
            correctCount = room.Answers.Values.Count(answer => answer.IsCorrect);
            questionIndex = room.CurrentQuestionIndex;
        }
        finally
        {
            room.Gate.Release();
        }

        if (teacher == null)
        {
            return;
        }

        await SendAsync(teacher, new NetworkMessage
        {
            Type = MessageTypes.TeacherLeaderboard,
            RoomCode = room.RoomCode,
            Data = SerializeData(new
            {
                students,
                totalQuestions = room.Questions.Count,
                questionIndex,
                answeredCount,
                correctCount
            })
        }).ConfigureAwait(false);
    }

    private static async Task BroadcastAsync(RoomSession room, NetworkMessage message)
    {
        PlayerSession[] players;
        await room.Gate.WaitAsync().ConfigureAwait(false);
        try
        {
            players = room.Players.Values.ToArray();
        }
        finally
        {
            room.Gate.Release();
        }

        foreach (var player in players)
        {
            await SendAsync(player, message).ConfigureAwait(false);
        }
    }

    private async Task SendCreateResultAsync(PlayerSession session, NetworkMessage request, bool success, string message, string? roomCode = null)
    {
        await SendAsync(session, new NetworkMessage
        {
            Type = MessageTypes.CreateRoomResult,
            RequestId = request.RequestId,
            Success = success,
            RoomCode = roomCode,
            Message = message,
            Error = success ? null : message
        }).ConfigureAwait(false);
    }

    private async Task SendJoinResultAsync(PlayerSession session, NetworkMessage request, bool success, string message, string? roomCode = null)
    {
        await SendAsync(session, new NetworkMessage
        {
            Type = MessageTypes.JoinRoomResult,
            RequestId = request.RequestId,
            Success = success,
            RoomCode = roomCode,
            Message = message,
            Error = success ? null : message,
            Role = success ? session.Role : null
        }).ConfigureAwait(false);
    }

    private async Task SendAnswerResultAsync(PlayerSession session, NetworkMessage request, bool success, string message)
    {
        await SendAsync(session, new NetworkMessage
        {
            Type = MessageTypes.AnswerResult,
            RequestId = request.RequestId,
            QuestionIndex = request.QuestionIndex,
            Success = success,
            Message = message,
            Error = success ? null : message
        }).ConfigureAwait(false);
    }

    private static Task SendErrorAsync(PlayerSession session, NetworkMessage request, string error)
    {
        return SendAsync(session, new NetworkMessage
        {
            Type = MessageTypes.Error,
            RequestId = request.RequestId,
            Error = error,
            Message = error
        });
    }

    private static async Task SendAsync(PlayerSession session, NetworkMessage message)
    {
        var payload = JsonSerializer.SerializeToUtf8Bytes(message, SerializerOptions);
        if (payload.Length == 0 || payload.Length > MaximumMessageBytes)
        {
            throw new InvalidOperationException("Outgoing TCP message exceeds the size limit.");
        }

        var buffer = new byte[payload.Length + 4];
        buffer[0] = (byte)(payload.Length & 0xff);
        buffer[1] = (byte)((payload.Length >> 8) & 0xff);
        buffer[2] = (byte)((payload.Length >> 16) & 0xff);
        buffer[3] = (byte)((payload.Length >> 24) & 0xff);
        Buffer.BlockCopy(payload, 0, buffer, 4, payload.Length);

        await session.WriteLock.WaitAsync().ConfigureAwait(false);
        try
        {
            await session.Stream.WriteAsync(buffer).ConfigureAwait(false);
            await session.Stream.FlushAsync().ConfigureAwait(false);
        }
        finally
        {
            session.WriteLock.Release();
        }
    }

    private static async Task<NetworkMessage?> ReadMessageAsync(NetworkStream stream)
    {
        var header = new byte[4];
        if (!await ReadExactlyAsync(stream, header).ConfigureAwait(false))
        {
            return null;
        }

        var payloadLength = header[0] | (header[1] << 8) | (header[2] << 16) | (header[3] << 24);
        if (payloadLength <= 0 || payloadLength > MaximumMessageBytes)
        {
            throw new InvalidDataException($"TCP message length is out of range: {payloadLength}.");
        }

        var payload = new byte[payloadLength];
        if (!await ReadExactlyAsync(stream, payload).ConfigureAwait(false))
        {
            throw new EndOfStreamException("Client disconnected in the middle of a message.");
        }

        return JsonSerializer.Deserialize<NetworkMessage>(Encoding.UTF8.GetString(payload), SerializerOptions)
            ?? throw new JsonException("Empty network message.");
    }

    private static async Task<bool> ReadExactlyAsync(NetworkStream stream, byte[] buffer)
    {
        var offset = 0;
        while (offset < buffer.Length)
        {
            var bytesRead = await stream.ReadAsync(buffer.AsMemory(offset, buffer.Length - offset)).ConfigureAwait(false);
            if (bytesRead == 0)
            {
                return false;
            }
            offset += bytesRead;
        }
        return true;
    }

    private static string? ValidateQuestions(IReadOnlyList<QuestionDefinition> questions)
    {
        for (var i = 0; i < questions.Count; i++)
        {
            var question = questions[i];
            if (string.IsNullOrWhiteSpace(question.Prompt))
            {
                return $"Câu {i + 1}: nội dung câu hỏi không được để trống.";
            }
            if (question.Options == null || question.Options.Count != 4 || question.Options.Any(string.IsNullOrWhiteSpace))
            {
                return $"Câu {i + 1}: cần đủ 4 đáp án có nội dung.";
            }
            if (question.CorrectIndex is < 0 or > 3)
            {
                return $"Câu {i + 1}: đáp án đúng không hợp lệ.";
            }
            question.SequenceIndex = i + 1;
        }
        return null;
    }

    private static QuestionDefinition CloneQuestion(QuestionDefinition source)
    {
        return new QuestionDefinition
        {
            LessonId = source.LessonId,
            LessonTitle = source.LessonTitle,
            QuestionId = string.IsNullOrWhiteSpace(source.QuestionId) ? Guid.NewGuid().ToString("N") : source.QuestionId,
            SequenceIndex = source.SequenceIndex,
            Prompt = source.Prompt.Trim(),
            Options = source.Options?.Select(option => option.Trim()).ToList() ?? new List<string>(),
            CorrectIndex = source.CorrectIndex,
            Explanation = source.Explanation?.Trim() ?? string.Empty
        };
    }

    private static string NormalizeName(string? name)
    {
        var result = string.IsNullOrWhiteSpace(name) ? "Guest" : name.Trim();
        if (result.Length > 24)
        {
            result = result[..24];
        }
        return result;
    }

    private static string NormalizeRoomName(string? roomName, string lessonTitle)
    {
        var result = string.IsNullOrWhiteSpace(roomName) ? lessonTitle.Trim() : roomName.Trim();
        return result.Length > 120 ? result[..120] : result;
    }

    private static string? NormalizeMode(string? mode)
    {
        if (string.Equals(mode, "Study", StringComparison.OrdinalIgnoreCase) || string.Equals(mode, "Học", StringComparison.OrdinalIgnoreCase))
        {
            return "Study";
        }
        if (string.Equals(mode, "Exam", StringComparison.OrdinalIgnoreCase) || string.Equals(mode, "Thi", StringComparison.OrdinalIgnoreCase))
        {
            return "Exam";
        }
        return null;
    }

    private static string GenerateRoomCode()
    {
        Span<char> code = stackalloc char[6];
        for (var i = 0; i < code.Length; i++)
        {
            code[i] = RoomAlphabet[RandomNumberGenerator.GetInt32(RoomAlphabet.Length)];
        }
        return new string(code);
    }

    private static string SerializeData<T>(T value)
    {
        return JsonSerializer.Serialize(value, SerializerOptions);
    }

    private static readonly JsonSerializerOptions SerializerOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        DefaultIgnoreCondition = JsonIgnoreCondition.WhenWritingNull,
        MaxDepth = 24
    };

    private void RemoveRoom(RoomSession room)
    {
        _rooms.TryRemove(room.RoomCode, out _);
        room.GameCancellation.Cancel();
    }
}

internal static class Program
{
    private const int DefaultPort = 7777;

    public static async Task Main(string[] args)
    {
        var address = Environment.GetEnvironmentVariable("CLASSROOM_QUIZ_BIND_IP") ?? "0.0.0.0";
        var port = ReadIntEnvironment("CLASSROOM_QUIZ_PORT", DefaultPort, 1, 65535);
        var studentCapacity = ReadIntEnvironment("CLASSROOM_QUIZ_MAX_STUDENTS", 40, 1, 200);
        for (var i = 0; i < args.Length; i++)
        {
            if (args[i] == "--ip" && i + 1 < args.Length)
            {
                address = args[++i];
            }
            else if (args[i] == "--port" && i + 1 < args.Length && int.TryParse(args[++i], out var parsedPort))
            {
                port = Math.Clamp(parsedPort, 1, 65535);
            }
        }

        if (!IPAddress.TryParse(address, out var ipAddress))
        {
            Console.Error.WriteLine($"Invalid bind address: {address}");
            Environment.ExitCode = 2;
            return;
        }

        var server = new TcpGameServer(ipAddress, port, studentCapacity);
        Console.WriteLine("ClassroomQuiz dedicated TCP server starting.");
        Console.WriteLine("Press Ctrl+C to stop the server.");
        using var cancellation = new CancellationTokenSource();
        Console.CancelKeyPress += (_, eventArgs) =>
        {
            eventArgs.Cancel = true;
            cancellation.Cancel();
        };

        await server.StartAsync(cancellation.Token).ConfigureAwait(false);
    }

    private static int ReadIntEnvironment(string name, int fallback, int minimum, int maximum)
    {
        var value = Environment.GetEnvironmentVariable(name);
        return int.TryParse(value, out var parsed) ? Math.Clamp(parsed, minimum, maximum) : fallback;
    }
}
