using System;
using System.Collections.Generic;
using System.Net.Sockets;
using System.Threading;
using UnityEngine;

/// <summary>
/// Quản lý kết nối mạng TCP socket với server.
/// Xử lý gửi/nhận packet đa luồng và điều phối (dispatch) về Main Thread của Unity.
/// </summary>
public class NetworkClient : MonoBehaviour
{
    public static NetworkClient Instance { get; private set; }

    [Header("Cấu hình kết nối Server")]
    [SerializeField] private string serverIP = "127.0.0.1";
    [SerializeField] private int serverPort = 7777;

    // Các trường phục vụ kết nối mạng
    private TcpClient tcpClient;
    private NetworkStream networkStream;
    private Thread receiveThread;
    private bool isConnected;

    // Hàng đợi dispatch action về Unity Main Thread
    private readonly Queue<Action> mainThreadActions = new Queue<Action>();
    private readonly object queueLock = new object();

    public bool IsConnected => isConnected;
    public string ServerIP => serverIP;
    public int ServerPort => serverPort;

    private void Awake()
    {
        // Khởi tạo Singleton pattern và giữ object tồn tại qua các Scene
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
        // TODO: Lấy các Action từ mainThreadActions (cần lock queueLock) và thực thi trên Unity Main Thread
    }

    /// <summary>
    /// Bắt đầu kết nối đến TCP Server.
    /// </summary>
    /// <param name="ip">Địa chỉ IP của server</param>
    /// <param name="port">Cổng (port) của server</param>
    public void Connect(string ip, int port)
    {
        // TODO: Thiết lập kết nối TcpClient với ip và port
        // TODO: Lấy NetworkStream và khởi tạo receiveThread để chạy ReceiveLoop()
        // TODO: Cập nhật trạng thái isConnected = true
    }

    /// <summary>
    /// Ngắt kết nối khỏi server và giải phóng tài nguyên.
    /// </summary>
    public void Disconnect()
    {
        // TODO: Đóng kết nối networkStream và tcpClient
        // TODO: Dừng receiveThread một cách an toàn
        // TODO: Đặt isConnected = false
    }

    /// <summary>
    /// Gửi packet dữ liệu lên server.
    /// </summary>
    /// <param name="type">Loại packet (PacketType)</param>
    /// <param name="jsonPayload">Chuỗi nội dung JSON của packet</param>
    public void SendPacket(PacketType type, string jsonPayload)
    {
        // TODO: Sử dụng PacketSerializer.Serialize(type, jsonPayload) để đóng gói thành mảng byte
        // TODO: Ghi dữ liệu mảng byte vào networkStream
    }

    /// <summary>
    /// Vòng lặp liên tục đọc dữ liệu nhận được từ server, chạy trên Thread riêng biệt.
    /// </summary>
    private void ReceiveLoop()
    {
        // TODO: Đọc luồng byte từ networkStream theo định dạng packet [4 bytes độ dài][2 bytes PacketType][payload]
        // TODO: Giải mã dữ liệu nhận được bằng PacketSerializer.Deserialize
        // TODO: Đưa callback OnPacketReceived vào mainThreadActions (lock queueLock) để Main Thread thực thi
    }

    /// <summary>
    /// Xử lý packet nhận được trên Main Thread.
    /// </summary>
    /// <param name="type">Loại packet</param>
    /// <param name="jsonPayload">Nội dung JSON của packet</param>
    public void OnPacketReceived(PacketType type, string jsonPayload)
    {
        // TODO: Chuyển tiếp packet nhận được sang PacketHandler.Instance.HandlePacket(type, jsonPayload)
    }

    private void OnApplicationQuit()
    {
        // Tự động ngắt kết nối khi tắt ứng dụng
        Disconnect();
    }
}
