using UnityEngine;
using UnityEngine.UI;

namespace ClassroomQuiz.UI
{
    public class MainMenuPanel : MonoBehaviour
    {
        public Button playButton;

        private void Start()
        {
            if (playButton != null) playButton.onClick.AddListener(OnPlayButtonClicked);
            
            // ĐĂNG KÝ SỰ KIỆN: Báo cho GameManager biết là khi nào kết nối thành công thì gọi hàm GoToLobby
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnConnectSuccess += GoToLobby;
            }
        }

        private void OnDestroy()
        {
            // HỦY ĐĂNG KÝ khi Panel này bị tắt/xóa (quan trọng để chống lỗi bộ nhớ)
            if (GameManager.Instance != null)
            {
                GameManager.Instance.OnConnectSuccess -= GoToLobby;
            }
        }

        public void OnPlayButtonClicked()
        {
            Debug.Log("UI: Đã bấm nút Play! Đang kết nối Server...");
            
            // 1. VIỆC CỦA NGƯỜI B: Đổi chữ nút thành "Đang kết nối..." hoặc hiện icon xoay vòng ở đây.
            playButton.interactable = false; // Khóa nút tránh bấm 2 lần

            // 2. GỌI NGƯỜI A: Gửi lệnh lên Server (Chỗ này giả lập code của Người A)
            // Lẽ ra Người A sẽ viết: NetworkClient.Instance.Connect("127.0.0.1");
            
            // ================== GIẢ LẬP SERVER TRẢ LỜI SAU 1 GIÂY ==================
            // Đoạn này Tạm thời để test. Khi ghép code thật, Người A sẽ xóa đoạn Invoke này.
            Invoke(nameof(MockServerResponse), 1f); 
        }

        private void MockServerResponse()
        {
            // NGƯỜI A SẼ GỌI DÒNG NÀY (Bên trong file PacketHandler) KHI NHẬN ĐƯỢC TIN NHẮN TỪ SERVER:
            GameManager.Instance.OnConnectSuccess?.Invoke(); 
        }

        // --- HÀM NÀY CHỈ ĐƯỢC GỌI KHI SERVER BÁO "OK" ---
        private void GoToLobby()
        {
            Debug.Log("UI: Server cho phép! Chuyển sang Lobby.");
            playButton.interactable = true; // Mở lại nút
            MenuUIManager.Instance.ShowLobby();
        }
    }
}
