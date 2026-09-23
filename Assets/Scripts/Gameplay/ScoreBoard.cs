using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý bảng xếp hạng / điểm số của người chơi trong phòng thi đấu.
/// </summary>
public class ScoreBoard : MonoBehaviour
{
    /// <summary>
    /// Lớp nội bộ biểu diễn thông tin hiển thị của một dòng điểm người chơi.
    /// </summary>
    [System.Serializable]
    public class ScoreEntry
    {
        [Tooltip("Text hiển thị tên người chơi")]
        public Text txtName;

        [Tooltip("Text hiển thị điểm số")]
        public Text txtScore;

        [Tooltip("Đối tượng viền hoặc icon highlight người chiến thắng")]
        public GameObject highlightObject;
    }

    [Header("UI References")]
    [Tooltip("Transform cha (Content) chứa danh sách các dòng điểm")]
    [SerializeField] private Transform content;

    [Tooltip("Prefab dòng điểm người chơi để sinh tự động vào bảng điểm")]
    [SerializeField] private GameObject scoreEntryPrefab;

    // Danh sách lưu trữ các dòng điểm tương ứng theo playerId
    private Dictionary<string, ScoreEntry> entries = new Dictionary<string, ScoreEntry>();

    /// <summary>
    /// Khởi tạo bảng điểm với danh sách người chơi trong phòng.
    /// </summary>
    /// <param name="players">Danh sách PlayerData nhận từ phòng chơi</param>
    public void Initialize(List<PlayerData> players)
    {
        // TODO: Xóa sạch các mục hiển thị cũ đang có trong content
        // TODO: entries.Clear()
        // TODO: Duyệt qua từng player trong players:
        //       - Instantiate scoreEntryPrefab vào content
        //       - Lấy hoặc tạo đối tượng ScoreEntry và liên kết các component Text
        //       - Cập nhật txtName.text = player.playerName và txtScore.text = player.currentScore.ToString()
        //       - Thêm vào Dictionary entries với key là player.playerId
    }

    /// <summary>
    /// Cập nhật điểm số mới cho người chơi theo ID.
    /// </summary>
    /// <param name="playerId">ID của người chơi</param>
    /// <param name="score">Điểm số mới cần hiển thị</param>
    public void UpdateScore(string playerId, int score)
    {
        // TODO: Kiểm tra nếu entries chứa playerId:
        //       - Cập nhật entries[playerId].txtScore.text = score.ToString()
    }

    /// <summary>
    /// Làm nổi bật dòng điểm của người chiến thắng trong trận đấu.
    /// </summary>
    /// <param name="winnerId">ID của người chiến thắng (nếu để trống, có thể tính theo người điểm cao nhất)</param>
    public void HighlightWinner(string winnerId = null)
    {
        // TODO: Nếu winnerId được truyền vào:
        //       - Bật highlightObject của dòng điểm tương ứng
        // TODO: Nếu winnerId là null hoặc rỗng:
        //       - Tìm người có điểm số cao nhất trong entries để kích hoạt highlight
    }
}
