using System.Collections;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý hiển thị và hoạt ảnh của NPC Giáo viên trong lớp học.
/// </summary>
public class TeacherNPC : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Khung chữ hiển thị lời thoại / câu hỏi của giáo viên")]
    [SerializeField] private Text txtDialogue;

    [Header("Visual References")]
    [Tooltip("SpriteRenderer của giáo viên")]
    [SerializeField] private SpriteRenderer spriteRenderer;

    [Tooltip("Animator điều khiển chuyển động của giáo viên")]
    [SerializeField] private Animator animator;

    // Tham chiếu Coroutine hiển thị lời thoại tạm thời
    private Coroutine introCoroutine;

    /// <summary>
    /// Hiển thị câu giới thiệu trong một khoảng thời gian rồi tự động ẩn đi.
    /// </summary>
    /// <param name="text">Nội dung giới thiệu</param>
    /// <param name="duration">Thời gian hiển thị (giây)</param>
    public void ShowIntro(string text, float duration)
    {
        // TODO: Dừng Coroutine cũ nếu đang chạy để tránh đè lời thoại
        // TODO: Bắt đầu Coroutine ShowIntroCoroutine(text, duration)
    }

    /// <summary>
    /// Coroutine hỗ trợ hiển thị lời thoại có giới hạn thời gian.
    /// </summary>
    private IEnumerator ShowIntroCoroutine(string text, float duration)
    {
        // TODO: Cập nhật txtDialogue.text = text
        // TODO: Phát hoạt ảnh nói chuyện (PlayTalkAnimation)
        // TODO: Chờ hết khoảng thời gian duration (yield return new WaitForSeconds(duration))
        // TODO: Xóa text hoặc ẩn khung thoại
        // TODO: Chuyển về hoạt ảnh đứng yên (PlayIdleAnimation)
        yield break;
    }

    /// <summary>
    /// Hiển thị câu hỏi của vòng thi hiện tại lên khung thoại của giáo viên.
    /// </summary>
    /// <param name="question">Nội dung câu hỏi</param>
    public void ShowQuestion(string question)
    {
        // TODO: Cập nhật nội dung câu hỏi lên txtDialogue.text
        // TODO: Có thể kích hoạt animation chỉ tay hoặc nói ngắn hạn
    }

    /// <summary>
    /// Kích hoạt hoạt ảnh nói chuyện / giảng bài.
    /// </summary>
    public void PlayTalkAnimation()
    {
        // TODO: Kích hoạt Trigger hoặc gán Bool trên Animator để chuyển sang animation Talk
    }

    /// <summary>
    /// Kích hoạt hoạt ảnh đứng yên mặc định (Idle).
    /// </summary>
    public void PlayIdleAnimation()
    {
        // TODO: Chuyển Animator về trạng thái Idle
    }
}
