using System;
using UnityEngine;

/// <summary>
/// Lớp tiện ích tĩnh (static helper) hỗ trợ đóng gói và gửi các yêu cầu từ Client lên Server.
/// Các phương thức tại đây gọi trực tiếp đến NetworkClient.Instance.SendPacket().
/// </summary>
public static class PacketSender
{
    /// <summary>
    /// Gửi thông tin định danh người chơi khi kết nối vào hệ thống.
    /// </summary>
    /// <param name="name">Tên hiển thị của người chơi</param>
    /// <param name="charId">ID nhân vật lựa chọn (0, 1, 2...)</param>
    public static void SendConnect(string name, int charId)
    {
        // TODO: Tạo JSON payload chứa name và charId (ví dụ: JsonUtility.ToJson(...))
        // TODO: Gọi NetworkClient.Instance.SendPacket(PacketType.C_CONNECT, jsonPayload);
    }

    /// <summary>
    /// Gửi yêu cầu tạo phòng chơi mới.
    /// </summary>
    public static void SendCreateRoom()
    {
        // TODO: Tạo JSON payload cấu hình phòng nếu cần
        // TODO: Gọi NetworkClient.Instance.SendPacket(PacketType.C_CREATE_ROOM, "{}");
    }

    /// <summary>
    /// Yêu cầu server gửi danh sách các phòng chơi hiện có.
    /// </summary>
    public static void SendRequestRooms()
    {
        // TODO: Gọi NetworkClient.Instance.SendPacket(PacketType.C_REQUEST_ROOMS, "{}");
    }

    /// <summary>
    /// Gửi yêu cầu tham gia vào một phòng chơi cụ thể.
    /// </summary>
    /// <param name="roomId">Mã ID của phòng cần tham gia</param>
    public static void SendJoinRoom(string roomId)
    {
        // TODO: Đóng gói roomId vào JSON payload
        // TODO: Gọi NetworkClient.Instance.SendPacket(PacketType.C_JOIN_ROOM, jsonPayload);
    }

    /// <summary>
    /// Gửi yêu cầu rời khỏi phòng chơi hiện tại.
    /// </summary>
    public static void SendLeaveRoom()
    {
        // TODO: Gọi NetworkClient.Instance.SendPacket(PacketType.C_LEAVE_ROOM, "{}");
    }

    /// <summary>
    /// Gửi tín hiệu bắt đầu trò chơi (chỉ dành cho Chủ phòng - Host).
    /// </summary>
    public static void SendStartGame()
    {
        // TODO: Gọi NetworkClient.Instance.SendPacket(PacketType.C_START_GAME, "{}");
    }

    /// <summary>
    /// Gửi tín hiệu bấm chuông giành quyền trả lời câu hỏi hiện tại.
    /// </summary>
    public static void SendRaiseHand()
    {
        // TODO: Gọi NetworkClient.Instance.SendPacket(PacketType.C_RAISE_HAND, "{}");
    }

    /// <summary>
    /// Gửi đáp án đã chọn sau khi giành được quyền trả lời.
    /// </summary>
    /// <param name="answerIndex">Vị trí đáp án được chọn (0 - 3)</param>
    public static void SendSubmitAnswer(int answerIndex)
    {
        // TODO: Đóng gói answerIndex vào chuỗi JSON payload
        // TODO: Gọi NetworkClient.Instance.SendPacket(PacketType.C_SUBMIT_ANSWER, jsonPayload);
    }

    /// <summary>
    /// Gửi bình chọn (vote) đồng ý chơi lại ván mới.
    /// </summary>
    public static void SendReplayVote()
    {
        // TODO: Đóng gói thông tin vote vào JSON payload nếu cần
        // TODO: Gọi NetworkClient.Instance.SendPacket(PacketType.C_REPLAY_VOTE, "{}");
    }
}
