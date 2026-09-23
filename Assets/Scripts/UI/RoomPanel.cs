using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using TMPro;

namespace ClassroomQuiz.UI
{
    /// <summary>
    /// Gắn vào Panel Room (Phòng Chờ).
    /// Hiển thị danh sách người chơi đang trong phòng + nút Leave/Start.
    /// </summary>
    public class RoomPanel : MonoBehaviour
    {
        [Header("Player Slots (Kéo 5 cái vào đây)")]
        public GameObject[] playerSlots = new GameObject[5]; // 5 ô chứa người chơi

        [Header("Bên trong mỗi Slot cần có")]
        // Mỗi slot chứa: 1 Image avatar + 1 TMP_Text tên + 1 Image crown (host)
        // Bạn sẽ kéo từng thành phần con vào các mảng dưới đây
        public TMP_Text[] playerNameTexts = new TMP_Text[5];
        public GameObject[] hostCrowns = new GameObject[5]; // Icon vương miện, mặc định ẩn

        [Header("Buttons")]
        public Button leaveButton;
        public Button startButton;

        [Header("Room Info")]
        public TMP_Text roomNameText; // Hiện tên phòng ở đầu Panel

        private void Start()
        {
            if (leaveButton) leaveButton.onClick.AddListener(OnLeaveClicked);
            if (startButton) startButton.onClick.AddListener(OnStartClicked);

            // Đăng ký sự kiện
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRoomCreated += OnRoomEntered;
                GameManager.Instance.OnJoinRoomSuccess += OnRoomEntered;
                GameManager.Instance.OnGameStarted += OnGameStart;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRoomCreated -= OnRoomEntered;
                GameManager.Instance.OnJoinRoomSuccess -= OnRoomEntered;
                GameManager.Instance.OnGameStarted -= OnGameStart;
            }
        }

        // --- GỌI KHI VÀO PHÒNG (Tạo hoặc Join) ---
        private void OnRoomEntered(RoomData room)
        {
            Debug.Log($"UI: Đã vào phòng {room.roomName}!");

            // Hiện tên phòng
            if (roomNameText) roomNameText.text = room.roomName;

            // Vẽ lại danh sách người chơi
            DrawPlayers(room.players);

            // Chỉ Host mới thấy nút Bắt Đầu
            bool isHost = (room.hostId == GameManager.Instance.LocalPlayer?.playerId);
            if (startButton) startButton.gameObject.SetActive(isHost);
        }

        // --- VẼ 5 Ô NGƯỜI CHƠI ---
        private void DrawPlayers(List<PlayerData> players)
        {
            // Tắt hết 5 slot trước
            for (int i = 0; i < playerSlots.Length; i++)
            {
                if (playerSlots[i]) playerSlots[i].SetActive(false);
                if (hostCrowns[i]) hostCrowns[i].SetActive(false);
            }

            if (players == null) return;

            // Bật lại các slot có người
            for (int i = 0; i < players.Count && i < playerSlots.Length; i++)
            {
                playerSlots[i].SetActive(true);
                playerNameTexts[i].text = players[i].playerName;

                // Nếu là Host thì hiện vương miện
                if (hostCrowns[i]) hostCrowns[i].SetActive(players[i].isHost);
            }
        }

        // --- NÚT RỜI PHÒNG ---
        public void OnLeaveClicked()
        {
            Debug.Log("UI: Rời phòng!");
            // Người A sẽ gọi: PacketSender.SendLeaveRoom();
            MenuUIManager.Instance.ShowLobby();
        }

        // --- NÚT BẮT ĐẦU (CHỈ HOST) ---
        public void OnStartClicked()
        {
            Debug.Log("UI: Đang gửi lệnh BẮT ĐẦU GAME...");
            startButton.interactable = false;

            // ================== GIẢ LẬP SERVER ==================
            Invoke(nameof(MockGameStartResponse), 1f);
        }

        private void MockGameStartResponse()
        {
            GameManager.Instance.OnGameStarted?.Invoke();
        }

        // --- CHUYỂN SCENE KHI SERVER BÁO BẮT ĐẦU ---
        private void OnGameStart()
        {
            Debug.Log("UI: Server báo bắt đầu! Chuyển sang GameScene...");
            SceneManager.LoadScene("GamePlay");
        }
    }
}
