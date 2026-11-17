using System;
using System.Collections.Concurrent;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

namespace ClassroomQuiz.Network
{
    [Serializable]
    public sealed class NetworkMessage
    {
        public int protocolVersion = 1;
        public string type = string.Empty;
        public string requestId = string.Empty;
        public string roomCode = string.Empty;
        public string roomName = string.Empty;
        public string playerName = string.Empty;
        public string authToken = string.Empty;
        public long accountId;
        public string role = string.Empty;
        public string mode = string.Empty;
        public string lessonTitle = string.Empty;
        public int lessonId;
        public int questionIndex = -1;
        public int answerIndex = -1;
        public int durationSeconds;
        public string error = string.Empty;
        public bool success;
        public string message = string.Empty;
        public string payload = string.Empty;
        public string data = string.Empty;
        public string serverAddress = string.Empty;
        public int serverPort;

        public int ProtocolVersion { get => protocolVersion; set => protocolVersion = value; }
        public string Type { get => type; set => type = value; }
        public string RequestId { get => requestId; set => requestId = value ?? string.Empty; }
        public string RoomCode { get => roomCode; set => roomCode = value ?? string.Empty; }
        public string RoomName { get => roomName; set => roomName = value ?? string.Empty; }
        public string PlayerName { get => playerName; set => playerName = value ?? string.Empty; }
        public string AuthToken { get => authToken; set => authToken = value ?? string.Empty; }
        public long AccountId { get => accountId > 0 ? accountId : 0; set => accountId = value; }
        public string Role { get => role; set => role = value ?? string.Empty; }
        public string Mode { get => mode; set => mode = value ?? string.Empty; }
        public string LessonTitle { get => lessonTitle; set => lessonTitle = value ?? string.Empty; }
        public int? LessonId { get => lessonId == 0 ? (int?)null : lessonId; set => lessonId = value ?? 0; }
        public int? QuestionIndex { get => questionIndex < 0 ? (int?)null : questionIndex; set => questionIndex = value ?? -1; }
        public int? AnswerIndex { get => answerIndex < 0 ? (int?)null : answerIndex; set => answerIndex = value ?? -1; }
        public int? DurationSeconds { get => durationSeconds == 0 ? (int?)null : durationSeconds; set => durationSeconds = value ?? 0; }
        public string Error { get => error; set => error = value ?? string.Empty; }
        public bool? Success { get => success; set => success = value == true; }
        public string Message { get => message; set => message = value ?? string.Empty; }
        public string Payload { get => payload; set => payload = value ?? string.Empty; }
        public string Data { get => data; set => data = value ?? string.Empty; }
        public string ServerAddress { get => serverAddress; set => serverAddress = value ?? string.Empty; }
        public int? ServerPort { get => serverPort == 0 ? (int?)null : serverPort; set => serverPort = value ?? 0; }
    }

    public static class NetworkMessageSerializer
    {
        public const int CurrentProtocolVersion = 1;

        public static string Serialize(NetworkMessage message)
        {
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            message.ProtocolVersion = CurrentProtocolVersion;
            return JsonUtility.ToJson(message);
        }

        public static NetworkMessage Deserialize(string json)
        {
            if (string.IsNullOrWhiteSpace(json))
            {
                return null;
            }

            try
            {
                return JsonUtility.FromJson<NetworkMessage>(json);
            }
            catch (ArgumentException exception)
            {
                Debug.LogWarning($"TCP message JSON was invalid: {exception.Message}");
                return null;
            }
        }
    }

    public enum ConnectionState
    {
        Disconnected,
        Connecting,
        Connected,
        Failed
    }

    public sealed class TcpGameClient : MonoBehaviour
    {
        public const int MaximumMessageBytes = 1_048_576;
        private static TcpGameClient _instance;

        private readonly ConcurrentQueue<Action> _mainThreadActions = new ConcurrentQueue<Action>();
        private readonly SemaphoreSlim _writeLock = new SemaphoreSlim(1, 1);

        private TcpClient _client;
        private NetworkStream _stream;
        private CancellationTokenSource _lifetime;
        private Task _receiveLoopTask;
        private TaskCompletionSource<NetworkMessage> _welcomeCompletion;
        private float _lastPingTime;
        private bool _destroyed;

        public static TcpGameClient Instance => _instance;
        public ConnectionState State { get; private set; } = ConnectionState.Disconnected;
        public string ServerAddress { get; private set; } = "127.0.0.1";
        public int ServerPort { get; private set; } = 7777;
        public string PlayerName { get; private set; } = "Guest";
        public bool IsConnected => State == ConnectionState.Connected && _client != null && _stream != null;

        public event Action<NetworkMessage> MessageReceived;
        public event Action<string> ErrorReceived;
        public event Action<ConnectionState> StateChanged;

