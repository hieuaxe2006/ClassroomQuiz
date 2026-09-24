using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

/// <summary>
/// UI Controller cho màn hình Main Lobby.
/// Xử lý 2 chức năng chính: Tạo Phòng (Host) và Vào Phòng (Join).
/// Gắn script này vào Panel "MainLobby" trong Canvas của scene MainLobby.
/// </summary>
public class MainLobbyUI : MonoBehaviour
{
    [Header("═══ INPUT FIELDS ═══")]
    [Tooltip("Ô nhập tên người chơi")]
    [SerializeField] private TMP_InputField inputPlayerName;

    [Tooltip("Ô nhập mã phòng (dùng khi Join)")]
    [SerializeField] private TMP_InputField inputRoomCode;

    [Header("═══ BUTTONS ═══")]
    [Tooltip("Nút Tạo Phòng")]
    [SerializeField] private Button btnCreateRoom;

    [Tooltip("Nút Vào Phòng")]
    [SerializeField] private Button btnJoinRoom;

    [Header("═══ FEEDBACK UI ═══")]
    [Tooltip("Panel hiển thị thông báo lỗi/trạng thái (có thể ẩn mặc định)")]
    [SerializeField] private GameObject panelMessage;

    [Tooltip("Text hiển thị nội dung thông báo")]
    [SerializeField] private TMP_Text txtMessage;

    [Tooltip("Panel Loading hiển thị khi đang kết nối")]
    [SerializeField] private GameObject panelLoading;

    [Tooltip("Text trạng thái kết nối trên panel loading")]
    [SerializeField] private TMP_Text txtLoadingStatus;

    [Header("═══ CẤU HÌNH ═══")]
    [Tooltip("Tên scene LobbyHost để chuyển sang sau khi tạo/vào phòng")]
    [SerializeField] private string lobbyHostSceneName = "LobbyHost";

    // Theo dõi trạng thái đang xử lý
    private bool isProcessing = false;

    // ═══════════════════════════════════════════
    //  UNITY LIFECYCLE
    // ═══════════════════════════════════════════

    private void Start()
    {
        // Gắn sự kiện click cho các nút
        btnCreateRoom?.onClick.AddListener(OnClickCreateRoom);
        btnJoinRoom?.onClick.AddListener(OnClickJoinRoom);

        // Ẩn panel thông báo & loading ban đầu
        if (panelMessage != null) panelMessage.SetActive(false);
        if (panelLoading != null) panelLoading.SetActive(false);

        // Giới hạn ô nhập mã phòng viết hoa, 6 ký tự
        if (inputRoomCode != null)
        {
            inputRoomCode.characterLimit = 6;
            inputRoomCode.onValueChanged.AddListener(OnRoomCodeChanged);
        }

        // Giới hạn tên 16 ký tự
        if (inputPlayerName != null)
        {
            inputPlayerName.characterLimit = 16;
        }

        // Đăng ký sự kiện từ PacketHandler
        RegisterPacketEvents();
    }

    private void OnDestroy()
    {
        // Hủy đăng ký sự kiện để tránh memory leak
        UnregisterPacketEvents();
    }

    // ═══════════════════════════════════════════
    //  PACKET EVENTS REGISTRATION
    // ═══════════════════════════════════════════

    private void RegisterPacketEvents()
    {
        if (PacketHandler.Instance == null) return;

        PacketHandler.Instance.OnConnectOK += HandleConnectOK;
        PacketHandler.Instance.OnRoomCreated += HandleRoomCreated;
        PacketHandler.Instance.OnJoinOK += HandleJoinOK;
        PacketHandler.Instance.OnJoinFail += HandleJoinFail;
    }

    private void UnregisterPacketEvents()
    {
        if (PacketHandler.Instance == null) return;

        PacketHandler.Instance.OnConnectOK -= HandleConnectOK;
        PacketHandler.Instance.OnRoomCreated -= HandleRoomCreated;
        PacketHandler.Instance.OnJoinOK -= HandleJoinOK;
        PacketHandler.Instance.OnJoinFail -= HandleJoinFail;
    }

