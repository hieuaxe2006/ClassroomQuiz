using System;
using UnityEngine;

/// <summary>
/// Các trạng thái chính của vòng đời trò chơi.
/// </summary>
public enum GameState
{
    Menu,
    InLobby,
    InRoom,
    Playing,
    GameOver
}

/// <summary>
/// Quản lý dữ liệu người chơi, trạng thái trò chơi toàn cục và điều phối logic chung.
/// Được cấu hình dạng Singleton và không bị hủy khi đổi Scene (DontDestroyOnLoad).
/// </summary>
public class GameManager : MonoBehaviour
{
    public static GameManager Instance { get; private set; }

    // =========================================================
    // CÁC SỰ KIỆN (EVENTS) ĐỂ UI LẮNG NGHE TỪ SERVER
    // =========================================================
    public Action OnConnectSuccess;                           // Gọi khi kết nối OK
    public Action<RoomData> OnRoomCreated;                    // Gọi khi tạo phòng xong
    public Action<System.Collections.Generic.List<RoomData>> OnRoomListReceived; // Gọi khi nhận DS phòng
    public Action<RoomData> OnJoinRoomSuccess;                // Gọi khi join phòng OK
    public Action OnGameStarted;                              // Gọi khi game bắt đầu

    [Header("Dữ liệu Người chơi & Phòng")]
    [SerializeField] private PlayerData localPlayer;
    [SerializeField] private RoomData currentRoom;

    [Header("Trạng thái Game")]
    [SerializeField] private GameState currentState = GameState.Menu;
    [SerializeField] private int currentRound = 0;

    public PlayerData LocalPlayer
    {
        get => localPlayer;
        set => localPlayer = value;
    }

    public RoomData CurrentRoom
    {
        get => currentRoom;
        set => currentRoom = value;
    }

    public GameState CurrentState => currentState;
    public int CurrentRound => currentRound;

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
    /// Thay đổi trạng thái trò chơi hiện tại và thực hiện các hành động chuyển tiếp tương ứng.
    /// </summary>
    /// <param name="newState">Trạng thái mới cần chuyển sang</param>
    public void SetState(GameState newState)
    {
        Debug.Log($"[GameManager] State: {currentState} → {newState}");
        currentState = newState;
    }

    /// <summary>
    /// Thiết lập lại thông tin trò chơi, điểm số và các biến phòng chơi về giá trị mặc định ban đầu.
    /// </summary>
    public void ResetGame()
    {
        currentRound = 0;

        if (localPlayer != null)
        {
            localPlayer.currentScore = 0;
        }

        currentRoom = null;
        SetState(GameState.Menu);
    }
}
