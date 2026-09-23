using System;
using UnityEngine;
using UnityEngine.UI;

/// <summary>
/// Quản lý đồng hồ đếm ngược thời gian cho lượt chơi và các pha trả lời câu hỏi.
/// </summary>
public class TimerController : MonoBehaviour
{
    [Header("UI References")]
    [Tooltip("Hình ảnh thanh đếm giờ dạng vòng tròn hoặc thanh ngang (Image type Filled)")]
    [SerializeField] private Image timerFillImage;

    [Tooltip("Text hiển thị số giây đếm ngược còn lại")]
    [SerializeField] private Text txtTimeLeft;

    [Header("Timer State")]
    [SerializeField] private float maxTime;
    [SerializeField] private float currentTime;
    [SerializeField] private bool isRunning;

    /// <summary>
    /// Sự kiện kích hoạt khi thời gian đếm ngược kết thúc (về 0).
    /// </summary>
    public event Action OnTimerExpired;

    /// <summary>
    /// Bắt đầu bộ đếm ngược với số giây quy định.
    /// </summary>
    /// <param name="seconds">Tổng thời gian đếm ngược (giây)</param>
    public void StartTimer(float seconds)
    {
        // TODO: Thiết lập maxTime = seconds
        // TODO: Thiết lập currentTime = seconds
        // TODO: Đặt isRunning = true
        // TODO: Cập nhật hiển thị UI ban đầu (timerFillImage, txtTimeLeft)
    }

    /// <summary>
    /// Dừng đếm ngược thời gian.
    /// </summary>
    public void StopTimer()
    {
        // TODO: Đặt isRunning = false
    }

    /// <summary>
    /// Lấy thời gian còn lại của đồng hồ đếm ngược.
    /// </summary>
    /// <returns>Số giây còn lại (float)</returns>
    public float GetRemainingTime()
    {
        // TODO: Trả về số giây còn lại
        return currentTime;
    }

    private void Update()
    {
        if (!isRunning) return;

        // TODO: Giảm currentTime theo Time.deltaTime (currentTime -= Time.deltaTime)
        // TODO: Cập nhật fillAmount của thanh hiển thị nếu có timerFillImage (timerFillImage.fillAmount = currentTime / maxTime)
        // TODO: Cập nhật txtTimeLeft.text theo số nguyên giây còn lại (ví dụ Mathf.CeilToInt(currentTime).ToString())
        // TODO: Kiểm tra nếu currentTime <= 0:
        //       - Gán currentTime = 0
        //       - Gọi StopTimer()
        //       - Kích hoạt sự kiện OnTimerExpired?.Invoke()
    }
}
