using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Đại diện cho avatar của người chơi tại vị trí bàn học trong phòng chơi.
/// </summary>
public class PlayerAvatar : MonoBehaviour
{
    [Header("Visual References")]
    [Tooltip("SpriteRenderer hiển thị hình ảnh nhân vật")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Animator điều khiển hoạt ảnh nhân vật")]
    [SerializeField] private Animator animator;

    [Tooltip("Text hiển thị tên người chơi")]
    [SerializeField] private Text txtName;

    [Tooltip("Danh sách Sprite của 3 nhân vật (tương ứng characterId 0, 1, 2)")]
    [SerializeField] private Sprite[] characterSprites;

    [Tooltip("Đối tượng viền hoặc hiệu ứng làm nổi bật người đang được trả lời")]
    [SerializeField] private GameObject highlightIndicator;

    [Header("Player Data")]
    [Tooltip("ID của người chơi sở hữu avatar này")]
    [SerializeField] private string playerId;

    public string PlayerId => playerId;

    /// <summary>
    /// Khởi tạo và thiết lập thông tin avatar từ dữ liệu PlayerData.
    /// </summary>
    /// <param name="data">Dữ liệu người chơi nhận được từ phòng/mạng</param>
    public void Setup(PlayerData data)
    {
        // TODO: Lưu playerId từ data.playerId
        // TODO: Gán tên người chơi vào txtName.text
        // TODO: Kiểm tra data.characterId hợp lệ (0-2) và gán sprite tương ứng từ characterSprites vào spriteRenderer.sprite
    }

    /// <summary>
    /// Kích hoạt hoạt ảnh giơ tay để giành quyền trả lời câu hỏi.
    /// </summary>
    public void PlayRaiseHandAnimation()
    {
        // TODO: Kích hoạt Trigger hoặc gán Bool trên Animator để thực hiện animation giơ tay (RaiseHand)
    }

    /// <summary>
    /// Đưa nhân vật trở lại trạng thái ngồi/đứng yên (Idle).
    /// </summary>
    public void PlayIdleAnimation()
    {
        // TODO: Chuyển Animator về trạng thái Idle
    }

    /// <summary>
    /// Bật hoặc tắt hiệu ứng thị giác nổi bật (highlight) cho người chơi đang có lượt trả lời.
    /// </summary>
    /// <param name="on">True nếu được quyền trả lời, False nếu tắt</param>
    public void SetHighlight(bool on)
    {
        // TODO: Kích hoạt hoặc ẩn highlightIndicator (SetActive(on))
    }
}
