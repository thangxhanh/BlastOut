using UnityEngine;
using UnityEngine.UI;
using DG.Tweening;

public class ScreenFlash : MonoBehaviour
{
    [SerializeField] private Image flashImage; // Image che phủ màn hình
    [SerializeField] private float flashDuration = 0.5f; // Tổng thời gian nháy
    [SerializeField] private float maxAlpha = 0.5f; // Độ sáng tối đa của đỏ (alpha)

    private void Awake()
    {
        if (flashImage != null)
        {
            // Đảm bảo flashImage bắt đầu ở alpha = 0
            Color color = flashImage.color;
            color.a = 0f;
            flashImage.color = color;
        }
    }

    public void FlashScreen()
    {
        if (flashImage == null) return;

        flashImage.gameObject.SetActive(true);
        flashImage.DOFade(maxAlpha, flashDuration / 2) // Fade in
            .OnComplete(() =>
            {
                flashImage.DOFade(0f, flashDuration / 2); // Fade out
            });
    }
}