    // ═══════════════════════════════════════════
    //  BUTTON HANDLERS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Xử lý khi nhấn nút "Tạo Phòng".
    /// Kết nối server → Gửi C_CONNECT → Gửi C_CREATE_ROOM.
    /// </summary>
    public void OnClickCreateRoom()
    {
        if (isProcessing) return;

        // Validate tên
        string playerName = inputPlayerName?.text?.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            ShowMessage("Vui lòng nhập tên trước khi tạo phòng!", Color.red);
            return;
        }

        isProcessing = true;
        SetButtonsInteractable(false);
        ShowLoading("Đang kết nối server...");

        // Lưu thông tin player vào GameManager
        SaveLocalPlayerData(playerName, isHost: true);

        // Kết nối server
        ConnectToServer();

        // Sau khi kết nối OK → gửi C_CONNECT, rồi C_CREATE_ROOM
        // (xử lý trong HandleConnectOK)
    }

    /// <summary>
    /// Xử lý khi nhấn nút "Vào Phòng".
    /// Kết nối server → Gửi C_CONNECT → Gửi C_JOIN_ROOM với roomId.
    /// </summary>
    public void OnClickJoinRoom()
    {
        if (isProcessing) return;

        // Validate tên
        string playerName = inputPlayerName?.text?.Trim();
        if (string.IsNullOrEmpty(playerName))
        {
            ShowMessage("Vui lòng nhập tên!", Color.red);
            return;
        }

        // Validate mã phòng
        string roomCode = inputRoomCode?.text?.Trim().ToUpper();
        if (string.IsNullOrEmpty(roomCode))
        {
            ShowMessage("Vui lòng nhập mã phòng!", Color.red);
            return;
        }

        isProcessing = true;
        SetButtonsInteractable(false);
        ShowLoading("Đang vào phòng...");

        // Lưu thông tin player vào GameManager
        SaveLocalPlayerData(playerName, isHost: false);

        // Kết nối server
        ConnectToServer();

        // Sau khi kết nối OK → gửi C_CONNECT, rồi C_JOIN_ROOM
        // (xử lý trong HandleConnectOK)
    }

    // ═══════════════════════════════════════════
    //  PACKET RESPONSE HANDLERS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Server xác nhận kết nối thành công.
    /// Tiếp tục gửi tạo phòng hoặc vào phòng tùy theo hành động ban đầu.
    /// </summary>
    private void HandleConnectOK(string json)
    {
        Debug.Log($"[MainLobbyUI] Kết nối OK: {json}");

        if (GameManager.Instance.LocalPlayer.isHost)
        {
            // Host → Gửi yêu cầu tạo phòng
            ShowLoading("Đang tạo phòng...");
            PacketSender.SendCreateRoom();
        }
        else
        {
            // Player → Gửi yêu cầu vào phòng
            string roomCode = inputRoomCode?.text?.Trim().ToUpper();
            ShowLoading($"Đang vào phòng {roomCode}...");
            PacketSender.SendJoinRoom(roomCode);
        }
    }

    /// <summary>
    /// Server xác nhận tạo phòng thành công. Chuyển sang scene LobbyHost.
    /// JSON chứa thông tin RoomData (roomId, hostId, players...).
    /// </summary>
    private void HandleRoomCreated(string json)
    {
        Debug.Log($"[MainLobbyUI] Phòng đã được tạo: {json}");

        // Parse room data
        RoomData roomData = JsonUtility.FromJson<RoomData>(json);
        if (roomData != null)
        {
            GameManager.Instance.CurrentRoom = roomData;
        }

        HideLoading();
        GameManager.Instance.SetState(GameState.InRoom);

        // Chuyển sang scene phòng chờ
        SceneManager.LoadScene(lobbyHostSceneName);
    }

    /// <summary>
    /// Server xác nhận vào phòng thành công. Chuyển sang scene LobbyHost.
    /// JSON chứa thông tin RoomData đầy đủ.
    /// </summary>
    private void HandleJoinOK(string json)
    {
        Debug.Log($"[MainLobbyUI] Vào phòng OK: {json}");

        // Parse room data
        RoomData roomData = JsonUtility.FromJson<RoomData>(json);
        if (roomData != null)
        {
            GameManager.Instance.CurrentRoom = roomData;
        }

        HideLoading();
        GameManager.Instance.SetState(GameState.InRoom);

        // Chuyển sang scene phòng chờ
        SceneManager.LoadScene(lobbyHostSceneName);
    }

    /// <summary>
    /// Server từ chối vào phòng (phòng đầy, không tồn tại, đã bắt đầu...).
    /// </summary>
    private void HandleJoinFail(string json)
    {
        Debug.LogWarning($"[MainLobbyUI] Vào phòng thất bại: {json}");

        HideLoading();
        isProcessing = false;
        SetButtonsInteractable(true);

        // Hiển thị lý do lỗi
        ShowMessage($"Không thể vào phòng: {json}", Color.red);
    }

    // ═══════════════════════════════════════════
    //  HELPER METHODS
    // ═══════════════════════════════════════════

    /// <summary>
    /// Lưu thông tin người chơi local vào GameManager.
    /// </summary>
    private void SaveLocalPlayerData(string playerName, bool isHost)
    {
        if (GameManager.Instance == null) return;

        var player = new PlayerData
        {
            playerName = playerName,
            playerId = System.Guid.NewGuid().ToString()[..8],
            isHost = isHost,
            currentScore = 0,
            totalWins = 0
        };

        GameManager.Instance.LocalPlayer = player;
    }

    /// <summary>
    /// Kết nối đến TCP Server thông qua NetworkClient.
    /// </summary>
    private void ConnectToServer()
    {
        if (NetworkClient.Instance == null)
        {
            ShowMessage("Lỗi: NetworkClient chưa được khởi tạo!", Color.red);
            isProcessing = false;
            SetButtonsInteractable(true);
            return;
        }

        if (!NetworkClient.Instance.IsConnected)
        {
            NetworkClient.Instance.Connect(
                NetworkClient.Instance.ServerIP,
                NetworkClient.Instance.ServerPort
            );
        }

        // Gửi gói tin kết nối với tên và character mặc định
        string playerName = GameManager.Instance.LocalPlayer.playerName;
        PacketSender.SendConnect(playerName, 0);
    }

    /// <summary>
    /// Chuyển mã phòng thành chữ hoa khi nhập.
    /// </summary>
    private void OnRoomCodeChanged(string value)
    {
        if (inputRoomCode != null)
        {
            inputRoomCode.text = value.ToUpper();
        }
    }

    /// <summary>
    /// Bật/tắt tương tác của các nút.
    /// </summary>
    private void SetButtonsInteractable(bool interactable)
    {
        if (btnCreateRoom != null) btnCreateRoom.interactable = interactable;
        if (btnJoinRoom != null) btnJoinRoom.interactable = interactable;
    }

    /// <summary>
    /// Hiển thị thông báo trên UI.
    /// </summary>
    private void ShowMessage(string message, Color color)
    {
        if (panelMessage != null) panelMessage.SetActive(true);
        if (txtMessage != null)
        {
            txtMessage.text = message;
            txtMessage.color = color;
        }

        // Tự ẩn sau 3 giây
        CancelInvoke(nameof(HideMessage));
        Invoke(nameof(HideMessage), 3f);
    }

    private void HideMessage()
    {
        if (panelMessage != null) panelMessage.SetActive(false);
    }

    /// <summary>
    /// Hiển thị panel loading.
    /// </summary>
    private void ShowLoading(string status)
    {
        if (panelLoading != null) panelLoading.SetActive(true);
        if (txtLoadingStatus != null) txtLoadingStatus.text = status;
    }

    private void HideLoading()
    {
        if (panelLoading != null) panelLoading.SetActive(false);
        isProcessing = false;
        SetButtonsInteractable(true);
    }
}
