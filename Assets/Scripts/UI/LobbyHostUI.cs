using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// UI Controller cho màn hình phòng chờ (LobbyHost).
/// Hiển thị mã phòng, danh sách người chơi, nút Start/Leave.
/// Gắn script này vào Canvas hoặc Panel chính trong scene LobbyHost.
/// </summary>
public class LobbyHostUI : MonoBehaviour
{
    [Header("═══ ROOM INFO ═══")]
    [Tooltip("Text hiển thị mã phòng (Room Code)")]
    [SerializeField] private TMP_Text txtRoomCode;

    [Tooltip("Nút Copy mã phòng")]
    [SerializeField] private Button btnCopyCode;

    [Tooltip("Text trạng thái copy (VD: 'Đã copy!')")]
    [SerializeField] private TMP_Text txtCopyStatus;

    [Tooltip("Text hiển thị số người chơi (VD: '2 / 5')")]
    [SerializeField] private TMP_Text txtPlayerCount;

    [Tooltip("Text hiển thị trạng thái phòng (WAITING, PLAYING...)")]
    [SerializeField] private TMP_Text txtRoomStatus;

    [Header("═══ PLAYER LIST ═══")]
    [Tooltip("Transform cha chứa các PlayerSlot (Content của ScrollView)")]
    [SerializeField] private Transform playerListParent;

    [Tooltip("Prefab cho mỗi slot player trong danh sách")]
    [SerializeField] private GameObject playerSlotPrefab;

    [Header("═══ BUTTONS ═══")]
    [Tooltip("Nút Bắt đầu trò chơi (chỉ Host thấy)")]
    [SerializeField] private Button btnStartGame;

    [Tooltip("Text trên nút Start (để thay đổi nội dung)")]
    [SerializeField] private TMP_Text txtStartButton;

    [Tooltip("Nút Rời phòng")]
    [SerializeField] private Button btnLeaveRoom;

    [Header("═══ PANELS ═══")]
    [Tooltip("Panel thông báo chung")]
    [SerializeField] private GameObject panelNotification;

    [Tooltip("Text trong panel thông báo")]
    [SerializeField] private TMP_Text txtNotification;

    [Header("═══ CẤU HÌNH ═══")]
    [Tooltip("Scene lobby để quay về khi rời phòng")]
    [SerializeField] private string mainLobbySceneName = "MainLobby";

    [Tooltip("Scene gameplay để chuyển khi bắt đầu game")]
    [SerializeField] private string gameplaySceneName = "GamePlay";

    [Tooltip("Số player tối thiểu để bắt đầu game")]
    [SerializeField] private int minPlayersToStart = 2;

    // Danh sách các slot UI đang hiển thị
    private readonly List<PlayerSlotUI> activeSlots = new List<PlayerSlotUI>();

    // ═══════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════

    private void Start()
    {
        // Gắn sự kiện cho các nút
        btnStartGame?.onClick.AddListener(OnClickStartGame);
        btnLeaveRoom?.onClick.AddListener(OnClickLeaveRoom);
        btnCopyCode?.onClick.AddListener(OnClickCopyCode);

        // Ẩn notification
        if (panelNotification != null) panelNotification.SetActive(false);

        // Đăng ký sự kiện mạng
        RegisterPacketEvents();

        // Hiển thị thông tin phòng hiện tại
        RefreshRoomUI();
    }

    private void OnDestroy()
    {
        UnregisterPacketEvents();
    }

    // ═══════════════════════════════════════════
    //  PACKET EVENTS
    // ═══════════════════════════════════════════

    private void RegisterPacketEvents()
    {
        if (PacketHandler.Instance == null) return;

        PacketHandler.Instance.OnPlayerJoined += HandlePlayerJoined;
        PacketHandler.Instance.OnPlayerLeft += HandlePlayerLeft;
        PacketHandler.Instance.OnGameStart += HandleGameStart;
        PacketHandler.Instance.OnPlayerDisconnected += HandlePlayerDisconnected;
    }

    private void UnregisterPacketEvents()
    {
        if (PacketHandler.Instance == null) return;

        PacketHandler.Instance.OnPlayerJoined -= HandlePlayerJoined;
        PacketHandler.Instance.OnPlayerLeft -= HandlePlayerLeft;
        PacketHandler.Instance.OnGameStart -= HandleGameStart;
        PacketHandler.Instance.OnPlayerDisconnected -= HandlePlayerDisconnected;
    }

