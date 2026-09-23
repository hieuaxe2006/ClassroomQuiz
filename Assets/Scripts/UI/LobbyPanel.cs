using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClassroomQuiz.UI
{
    public class LobbyPanel : MonoBehaviour
    {
        public Button createRoomButton;
        public Button findRoomButton;
        public Button backButton;

        private void Start()
        {
            if (createRoomButton) createRoomButton.onClick.AddListener(OnCreateRoomClicked);
            if (findRoomButton) findRoomButton.onClick.AddListener(OnFindRoomClicked);
            if (backButton) backButton.onClick.AddListener(OnBackClicked);

            // Đăng ký nghe sự kiện từ Server
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRoomListReceived += GoToRoomList;
                GameManager.Instance.OnRoomCreated += GoToRoom;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRoomListReceived -= GoToRoomList;
                GameManager.Instance.OnRoomCreated -= GoToRoom;
            }
        }

        public void OnCreateRoomClicked()
        {
            Debug.Log("UI: Đang gửi lệnh TẠO PHÒNG...");
            createRoomButton.interactable = false;

            // ================== GIẢ LẬP SERVER ==================
            Invoke(nameof(MockCreateRoomResponse), 1f);
        }

        public void OnFindRoomClicked()
        {
            Debug.Log("UI: Đang gửi lệnh LẤY DANH SÁCH PHÒNG...");
            findRoomButton.interactable = false;

            // ================== GIẢ LẬP SERVER ==================
            Invoke(nameof(MockRoomListResponse), 1f);
        }

        // --- CÁC HÀM GIẢ LẬP SERVER TRẢ LỜI ---
        private void MockCreateRoomResponse()
        {
            RoomData fakeRoom = new RoomData { roomId = "R01", roomName = "Phòng của Test", hostId = "123", status = "WAITING" };
            GameManager.Instance.CurrentRoom = fakeRoom; // Lưu dữ liệu phòng vào GameManager
            GameManager.Instance.OnRoomCreated?.Invoke(fakeRoom); // Bật loa
        }

        private void MockRoomListResponse()
        {
            List<RoomData> fakeList = new List<RoomData>
            {
                new RoomData { roomId = "R01", roomName = "Phòng Vip", hostId = "A", players = new List<PlayerData>{ new PlayerData() } },
                new RoomData { roomId = "R02", roomName = "Chơi vui", hostId = "B", players = new List<PlayerData>{ new PlayerData(), new PlayerData() } }
            };
            GameManager.Instance.OnRoomListReceived?.Invoke(fakeList); // Bật loa
        }

        // --- HÀM CHUYỂN MÀN HÌNH (Chỉ chạy khi Server gọi loa) ---
        private void GoToRoomList(List<RoomData> rooms)
        {
            findRoomButton.interactable = true;
            MenuUIManager.Instance.ShowRoomList();
        }

        private void GoToRoom(RoomData room)
        {
            createRoomButton.interactable = true;
            MenuUIManager.Instance.ShowRoom();
        }

        public void OnBackClicked()
        {
            MenuUIManager.Instance.ShowMainMenu();
        }
    }
}
