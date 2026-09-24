using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// Script UI chính cho Scene MainMenu.
/// Gộp tất cả logic: Nhập tên → Tạo/Tìm phòng → Phòng chờ → Bắt đầu game.
/// Gắn vào Canvas chính trong MainMenu scene.
/// 
/// Kết nối trực tiếp với GameLobbyServer qua WebSocket.
/// </summary>
public class MainMenuUI : MonoBehaviour
{
    // ═══════════════════════════════════════════
    //  PANELS
    // ═══════════════════════════════════════════
    [Header("═══ PANELS ═══")]
    [Tooltip("Panel 1: Màn hình chào (Nút Play, Character, Quit)")]
    public GameObject panelStart;

    [Tooltip("Panel 2: Sảnh (Nhập tên + Tạo/Join phòng)")]
    public GameObject panelLobby;

    [Tooltip("Panel 3: Phòng chờ (hiện người chơi + nút Start)")]
    public GameObject panelRoom;

    // ═══════════════════════════════════════════
    //  PANEL 1: START MENU
    // ═══════════════════════════════════════════
    [Header("═══ PANEL 1: START MENU ═══")]
    public Button btnPlay;
    public Button btnCharacter;
    public Button btnQuit;

    // ═══════════════════════════════════════════
    //  PANEL 2: LOBBY
    // ═══════════════════════════════════════════
    [Header("═══ PANEL 2: LOBBY ═══")]
    [Tooltip("Ô nhập tên người chơi")]
    public TMP_InputField inputPlayerName;

    [Tooltip("Ô nhập mã phòng (để Join)")]
    public TMP_InputField inputRoomCode;

    [Tooltip("Nút Tạo Phòng")]
    public Button btnCreateRoom;

    [Tooltip("Nút Vào Phòng")]
    public Button btnJoinRoom;

    [Tooltip("Nút Quay lại (về Start Menu)")]
    public Button btnBackToStart;

    [Tooltip("Text hiện trạng thái / lỗi")]
    public TMP_Text txtStatus;

    // ═══════════════════════════════════════════
    //  PANEL 3: PHÒNG CHỜ (ROOM)
    // ═══════════════════════════════════════════
    [Header("═══ PANEL 3: PHÒNG CHỜ ═══")]
    [Tooltip("Text hiện IP Server đang kết nối")]
    public TMP_Text txtServerIP;

    [Tooltip("Text hiện Mã phòng")]
    public TMP_Text txtRoomCode;

    [Tooltip("Text hiện số người: 2/4")]
    public TMP_Text txtPlayerCount;

    [Tooltip("Content của ScrollView chứa danh sách player")]
    public Transform playerListContent;

    [Tooltip("Prefab PlayerSlot (kéo từ Assets/Prefabs)")]
    public GameObject playerSlotPrefab;

    [Tooltip("Nút Bắt Đầu (chỉ Host thấy)")]
    public Button btnStart;

    [Tooltip("Nút Rời Phòng")]
    public Button btnLeave;

    // ═══════════════════════════════════════════
    //  PRIVATE STATE
    // ═══════════════════════════════════════════
    private bool isHost = false;
    private string currentRoomId = "";

    // ═══════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════
    private void Start()
    {
        // Hiện Panel Start, ẩn các Panel kia
        ShowPanel(panelStart);

        // Gắn nút Panel 1
        if (btnPlay) btnPlay.onClick.AddListener(OnClickPlay);
        if (btnQuit) btnQuit.onClick.AddListener(OnClickQuit);

        // Gắn nút Panel 2
        if (btnCreateRoom) btnCreateRoom.onClick.AddListener(OnClickCreateRoom);
        if (btnJoinRoom) btnJoinRoom.onClick.AddListener(OnClickJoinRoom);
        if (btnBackToStart) btnBackToStart.onClick.AddListener(OnClickBackToStart);

        // Gắn nút Panel 3
        if (btnStart) btnStart.onClick.AddListener(OnClickStartGame);
        if (btnLeave) btnLeave.onClick.AddListener(OnClickLeaveRoom);

        // Giới hạn input
        if (inputPlayerName) inputPlayerName.characterLimit = 16;
        if (inputRoomCode) inputRoomCode.characterLimit = 6;

        // Ẩn status
        if (txtStatus) txtStatus.text = "";

        // Lắng nghe tin nhắn từ Server
        if (NetworkClient.Instance != null)
        {
            NetworkClient.Instance.OnMessageReceived += OnServerMessage;
        }
    }

    private void OnDestroy()
    {
        if (NetworkClient.Instance != null)
        {
            NetworkClient.Instance.OnMessageReceived -= OnServerMessage;
        }
    }

