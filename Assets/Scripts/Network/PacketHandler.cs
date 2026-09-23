using System;
using UnityEngine;

/// <summary>
/// Singleton xử lý phân loại các gói tin (Packet) nhận được từ server
/// và phát ra các sự kiện (Events/Actions) cho UI và các hệ thống khác đăng ký lắng nghe.
/// </summary>
public class PacketHandler : MonoBehaviour
{
    public static PacketHandler Instance { get; private set; }

    #region Sự kiện (Events) cho UI / Gameplay đăng ký
    // === Kết nối ===
    public event Action<string> OnConnectOK;

    // === Phòng (Room) ===
    public event Action<string> OnRoomCreated;
    public event Action<string> OnRoomList;
    public event Action<string> OnJoinOK;
    public event Action<string> OnJoinFail;
    public event Action<string> OnPlayerJoined;
    public event Action<string> OnPlayerLeft;

    // === Trò chơi (Game) ===
    public event Action<string> OnGameStart;
    public event Action<string> OnShowQuestion;
    public event Action<string> OnHandWinner;
    public event Action<string> OnAnswerResult;
    public event Action<string> OnTurnPassed;
    public event Action<string> OnRebuzz;
    public event Action<string> OnRoundEnd;
    public event Action<string> OnGameEnd;

    // === Replay / Chơi lại ===
    public event Action<string> OnReplayStatus;
    public event Action<string> OnRestartGame;

    // === Mất kết nối ===
    public event Action<string> OnPlayerDisconnected;
    #endregion

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

    /// <summary>
    /// Xử lý packet dựa trên PacketType và chuỗi JSON nhận được.
    /// </summary>
    /// <param name="type">Loại packet</param>
    /// <param name="json">Chuỗi dữ liệu JSON đi kèm</param>
    public void HandlePacket(PacketType type, string json)
    {
        switch (type)
        {
            // ==================== KẾT NỐI ====================
            case PacketType.C_CONNECT:
                // TODO: Gói tin gửi từ client, server không gửi gói này về client
                break;

            case PacketType.S_CONNECT_OK:
                // TODO: Xử lý dữ liệu xác nhận kết nối thành công từ server và kích hoạt sự kiện
                OnConnectOK?.Invoke(json);
                break;

            // ==================== PHÒNG CHƠI ====================
            case PacketType.C_CREATE_ROOM:
                // TODO: Gói tin gửi yêu cầu tạo phòng từ client
                break;

            case PacketType.S_ROOM_CREATED:
                // TODO: Xử lý phản hồi tạo phòng thành công (roomId, thông tin phòng)
                OnRoomCreated?.Invoke(json);
                break;

            case PacketType.C_REQUEST_ROOMS:
                // TODO: Gói tin yêu cầu danh sách phòng từ client
                break;

            case PacketType.S_ROOM_LIST:
                // TODO: Phân tích danh sách các phòng khả dụng và cập nhật UI Lobby
                OnRoomList?.Invoke(json);
                break;

            case PacketType.C_JOIN_ROOM:
                // TODO: Gói tin yêu cầu vào phòng từ client
                break;

            case PacketType.S_JOIN_OK:
                // TODO: Xử lý khi vào phòng thành công (dữ liệu phòng, người chơi)
                OnJoinOK?.Invoke(json);
                break;

            case PacketType.S_JOIN_FAIL:
                // TODO: Xử lý lỗi khi không thể vào phòng (phòng đầy, đã bắt đầu...)
                OnJoinFail?.Invoke(json);
                break;

            case PacketType.S_PLAYER_JOINED:
                // TODO: Cập nhật danh sách người chơi khi có người mới tham gia phòng
                OnPlayerJoined?.Invoke(json);
                break;

            case PacketType.S_PLAYER_LEFT:
                // TODO: Cập nhật danh sách người chơi khi có người rời khỏi phòng
                OnPlayerLeft?.Invoke(json);
                break;

            case PacketType.C_LEAVE_ROOM:
                // TODO: Gói tin yêu cầu rời phòng từ client
                break;

            // ==================== GAMEPLAY ====================
            case PacketType.C_START_GAME:
                // TODO: Gói tin bắt đầu game từ Host
                break;

            case PacketType.S_GAME_START:
                // TODO: Chuyển sang màn hình chơi game, đếm ngược bắt đầu ván đấu
                OnGameStart?.Invoke(json);
                break;

            case PacketType.S_SHOW_QUESTION:
                // TODO: Hiển thị câu hỏi, danh sách đáp án và bộ đếm giờ trả lời
                OnShowQuestion?.Invoke(json);
                break;

            case PacketType.C_RAISE_HAND:
                // TODO: Gói tin bấm chuông giành quyền trả lời từ client
                break;

            case PacketType.S_HAND_WINNER:
                // TODO: Thông báo người chơi giành được quyền trả lời đầu tiên
                OnHandWinner?.Invoke(json);
                break;

            case PacketType.C_SUBMIT_ANSWER:
                // TODO: Gói tin nộp đáp án từ người chơi được quyền trả lời
                break;

            case PacketType.S_ANSWER_RESULT:
                // TODO: Hiển thị kết quả đúng/sai và cập nhật điểm số người chơi
                OnAnswerResult?.Invoke(json);
                break;

            case PacketType.S_TURN_PASSED:
                // TODO: Xử lý khi người trả lời sai hoặc hết giờ, chuyển lượt cho người khác
                OnTurnPassed?.Invoke(json);
                break;

            case PacketType.S_REBUZZ:
                // TODO: Mở lại lượt bấm chuông cho các người chơi còn lại
                OnRebuzz?.Invoke(json);
                break;

            case PacketType.S_ROUND_END:
                // TODO: Kết thúc vòng câu hỏi hiện tại, hiển thị bảng điểm tạm thời
                OnRoundEnd?.Invoke(json);
                break;

            case PacketType.S_GAME_END:
                // TODO: Kết thúc toàn bộ trò chơi, chuyển sang màn hình tổng kết kết quả
                OnGameEnd?.Invoke(json);
                break;

            // ==================== REPLAY ====================
            case PacketType.C_REPLAY_VOTE:
                // TODO: Gói tin biểu quyết muốn chơi lại ván mới từ client
                break;

            case PacketType.S_REPLAY_STATUS:
                // TODO: Cập nhật số phiếu biểu quyết chơi lại của các người chơi
                OnReplayStatus?.Invoke(json);
                break;

            case PacketType.S_RESTART_GAME:
                // TODO: Reset toàn bộ bàn chơi và bắt đầu ván đấu mới
                OnRestartGame?.Invoke(json);
                break;

            // ==================== MẤT KẾT NỐI ====================
            case PacketType.S_PLAYER_DISCONNECTED:
                // TODO: Xử lý thông báo người chơi bị mất kết nối hoặc thoát đột ngột
                OnPlayerDisconnected?.Invoke(json);
                break;

            default:
                Debug.LogWarning($"[PacketHandler] Chưa có định nghĩa xử lý cho loại packet: {type}");
                break;
        }
    }
}