        private void Awake()
        {
            if (_instance != null && _instance != this)
            {
                Destroy(gameObject);
                return;
            }

            _instance = this;
            DontDestroyOnLoad(gameObject);
        }

        public async Task<bool> ConnectAsync(string address, int port, string playerName, int timeoutMilliseconds = 5000)
        {
            if (IsConnected)
            {
                return true;
            }

            if (State == ConnectionState.Connecting)
            {
                return false;
            }

            Disconnect(false);
            if (string.IsNullOrWhiteSpace(address))
            {
                address = "127.0.0.1";
            }
            if (port < 1 || port > 65535)
            {
                throw new ArgumentOutOfRangeException(nameof(port), "Cổng TCP phải nằm trong khoảng 1–65535.");
            }

            ServerAddress = address.Trim();
            ServerPort = port;
            PlayerName = NormalizePlayerName(playerName);
            SetState(ConnectionState.Connecting);

            _client = new TcpClient();
            _client.NoDelay = true;
            _welcomeCompletion = new TaskCompletionSource<NetworkMessage>(TaskCreationOptions.RunContinuationsAsynchronously);
            _lifetime = new CancellationTokenSource();

            try
            {
                var connectTask = _client.ConnectAsync(ServerAddress, ServerPort);
                var connectWinner = await Task.WhenAny(connectTask, Task.Delay(timeoutMilliseconds));
                if (connectWinner != connectTask)
                {
                    throw new TimeoutException("Kết nối TCP quá thời gian chờ.");
                }

                await connectTask;
                _stream = _client.GetStream();
                _receiveLoopTask = ReceiveLoopAsync(_lifetime.Token);

                await SendAsync(new NetworkMessage
                {
                    Type = "Hello",
                    PlayerName = PlayerName,
                    AuthToken = PlayerPrefs.GetString(PhpApiClient.AccountTokenPreferenceKey, string.Empty),
                    RequestId = Guid.NewGuid().ToString("N")
                });

                var welcomeWinner = await Task.WhenAny(_welcomeCompletion.Task, Task.Delay(timeoutMilliseconds));
                if (welcomeWinner != _welcomeCompletion.Task)
                {
                    throw new TimeoutException("Máy chủ không phản hồi thông điệp Welcome đúng hạn.");
                }

                var welcome = await _welcomeCompletion.Task;
                if (welcome.Success != true)
                {
                    throw new InvalidOperationException(string.IsNullOrWhiteSpace(welcome.Error)
                        ? "Tài khoản không được máy chủ xác thực."
                        : welcome.Error);
                }
                if (welcome.ProtocolVersion != NetworkMessageSerializer.CurrentProtocolVersion)
                {
                    throw new InvalidOperationException($"Phiên bản giao thức không tương thích (client {NetworkMessageSerializer.CurrentProtocolVersion}, server {welcome.ProtocolVersion}).");
                }

                SetState(ConnectionState.Connected);
                _lastPingTime = Time.unscaledTime;
                return true;
            }
            catch (Exception exception)
            {
                var message = exception is SocketException
                    ? $"Không thể kết nối {ServerAddress}:{ServerPort}. Hãy kiểm tra địa chỉ và bảo đảm TCP server đang chạy."
                    : exception.Message;
                Disconnect(false);
                SetState(ConnectionState.Failed);
                ErrorReceived?.Invoke(message);
                return false;
            }
        }

        public async Task SendAsync(NetworkMessage message)
        {
            if (!IsConnected && State != ConnectionState.Connecting)
            {
                throw new InvalidOperationException("TCP client chưa kết nối tới server.");
            }
            if (message == null)
            {
                throw new ArgumentNullException(nameof(message));
            }

            var json = NetworkMessageSerializer.Serialize(message);
            var payload = Encoding.UTF8.GetBytes(json);
            if (payload.Length == 0 || payload.Length > MaximumMessageBytes)
            {
                throw new InvalidOperationException("TCP message vượt quá giới hạn kích thước.");
            }

            var frame = new byte[payload.Length + 4];
            frame[0] = (byte)(payload.Length & 0xff);
            frame[1] = (byte)((payload.Length >> 8) & 0xff);
            frame[2] = (byte)((payload.Length >> 16) & 0xff);
            frame[3] = (byte)((payload.Length >> 24) & 0xff);
            Buffer.BlockCopy(payload, 0, frame, 4, payload.Length);

            await _writeLock.WaitAsync();
            try
            {
                if (_stream == null || _lifetime == null || _lifetime.IsCancellationRequested)
                {
                    throw new IOException("TCP kết nối đã đóng.");
                }

                await _stream.WriteAsync(frame, 0, frame.Length);
                await _stream.FlushAsync();
            }
            finally
            {
                _writeLock.Release();
            }
        }