    // ═══════════════════════════════════════════
    //  PANEL SWITCHING & SIMPLE BUTTONS
    // ═══════════════════════════════════════════
    private void ShowPanel(GameObject panel)
    {
        if (panelStart) panelStart.SetActive(false);
        if (panelLobby) panelLobby.SetActive(false);
        if (panelRoom) panelRoom.SetActive(false);
        if (panel) panel.SetActive(true);
    }

    public void OnClickPlay()
    {
        ShowPanel(panelLobby);
    }

    public void OnClickBackToStart()
    {
        ShowPanel(panelStart);
    }

    public void OnClickQuit()
    {
        Debug.Log("Thoát Game!");
        Application.Quit();
    }

    // ═══════════════════════════════════════════
    //  BUTTON HANDLERS
    // ═══════════════════════════════════════════

    [System.Serializable]
    private class CreateRoomRequest
    {
        public string Type;
        public string PlayerName;
    }

    [System.Serializable]
    private class JoinRoomRequest
    {
        public string Type;
        public string PlayerName;
        public string RoomId;
    }

    /// <summary>
    /// Bấm nút TẠO PHÒNG
    /// </summary>
    public void OnClickCreateRoom()
    {
        string playerName = inputPlayerName?.text?.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            ShowStatus("⚠️ Vui lòng nhập tên!", Color.red);
            return;
        }

        isHost = true;
        ShowStatus("Đang kết nối...", Color.yellow);
        SetButtonsInteractable(false);