    // ═══════════════════════════════════════════
    //  PACKET HANDLERS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Có người chơi mới tham gia phòng.
    /// Server gửi S_PLAYER_JOINED kèm RoomData cập nhật.
    /// </summary>
    private void HandlePlayerJoined(string json)
    {
        Debug.Log($"[LobbyHostUI] Player joined: {json}");

        // Cập nhật room data từ server
        RoomData updatedRoom = JsonUtility.FromJson<RoomData>(json);
        if (updatedRoom != null)
        {
            GameManager.Instance.CurrentRoom = updatedRoom;
        }

        ShowNotification($"🎮 Có người chơi mới tham gia!", Color.green);
        RefreshRoomUI();
    }

    /// <summary>
    /// Có người chơi rời khỏi phòng.
    /// </summary>
    private void HandlePlayerLeft(string json)
    {
        Debug.Log($"[LobbyHostUI] Player left: {json}");

        RoomData updatedRoom = JsonUtility.FromJson<RoomData>(json);
        if (updatedRoom != null)
        {
            GameManager.Instance.CurrentRoom = updatedRoom;
        }

        ShowNotification("👋 Có người chơi rời phòng.", new Color(1f, 0.84f, 0f));
        RefreshRoomUI();
    }

    /// <summary>
    /// Server thông báo game bắt đầu → Chuyển scene Gameplay.
    /// </summary>
    private void HandleGameStart(string json)
    {
        Debug.Log($"[LobbyHostUI] Game started: {json}");

        GameManager.Instance.SetState(GameState.Playing);
        SceneManager.LoadScene(gameplaySceneName);
    }

    /// <summary>
    /// Một người chơi bị mất kết nối đột ngột.
    /// </summary>
    private void HandlePlayerDisconnected(string json)
    {
        Debug.LogWarning($"[LobbyHostUI] Player disconnected: {json}");

        RoomData updatedRoom = JsonUtility.FromJson<RoomData>(json);
        if (updatedRoom != null)
        {
            GameManager.Instance.CurrentRoom = updatedRoom;
        }

        ShowNotification("⚠️ Một người chơi bị mất kết nối!", Color.red);
        RefreshRoomUI();
    }

    // ═══════════════════════════════════════════
    //  UI REFRESH
    // ═══════════════════════════════════════════

    /// <summary>
    /// Làm mới toàn bộ giao diện phòng chờ dựa trên GameManager.CurrentRoom.
    /// </summary>
    public void RefreshRoomUI()
    {
        RoomData room = GameManager.Instance?.CurrentRoom;
        if (room == null)
        {
            Debug.LogWarning("[LobbyHostUI] CurrentRoom is null!");
            return;
        }

        PlayerData localPlayer = GameManager.Instance.LocalPlayer;
        bool isHost = localPlayer != null && localPlayer.isHost;

        // === Mã phòng ===
        if (txtRoomCode != null)
        {
            txtRoomCode.text = room.roomId ?? "------";
        }

        // === Số người chơi ===
        int currentCount = room.players?.Count ?? 0;
        if (txtPlayerCount != null)
        {
            txtPlayerCount.text = $"{currentCount} / {room.maxPlayers}";
        }

        // === Trạng thái phòng ===
        if (txtRoomStatus != null)
        {
            txtRoomStatus.text = room.status ?? "WAITING";
        }

        // === Nút Start (chỉ Host + đủ người) ===
        if (btnStartGame != null)
        {
            bool canStart = isHost && currentCount >= minPlayersToStart;
            btnStartGame.gameObject.SetActive(isHost);
            btnStartGame.interactable = canStart;

            if (txtStartButton != null)
            {
                if (!isHost)
                    txtStartButton.text = "Chờ Host bắt đầu...";
                else if (currentCount < minPlayersToStart)
                    txtStartButton.text = $"Cần ít nhất {minPlayersToStart} người";
                else
                    txtStartButton.text = "🚀 Bắt Đầu Trò Chơi";
            }
        }

        // === Render danh sách Player ===
        RenderPlayerList(room, localPlayer, isHost);
    }

