using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý giao diện hiển thị câu hỏi và 4 lựa chọn đáp án cho người chơi.
/// </summary>
public class QuestionDisplay : MonoBehaviour
{
    [Header("Question UI References")]
    [Tooltip("Text hiển thị số thứ tự câu hỏi / vòng chơi")]
    [SerializeField] private Text txtQuestionNumber;

    [Tooltip("Text hiển thị nội dung câu hỏi")]
    [SerializeField] private Text txtQuestion;

    [Header("Answer Options UI")]
    [Tooltip("Danh sách 4 nút bấm tương ứng 4 đáp án A, B, C, D")]
    [SerializeField] private Button[] answerButtons = new Button[4];

    [Tooltip("Text hiển thị nội dung của 4 phương án trả lời")]
    [SerializeField] private Text[] answerTexts = new Text[4];

    [Tooltip("Image nền của 4 nút đáp án để thay đổi màu sắc chỉ thị")]
    [SerializeField] private Image[] answerBackgrounds = new Image[4];

    [Header("Color States")]
    [Tooltip("Màu nền nút khi ở trạng thái mặc định")]
    [SerializeField] private Color normalColor = Color.white;

    [Tooltip("Màu nền nút khi hiển thị đáp án đúng")]
    [SerializeField] private Color correctColor = Color.green;

    [Tooltip("Màu nền nút khi người chơi chọn đáp án sai")]
    [SerializeField] private Color wrongColor = Color.red;

    /// <summary>
    /// Hiển thị thông tin câu hỏi mới cùng 4 đáp án lên giao diện.
    /// </summary>
    /// <param name="round">Vòng chơi hiện tại</param>
    /// <param name="question">Nội dung câu hỏi</param>
    /// <param name="answers">Mảng chứa 4 phương án trả lời</param>
    public void DisplayQuestion(int round, string question, string[] answers)
    {
        // TODO: Cập nhật txtQuestionNumber.text theo định dạng vòng chơi (ví dụ: "Câu {round}")
        // TODO: Cập nhật txtQuestion.text = question
        // TODO: Gọi ResetColors() để đặt lại màu nút mặc định
        // TODO: Gán nội dung từng đáp án vào answerTexts[i].text
    }

    /// <summary>
    /// Làm nổi bật đáp án đúng bằng màu xanh (correctColor).
    /// </summary>
    /// <param name="index">Chỉ số đáp án đúng (0-3)</param>
    public void HighlightCorrect(int index)
    {
        // TODO: Kiểm tra index hợp lệ (0 <= index < answerBackgrounds.Length)
        // TODO: Gán answerBackgrounds[index].color = correctColor
    }

    /// <summary>
    /// Làm nổi bật đáp án sai bằng màu đỏ (wrongColor).
    /// </summary>
    /// <param name="index">Chỉ số đáp án sai (0-3)</param>
    public void HighlightWrong(int index)
    {
        // TODO: Kiểm tra index hợp lệ (0 <= index < answerBackgrounds.Length)
        // TODO: Gán answerBackgrounds[index].color = wrongColor
    }

    /// <summary>
    /// Khôi phục màu nền của toàn bộ các nút đáp án về màu mặc định (normalColor).
    /// </summary>
    public void ResetColors()
    {
        // TODO: Lặp qua từng phần tử trong answerBackgrounds và gán color = normalColor
    }

    /// <summary>
    /// Bật hoặc tắt khả năng bấm (interactable) của toàn bộ 4 nút đáp án.
    /// </summary>
    /// <param name="on">True nếu cho phép bấm, False nếu vô hiệu hóa</param>
    public void SetInteractable(bool on)
    {
        // TODO: Lặp qua từng nút trong answerButtons và gán interactable = on
    }
}
