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
        playerId = data.playerId;

        // Gán tên người chơi
        if (txtName != null)
        {
            txtName.text = data.playerName;
        }

        // Gán sprite nhân vật theo characterId (0, 1, 2)
        if (spriteRenderer != null && characterSprites != null
            && data.characterId >= 0 && data.characterId < characterSprites.Length)
        {
            spriteRenderer.sprite = characterSprites[data.characterId];
        }

        // Tắt highlight mặc định
        SetHighlight(false);
    }

    /// <summary>
    /// Kích hoạt hoạt ảnh giơ tay để giành quyền trả lời câu hỏi.
    /// </summary>
    public void PlayRaiseHandAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("RaiseHand");
        }
    }

    /// <summary>
    /// Đưa nhân vật trở lại trạng thái ngồi/đứng yên (Idle).
    /// </summary>
    public void PlayIdleAnimation()
    {
        if (animator != null)
        {
            animator.SetTrigger("Idle");
        }
    }

    /// <summary>
    /// Bật hoặc tắt hiệu ứng thị giác nổi bật (highlight) cho người chơi đang có lượt trả lời.
    /// </summary>
    /// <param name="on">True nếu được quyền trả lời, False nếu tắt</param>
    public void SetHighlight(bool on)
    {
        if (highlightIndicator != null)
        {
            highlightIndicator.SetActive(on);
        }
    }
}
