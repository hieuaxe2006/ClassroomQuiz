// PacketType.cs — CẢ 2 NGƯỜI DÙNG CHUNG (Server + Client)
// Không được tự ý sửa mà không báo người kia!

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
