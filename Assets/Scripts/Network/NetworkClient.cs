using System;
using System.Collections.Concurrent;
using System.Net.WebSockets;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEngine;

public class NetworkClient : MonoBehaviour
{
    public static NetworkClient Instance { get; private set; }

    [SerializeField] private string serverIP = "127.0.0.1";
    [SerializeField] private int serverPort = 8181;

    public string ServerIP => serverIP;
    public int ServerPort => serverPort;

    public string ConnectedIP { get; private set; }
    public bool IsConnected => webSocket != null && webSocket.State == WebSocketState.Open;

    public event Action<string> OnMessageReceived;

    private ClientWebSocket webSocket;
    private ConcurrentQueue<string> messageQueue = new ConcurrentQueue<string>();
    private CancellationTokenSource cancellationTokenSource;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private void Update()
    {
        while (messageQueue.TryDequeue(out string message))
        {
            OnMessageReceived?.Invoke(message);
        }
    }

    public async void Connect(string ip, int port)
    {
        if (IsConnected) return;

        serverIP = ip;
        serverPort = port;
        ConnectedIP = $"{ip}:{port}";

        webSocket = new ClientWebSocket();
        cancellationTokenSource = new CancellationTokenSource();

        Uri serverUri = new Uri($"ws://{ip}:{port}");
        
        try
        {
            await webSocket.ConnectAsync(serverUri, cancellationTokenSource.Token);
            Debug.Log($"Connected to {serverUri}");
            
            // Start receiving messages in the background
            _ = ReceiveMessagesAsync();
        }
        catch (Exception e)
        {
            Debug.LogError($"Connection error: {e.Message}");
        }
    }

    public new async void SendMessage(string json)
    {
        if (!IsConnected)
        {
            Debug.LogError("Cannot send message: Not connected.");
            return;
        }

        try
        {
            byte[] bytes = Encoding.UTF8.GetBytes(json);
            ArraySegment<byte> buffer = new ArraySegment<byte>(bytes);
            await webSocket.SendAsync(buffer, WebSocketMessageType.Text, true, cancellationTokenSource.Token);
        }
        catch (Exception e)
        {
            Debug.LogError($"Error sending message: {e.Message}");
        }
    }

    private async Task ReceiveMessagesAsync()
    {
        byte[] buffer = new byte[8192];

        try
        {
            while (IsConnected && !cancellationTokenSource.Token.IsCancellationRequested)
            {
                ArraySegment<byte> segment = new ArraySegment<byte>(buffer);
                WebSocketReceiveResult result = await webSocket.ReceiveAsync(segment, cancellationTokenSource.Token);

                if (result.MessageType == WebSocketMessageType.Close)
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, string.Empty, CancellationToken.None);
                    Debug.Log("WebSocket connection closed by server.");
                }
                else if (result.MessageType == WebSocketMessageType.Text)
                {
                    string message = Encoding.UTF8.GetString(buffer, 0, result.Count);
                    
                    if (!result.EndOfMessage)
                    {
                        // Handle fragmented messages
                        StringBuilder sb = new StringBuilder(message);
                        while (!result.EndOfMessage)
                        {
                            result = await webSocket.ReceiveAsync(segment, cancellationTokenSource.Token);
                            sb.Append(Encoding.UTF8.GetString(buffer, 0, result.Count));
                        }
                        message = sb.ToString();
                    }

                    messageQueue.Enqueue(message);
                }
            }
        }
        catch (OperationCanceledException)
        {
            // Expected when cancellation is requested
        }
        catch (Exception e)
        {
            Debug.LogError($"Error receiving message: {e.Message}");
        }
    }

    public async void Disconnect()
    {
        if (webSocket != null)
        {
            if (webSocket.State == WebSocketState.Open)
            {
                try
                {
                    await webSocket.CloseAsync(WebSocketCloseStatus.NormalClosure, "Client disconnected", CancellationToken.None);
                }
                catch (Exception e)
                {
                    Debug.LogError($"Error closing connection: {e.Message}");
                }
            }
            
            if (cancellationTokenSource != null)
            {
                cancellationTokenSource.Cancel();
                cancellationTokenSource.Dispose();
                cancellationTokenSource = null;
            }
            
            webSocket.Dispose();
            webSocket = null;
            
            ConnectedIP = null;
            Debug.Log("Disconnected.");
        }
    }

    private void OnDestroy()
    {
        Disconnect();
    }
}