        var req = new CreateRoomRequest { Type = "CREATE_ROOM", PlayerName = playerName };
        ConnectAndSend(JsonUtility.ToJson(req));
    }

    /// <summary>
    /// Bấm nút VÀO PHÒNG
    /// </summary>
    public void OnClickJoinRoom()
    {
        string playerName = inputPlayerName?.text?.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            ShowStatus("⚠️ Vui lòng nhập tên!", Color.red);
            return;
        }

        string roomCode = inputRoomCode?.text?.Trim().ToUpper();
        if (string.IsNullOrEmpty(roomCode))
        {
            ShowStatus("⚠️ Vui lòng nhập mã phòng!", Color.red);
            return;
        }

        isHost = false;
        ShowStatus("Đang kết nối...", Color.yellow);
        SetButtonsInteractable(false);

        var req = new JoinRoomRequest { Type = "JOIN_ROOM", PlayerName = playerName, RoomId = roomCode };
        ConnectAndSend(JsonUtility.ToJson(req));
    }

    /// <summary>
    /// Bấm nút BẮT ĐẦU (chỉ Host)
    /// </summary>
    public void OnClickStartGame()
    {
        Debug.Log("UI: Bắt đầu game!");
        SceneManager.LoadScene("GamePlay");
    }

    /// <summary>
    /// Bấm nút RỜI PHÒNG
    /// </summary>
    public void OnClickLeaveRoom()
    {
        if (NetworkClient.Instance != null && NetworkClient.Instance.IsConnected)
        {
            NetworkClient.Instance.SendMessage("{\"Type\":\"LEAVE_ROOM\"}");
        }

        NetworkClient.Instance?.Disconnect();

        currentRoomId = "";
        ShowPanel(panelLobby); // Quay về Lobby sau khi rời phòng
        SetButtonsInteractable(true);
        ShowStatus("Đã rời phòng.", Color.white);
    }

    // ═══════════════════════════════════════════
    //  NETWORK
    // ═══════════════════════════════════════════

    /// <summary>
    /// Kết nối tới WebSocket server và gửi 1 tin nhắn JSON
    /// </summary>
    private async void ConnectAndSend(string jsonPayload)
    {
        if (NetworkClient.Instance == null)
        {
            ShowStatus("❌ Lỗi: Chưa có NetworkClient!", Color.red);
            SetButtonsInteractable(true);
            return;
        }

        // Kết nối nếu chưa kết nối
        if (!NetworkClient.Instance.IsConnected)
        {
            NetworkClient.Instance.Connect(
                NetworkClient.Instance.ServerIP,
                NetworkClient.Instance.ServerPort
            );

            float timer = 0f;
            while (!NetworkClient.Instance.IsConnected && timer < 5f)
            {
                await System.Threading.Tasks.Task.Delay(100);
                timer += 0.1f;
            }

            if (!NetworkClient.Instance.IsConnected)
            {
                ShowStatus("❌ Không thể kết nối Server!", Color.red);
                SetButtonsInteractable(true);
                return;
            }
        }

        NetworkClient.Instance.SendMessage(jsonPayload);
        ShowStatus("✅ Đã gửi yêu cầu...", Color.green);
    }

    // ═══════════════════════════════════════════
    //  SERVER MESSAGE HANDLER (JsonUtility)
    // ═══════════════════════════════════════════

    [System.Serializable]
    private class BaseMessage
    {
        public string Type;
    }

    [System.Serializable]
    private class ErrorMessage
    {
        public string Type;
        public string Message;
    }

    [System.Serializable]
    private class PlayerState
    {
        public string Name;
        public bool IsHost;
        public string Status;
    }

    [System.Serializable]
    private class RoomStateMessage
    {
        public string Type;
        public string RoomId;
        public int MaxPlayers;
        public PlayerState[] Players;
    }

    /// <summary>
    /// Nhận tin nhắn JSON từ Server (đã dispatch về Main Thread)
    /// </summary>
    private void OnServerMessage(string json)
    {
        Debug.Log($"[MainMenuUI] Server gửi: {json}");

        try
        {
            BaseMessage baseMsg = JsonUtility.FromJson<BaseMessage>(json);

            switch (baseMsg.Type)
            {
                case "ROOM_STATE":
                    RoomStateMessage roomMsg = JsonUtility.FromJson<RoomStateMessage>(json);
                    HandleRoomState(roomMsg);
                    break;

                case "ERROR":
                    ErrorMessage errorMsg = JsonUtility.FromJson<ErrorMessage>(json);
                    ShowStatus($"❌ {errorMsg.Message}", Color.red);
                    SetButtonsInteractable(true);
                    break;

                default:
                    Debug.Log($"[MainMenuUI] Loại tin nhắn chưa xử lý: {baseMsg.Type}");
                    break;
            }
        }
        catch (System.Exception e)
        {
            Debug.LogError($"[MainMenuUI] Lỗi parse JSON: {e.Message}");
        }
    }

    /// <summary>
    /// Xử lý tin nhắn ROOM_STATE từ server
    /// </summary>
    private void HandleRoomState(RoomStateMessage roomMsg)
    {
        currentRoomId = roomMsg.RoomId;
        int maxPlayers = roomMsg.MaxPlayers;
        int playerCount = roomMsg.Players != null ? roomMsg.Players.Length : 0;

        // Chuyển sang Panel Phòng Chờ
        ShowPanel(panelRoom);

        // Hiện IP Server
        if (txtServerIP) txtServerIP.text = $"Server: {NetworkClient.Instance.ConnectedIP}";

        // Hiện mã phòng
        if (txtRoomCode) txtRoomCode.text = $"Mã phòng: {currentRoomId}";

        // Hiện số người
        if (txtPlayerCount) txtPlayerCount.text = $"{playerCount} / {maxPlayers}";

        // Hiện/ẩn nút Start (chỉ Host mới thấy)
        if (btnStart) btnStart.gameObject.SetActive(isHost);

        // Vẽ danh sách người chơi
        DrawPlayerList(roomMsg.Players);
    }

    /// <summary>
    /// Vẽ danh sách người chơi vào ScrollView
    /// </summary>
    private void DrawPlayerList(PlayerState[] playersArray)
    {
        // Xóa các slot cũ
        if (playerListContent != null)
        {
            foreach (Transform child in playerListContent)
            {
                Destroy(child.gameObject);
            }
        }

        if (playersArray == null) return;

        // Tạo slot mới cho từng người chơi
        for (int i = 0; i < playersArray.Length; i++)
        {
            PlayerState player = playersArray[i];
            
            if (playerSlotPrefab != null && playerListContent != null)
            {
                GameObject slotObj = Instantiate(playerSlotPrefab, playerListContent);
                
                // Tìm các component TMPro trong Prefab để gán dữ liệu
                PlayerSlotUI slotUI = slotObj.GetComponent<PlayerSlotUI>();
                if (slotUI != null)
                {
                    PlayerData pd = new PlayerData
                    {
                        playerName = player.Name,
                        isHost = player.IsHost,
                        playerId = i.ToString()
                    };
                    string myName = inputPlayerName?.text?.Trim();
                    bool isLocal = (player.Name == myName);
                    slotUI.Setup(pd, i, isLocal, isHost);
                }
            }
        }
    }

    // ═══════════════════════════════════════════
    //  HELPERS
    // ═══════════════════════════════════════════

    private void ShowStatus(string msg, Color color)
    {
        if (txtStatus)
        {
            txtStatus.text = msg;
            txtStatus.color = color;
        }
    }

    private void SetButtonsInteractable(bool interactable)
    {
        if (btnCreateRoom) btnCreateRoom.interactable = interactable;
        if (btnJoinRoom) btnJoinRoom.interactable = interactable;
    }
}
