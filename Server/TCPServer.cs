using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading;

namespace ClassroomQuiz.Server
{
    /// <summary>
    /// Định nghĩa các loại Packet trao đổi giữa Client và Server.
    /// Bản sao đồng bộ hoàn toàn với phía Unity Client (Assets/Scripts/Network/PacketType.cs).
    /// </summary>
    public enum PacketType : ushort
    {
        // === Kết nối ===
        C_CONNECT           = 1,
        S_CONNECT_OK        = 2,

        // === Phòng ===
        C_CREATE_ROOM       = 3,
        S_ROOM_CREATED      = 4,
        C_REQUEST_ROOMS     = 5,
        S_ROOM_LIST         = 6,
        C_JOIN_ROOM         = 7,
        S_JOIN_OK           = 8,
        S_JOIN_FAIL         = 9,
        S_PLAYER_JOINED     = 10,
        S_PLAYER_LEFT       = 11,
        C_LEAVE_ROOM        = 12,

        // === Game ===
        C_START_GAME        = 13,
        S_GAME_START        = 14,
        S_SHOW_QUESTION     = 15,
        C_RAISE_HAND        = 16,
        S_HAND_WINNER       = 17,
        C_SUBMIT_ANSWER     = 18,
        S_ANSWER_RESULT     = 19,
        S_TURN_PASSED       = 20,
        S_REBUZZ            = 21,
        S_ROUND_END         = 22,
        S_GAME_END          = 23,

        // === Replay ===
        C_REPLAY_VOTE       = 24,
        S_REPLAY_STATUS     = 25,
        S_RESTART_GAME      = 26,

        // === Disconnect ===
        S_PLAYER_DISCONNECTED = 27
    }

    /// <summary>
    /// TCPServer.cs - Quản lý TCP Listener chính, danh sách kết nối Client và điều phối broadcast tin nhắn.
    /// Chịu trách nhiệm mở cổng, chấp nhận socket kết nối mới và gửi gói tin tới các phòng chơi.
    /// </summary>
    public class TCPServer
    {
        // Quản lý socket lắng nghe TCP từ các client
        private TcpListener listener;

        // Danh sách các client đang kết nối tới server
        private readonly List<ClientHandler> clients = new List<ClientHandler>();

        // Quản lý các phòng chơi và trạng thái phòng
        private RoomManager roomManager;

        // Luồng chạy ngầm để liên tục chấp nhận kết nối TCP mới
        private Thread acceptThread;

        // Cờ kiểm soát vòng lặp lắng nghe của server
        private bool isRunning;

        // Khóa đồng bộ cho danh sách client khi thao tác đa luồng
        private readonly object clientsLock = new object();

        // Thuộc tính công khai truy cập RoomManager
        public RoomManager RoomManager => roomManager;

        // Thuộc tính công khai danh sách client
        public List<ClientHandler> Clients => clients;

        public TCPServer()
        {
            // TODO: Khởi tạo RoomManager
            roomManager = new RoomManager();
        }

        /// <summary>
        /// Khởi động server TCP trên cổng chỉ định và bắt đầu lắng nghe kết nối.
        /// </summary>
        /// <param name="port">Cổng mạng lắng nghe (ví dụ: 7777)</param>
        public void Start(int port)
        {
            // TODO: Khởi tạo TcpListener với IPAddress.Any và port chỉ định
            // TODO: Bắt đầu listener (listener.Start())
            // TODO: Đặt isRunning = true
            // TODO: Khởi tạo và khởi chạy acceptThread để lặp AcceptTcpClient
        }

        /// <summary>
        /// Vòng lặp chạy ngầm trên acceptThread để tiếp nhận từng kết nối TcpClient mới.
        /// </summary>
        private void AcceptLoop()
        {
            // TODO: Vòng lặp while(isRunning):
            //   - Gọi listener.AcceptTcpClient()
            //   - Tạo đối tượng ClientHandler mới cho client này
            //   - Thêm ClientHandler vào danh sách clients (sử dụng lock)
            //   - Gọi clientHandler.Start() để bắt đầu lắng nghe dữ liệu từ client
            //   - Xử lý SocketException khi server dừng
        }

        /// <summary>
        /// Dừng server TCP, ngắt toàn bộ kết nối và giải phóng tài nguyên.
        /// </summary>
        public void Stop()
        {
            // TODO: Đặt isRunning = false
            // TODO: Dừng listener (listener.Stop())
            // TODO: Lặp qua toàn bộ danh sách clients và gọi client.Disconnect()
            // TODO: Xóa sạch danh sách clients
        }

        /// <summary>
        /// Gửi một gói tin đến toàn bộ người chơi trong một phòng cụ thể.
        /// </summary>
        /// <param name="roomId">Mã định danh của phòng</param>
        /// <param name="type">Loại packet</param>
        /// <param name="json">Chuỗi dữ liệu JSON cần gửi</param>
        public void BroadcastToRoom(string roomId, PacketType type, string json)
        {
            // TODO: Lấy thông tin phòng từ roomManager.GetRoom(roomId)
            // TODO: Kiểm tra nếu phòng tồn tại:
            //   - Lặp qua danh sách người chơi trong phòng (room.Players)
            //   - Lấy clientHandler từ từng Player
            //   - Gọi clientHandler.SendPacket(type, json) nếu clientHandler đang kết nối
        }

        /// <summary>
        /// Xóa một client khỏi danh sách quản lý khi client ngắt kết nối.
        /// </summary>
        /// <param name="client">ClientHandler cần gỡ bỏ</param>
        public void RemoveClient(ClientHandler client)
        {
            // TODO: Lock clientsLock và gỡ client khỏi danh sách clients
        }
    }
}
