using UnityEngine;

namespace ClassroomQuiz.UI
{
    /// <summary>
    /// Gắn script này vào Canvas chính (hoặc GameObject MenuUIManager) trong MenuScene.
    /// Kéo thả các Panel tương ứng vào các ô trong Inspector.
    /// </summary>
    public class MenuUIManager : MonoBehaviour
    {
        public static MenuUIManager Instance;

        [Header("UI Panels (Kéo thả từ Hierarchy vào đây)")]
        public GameObject mainMenuPanel;
        public GameObject lobbyPanel;
        public GameObject roomListPanel;
        public GameObject roomPanel;

        private void Awake()
        {
            if (Instance == null) Instance = this;
            else Destroy(gameObject);
        }

        private void Start()
        {
            // Khởi đầu: Chỉ hiện Main Menu
            ShowPanel(mainMenuPanel);
        }

        // --- HÀM ẨN/HIỆN PANEL CHO CÁC SCRIPT KHÁC GỌI ---
        public void ShowMainMenu() => ShowPanel(mainMenuPanel);
        public void ShowLobby() => ShowPanel(lobbyPanel);
        public void ShowRoomList() => ShowPanel(roomListPanel);
        public void ShowRoom() => ShowPanel(roomPanel);

        // Logic nội bộ để tắt hết các panel và chỉ bật panel được chỉ định
        private void ShowPanel(GameObject panelToShow)
        {
            if(mainMenuPanel) mainMenuPanel.SetActive(false);
            if(lobbyPanel) lobbyPanel.SetActive(false);
            if(roomListPanel) roomListPanel.SetActive(false);
            if(roomPanel) roomPanel.SetActive(false);

            if (panelToShow != null)
            {
                panelToShow.SetActive(true);
            }
        }
    }
}
