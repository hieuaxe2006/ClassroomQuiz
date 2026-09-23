using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace ClassroomQuiz.UI
{
    /// <summary>
    /// Gắn vào Prefab RoomListItem (Một dòng hiển thị phòng trong ScrollView)
    /// </summary>
    public class RoomListItem : MonoBehaviour
    {
        public TMP_Text roomNameText;
        public TMP_Text playerCountText;
        public Button joinButton;

        private string myRoomId;

        public void Setup(RoomData roomInfo)
        {
            myRoomId = roomInfo.roomId;
            roomNameText.text = roomInfo.roomName;

            int currentCount = roomInfo.players != null ? roomInfo.players.Count : 0;
            playerCountText.text = $"{currentCount}/{roomInfo.maxPlayers}";

            joinButton.onClick.RemoveAllListeners();
            joinButton.onClick.AddListener(OnJoinClicked);
        }

        private void OnJoinClicked()
        {
            Debug.Log($"UI: Đang gửi lệnh JOIN phòng {myRoomId}...");
            joinButton.interactable = false;

            // ================== GIẢ LẬP SERVER ==================
            Invoke(nameof(MockJoinResponse), 1f);
        }

        private void MockJoinResponse()
        {
            RoomData fakeRoom = new RoomData
            {
                roomId = myRoomId,
                roomName = "Phòng vừa Join",
                hostId = "other_player",
                status = "WAITING",
                players = new System.Collections.Generic.List<PlayerData>
                {
                    new PlayerData { playerId = "other_player", playerName = "Host Gốc", isHost = true },
                    new PlayerData { playerId = "123", playerName = "Bạn (Test)", isHost = false }
                }
            };
            GameManager.Instance.CurrentRoom = fakeRoom;
            GameManager.Instance.OnJoinRoomSuccess?.Invoke(fakeRoom);
        }
    }
}