    /// <summary>
    /// Tạo/cập nhật danh sách player slot UI.
    /// </summary>
    private void RenderPlayerList(RoomData room, PlayerData localPlayer, bool isHost)
    {
        if (playerListParent == null || playerSlotPrefab == null) return;

        // Xóa các slot cũ
        ClearPlayerSlots();

        List<PlayerData> players = room.players ?? new List<PlayerData>();

        // Tạo slot cho từng player đang có trong phòng
        for (int i = 0; i < players.Count; i++)
        {
            GameObject slotObj = Instantiate(playerSlotPrefab, playerListParent);
            PlayerSlotUI slotUI = slotObj.GetComponent<PlayerSlotUI>();

            if (slotUI != null)
            {
                bool isLocal = localPlayer != null &&
                               players[i].playerId == localPlayer.playerId;

                slotUI.Setup(players[i], i, isLocal, isHost);
                activeSlots.Add(slotUI);
            }
        }

        // Tạo empty slot cho các vị trí trống
        for (int i = players.Count; i < room.maxPlayers; i++)
        {
            GameObject slotObj = Instantiate(playerSlotPrefab, playerListParent);
            PlayerSlotUI slotUI = slotObj.GetComponent<PlayerSlotUI>();

            if (slotUI != null)
            {
                slotUI.SetupEmpty(i);
                activeSlots.Add(slotUI);
            }
        }
    }

    /// <summary>
    /// Xóa toàn bộ player slot đang hiển thị.
    /// </summary>
    private void ClearPlayerSlots()
    {
        foreach (var slot in activeSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }
        activeSlots.Clear();
    }

    // ═══════════════════════════════════════════
    //  BUTTON HANDLERS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Host nhấn nút "Bắt đầu trò chơi".
    /// </summary>
    public void OnClickStartGame()
    {
        RoomData room = GameManager.Instance?.CurrentRoom;
        int currentCount = room?.players?.Count ?? 0;

        if (currentCount < minPlayersToStart)
        {
            ShowNotification($"Cần ít nhất {minPlayersToStart} người chơi!", Color.red);
            return;
        }

        Debug.Log("[LobbyHostUI] Host bấm Start Game");
        btnStartGame.interactable = false;

        if (txtStartButton != null)
            txtStartButton.text = "Đang bắt đầu...";

        PacketSender.SendStartGame();
    }

    /// <summary>
    /// Nhấn nút "Rời phòng".
    /// </summary>
    public void OnClickLeaveRoom()
    {
        Debug.Log("[LobbyHostUI] Rời phòng");

        // Gửi packet rời phòng
        PacketSender.SendLeaveRoom();

        // Reset game state
        GameManager.Instance.CurrentRoom = null;
        GameManager.Instance.SetState(GameState.InLobby);

        // Ngắt kết nối
        NetworkClient.Instance?.Disconnect();

        // Quay về lobby
        SceneManager.LoadScene(mainLobbySceneName);
    }

    /// <summary>
    /// Copy mã phòng vào clipboard.
    /// </summary>
    public void OnClickCopyCode()
    {
        string code = GameManager.Instance?.CurrentRoom?.roomId;
        if (!string.IsNullOrEmpty(code))
        {
            GUIUtility.systemCopyBuffer = code;
            Debug.Log($"[LobbyHostUI] Đã copy mã phòng: {code}");

            if (txtCopyStatus != null)
            {
                txtCopyStatus.text = "✅ Đã copy!";
                CancelInvoke(nameof(ResetCopyStatus));
                Invoke(nameof(ResetCopyStatus), 2f);
            }
        }
    }

    private void ResetCopyStatus()
    {
        if (txtCopyStatus != null)
            txtCopyStatus.text = "";
    }

    // ═══════════════════════════════════════════
    //  NOTIFICATION
    // ═══════════════════════════════════════════

    /// <summary>
    /// Hiển thị thông báo ngắn trên UI.
    /// </summary>
    private void ShowNotification(string message, Color color)
    {
        if (panelNotification != null) panelNotification.SetActive(true);
        if (txtNotification != null)
        {
            txtNotification.text = message;
            txtNotification.color = color;
        }

        CancelInvoke(nameof(HideNotification));
        Invoke(nameof(HideNotification), 3f);
    }

    private void HideNotification()
    {
        if (panelNotification != null) panelNotification.SetActive(false);
    }
}
