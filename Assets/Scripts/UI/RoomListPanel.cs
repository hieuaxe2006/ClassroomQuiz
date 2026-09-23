using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace ClassroomQuiz.UI
{
    /// <summary>
    /// Gắn vào Panel RoomList
    /// </summary>
    public class RoomListPanel : MonoBehaviour
    {
        public GameObject roomListItemPrefab; // Kéo Prefab RoomListItem vào đây
        public Transform contentContainer;    // Kéo cục Content của ScrollView vào đây
        public Button refreshButton;
        public Button backButton;

        private void Start()
        {
            if (backButton) backButton.onClick.AddListener(OnBackClicked);
            if (refreshButton) refreshButton.onClick.AddListener(OnRefreshClicked);

            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRoomListReceived += DrawRoomList;
                GameManager.Instance.OnJoinRoomSuccess += GoToRoom;
            }
        }

        private void OnDestroy()
        {
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnRoomListReceived -= DrawRoomList;
                GameManager.Instance.OnJoinRoomSuccess -= GoToRoom;
            }
        }

        private void DrawRoomList(List<RoomData> rooms)
        {
            // Xóa sạch phòng cũ
            foreach (Transform child in contentContainer)
            {
                Destroy(child.gameObject);
            }

            // Tạo phòng mới từ dữ liệu Server
            foreach (RoomData room in rooms)
            {
                GameObject newItem = Instantiate(roomListItemPrefab, contentContainer);
                RoomListItem script = newItem.GetComponent<RoomListItem>();
                script.Setup(room);
            }
        }

        public void OnRefreshClicked()
        {
            Debug.Log("UI: Đang làm mới danh sách phòng...");
            // Người A sẽ gọi: PacketSender.SendRequestRooms();

            // ================== GIẢ LẬP SERVER ==================
            List<RoomData> fakeList = new List<RoomData>
            {
                new RoomData { roomId = "R01", roomName = "Phòng Vip",  players = new List<PlayerData>{ new PlayerData() } },
                new RoomData { roomId = "R02", roomName = "Chơi vui",   players = new List<PlayerData>{ new PlayerData(), new PlayerData() } },
                new RoomData { roomId = "R03", roomName = "Phòng mới!", players = new List<PlayerData>() }
            };
            GameManager.Instance.OnRoomListReceived?.Invoke(fakeList);
        }

        private void GoToRoom(RoomData room)
        {
            MenuUIManager.Instance.ShowRoom();
        }

        public void OnBackClicked()
        {
            MenuUIManager.Instance.ShowLobby();
        }
    }
}