        private async Task ReceiveLoopAsync(CancellationToken cancellationToken)
        {
            var header = new byte[4];
            try
            {
                while (!cancellationToken.IsCancellationRequested && _stream != null)
                {
                    if (!await ReadExactlyAsync(_stream, header, cancellationToken))
                    {
                        break;
                    }

                    var payloadLength = header[0] | (header[1] << 8) | (header[2] << 16) | (header[3] << 24);
                    if (payloadLength <= 0 || payloadLength > MaximumMessageBytes)
                    {
                        throw new InvalidOperationException($"TCP message size {payloadLength} nằm ngoài giới hạn.");
                    }

                    var payload = new byte[payloadLength];
                    if (!await ReadExactlyAsync(_stream, payload, cancellationToken))
                    {
                        throw new IOException("TCP stream đóng giữa một message.");
                    }

                    var message = NetworkMessageSerializer.Deserialize(Encoding.UTF8.GetString(payload));
                    if (message == null || string.IsNullOrWhiteSpace(message.Type))
                    {
                        throw new InvalidOperationException("Server gửi message JSON không hợp lệ.");
                    }

                    if (string.Equals(message.Type, "Welcome", StringComparison.Ordinal))
                    {
                        _welcomeCompletion?.TrySetResult(message);
                    }

                    _mainThreadActions.Enqueue(() => MessageReceived?.Invoke(message));
                }

                if (!cancellationToken.IsCancellationRequested)
                {
                    QueueDisconnected("Máy chủ đã đóng kết nối.");
                }
            }
            catch (OperationCanceledException)
            {
            }
            catch (ObjectDisposedException)
            {
            }
            catch (Exception exception)
            {
                if (!cancellationToken.IsCancellationRequested)
                {
                    QueueDisconnected($"Kết nối TCP bị gián đoạn: {exception.Message}");
                }
            }
        }

        private static async Task<bool> ReadExactlyAsync(NetworkStream stream, byte[] buffer, CancellationToken cancellationToken)
        {
            var offset = 0;
            while (offset < buffer.Length)
            {
                cancellationToken.ThrowIfCancellationRequested();
                var bytesRead = await stream.ReadAsync(buffer, offset, buffer.Length - offset, cancellationToken);
                if (bytesRead == 0)
                {
                    return false;
                }
                offset += bytesRead;
            }
            return true;
        }

        private void Update()
        {
            while (_mainThreadActions.TryDequeue(out var action))
            {
                action?.Invoke();
            }

            if (IsConnected && Time.unscaledTime - _lastPingTime >= 15f)
            {
                _lastPingTime = Time.unscaledTime;
                _ = SendPingAsync();
            }
        }

        private async Task SendPingAsync()
        {
            try
            {
                await SendAsync(new NetworkMessage
                {
                    Type = "Ping",
                    RequestId = Guid.NewGuid().ToString("N")
                });
            }
            catch (Exception exception)
            {
                QueueDisconnected($"Không gửi được heartbeat TCP: {exception.Message}");
            }
        }

        public void Disconnect()
        {
            Disconnect(true);
        }

        private void Disconnect(bool publishState)
        {
            try
            {
                _lifetime?.Cancel();
                _stream?.Close();
                _client?.Close();
            }
            catch (Exception exception)
            {
                Debug.LogWarning($"Lỗi khi đóng TCP connection: {exception.Message}");
            }

            _lifetime?.Dispose();
            _lifetime = null;
            _stream = null;
            _client = null;
            _receiveLoopTask = null;
            _welcomeCompletion?.TrySetCanceled();
            _welcomeCompletion = null;
            if (publishState)
            {
                SetState(ConnectionState.Disconnected);
            }
            else
            {
                State = ConnectionState.Disconnected;
            }
        }

        private void QueueDisconnected(string error)
        {
            if (_destroyed)
            {
                return;
            }

            _mainThreadActions.Enqueue(() =>
            {
                if (State == ConnectionState.Disconnected)
                {
                    return;
                }
                Disconnect(false);
                SetState(ConnectionState.Failed);
                ErrorReceived?.Invoke(error);
            });
        }

        private void SetState(ConnectionState newState)
        {
            if (State == newState)
            {
                return;
            }

            State = newState;
            StateChanged?.Invoke(newState);
        }

        private static string NormalizePlayerName(string value)
        {
            var normalized = string.IsNullOrWhiteSpace(value) ? "Guest" : value.Trim();
            return normalized.Length > 24 ? normalized.Substring(0, 24) : normalized;
        }

        private void OnApplicationQuit()
        {
            Disconnect(false);
        }

        private void OnDestroy()
        {
            if (_instance == this)
            {
                _instance = null;
            }
            _destroyed = true;
            Disconnect(false);
        }
    }
}
