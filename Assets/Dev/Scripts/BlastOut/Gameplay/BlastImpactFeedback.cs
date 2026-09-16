using Dacodelaac.Core;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Hai phản hồi đi kèm mỗi vụ nổ: khựng hình và rung máy.

       Khựng hình (hitstop) là một nhịp dừng rất ngắn ngay lúc nổ. Nó không phải hiệu ứng trang trí:
       khoảng lặng đó nói với người chơi "cú vừa rồi CÓ tác động", và nhờ dừng lại mà mắt kịp thấy
       vụ nổ trước khi mọi thứ bắt đầu văng.

       Rung máy bù lại phần lực mà hình ảnh phẳng không truyền tải được. Cả hai đều phải NGẮN —
       kéo dài thì thành khó chịu và làm người chơi mất kiểm soát. */
    public class BlastImpactFeedback : BaseMono
    {
        [SerializeField] Transform cameraTransform;

        [Header("Khựng hình")]
        [Tooltip("Thời gian dừng, tính bằng giây THỰC. Trên 0.1s là bắt đầu thấy giật khó chịu.")]
        [SerializeField] float hitStopDuration = 0.055f;

        [Header("Rung máy")]
        [SerializeField] float shakeDuration = 0.22f;
        [SerializeField] float shakeStrength = 0.22f;
        [Tooltip("Số lần đổi hướng mỗi giây. Thấp quá thành lắc lư, cao quá thành nhiễu.")]
        [SerializeField] float shakeFrequency = 26f;

        [Tooltip("Bán kính nổ dùng làm mốc. Vụ nổ lớn hơn thì rung mạnh hơn theo tỉ lệ.")]
        [SerializeField] float referenceRadius = 1.9f;

        Vector3 restPosition;
        float shakeElapsed;
        float shakeScale;
        float stopRemaining;
        bool stopped;

        public override void Initialize()
        {
            base.Initialize();
            restPosition = cameraTransform.localPosition;
        }

        /* Khớp chữ ký sự kiện BlastResolver.Blasted. */
        public void OnBlast(Vector2 position, float radius)
        {
            shakeScale = referenceRadius > 0.001f ? Mathf.Clamp(radius / referenceRadius, 0.6f, 2f) : 1f;
            shakeElapsed = 0f;

            stopRemaining = hitStopDuration;
            if (stopped) return;

            stopped = true;
            Time.timeScale = 0f;
        }

        /* Gọi từ BlastGameController mỗi frame. Dùng thời gian THỰC: lúc khựng hình thì timeScale
           bằng 0, nếu đếm bằng deltaTime thường thì đồng hồ đứng luôn và game treo vĩnh viễn. */
        public void ManualTick()
        {
            var realDelta = Time.unscaledDeltaTime;

            if (stopped)
            {
                stopRemaining -= realDelta;
                if (stopRemaining <= 0f)
                {
                    stopped = false;
                    Time.timeScale = 1f;
                }
            }

            TickShake(realDelta);
        }

        void TickShake(float realDelta)
        {
            if (shakeElapsed >= shakeDuration)
            {
                if (cameraTransform.localPosition != restPosition) cameraTransform.localPosition = restPosition;
                return;
            }

            shakeElapsed += realDelta;

            /* Tắt dần theo thời gian còn lại: rung mạnh nhất ngay lúc nổ rồi lắng xuống, thay vì
               rung đều rồi cắt phụt — cắt đột ngột nhìn như lỗi hiển thị. */
            var falloff = 1f - shakeElapsed / shakeDuration;
            var amount = shakeStrength * shakeScale * falloff * falloff;
            var phase = shakeElapsed * shakeFrequency;

            /* Hai tần số lệch nhau cho quỹ đạo không lặp lại; sin thuần sẽ thành dao động đều đặn. */
            var offset = new Vector3(
                Mathf.Sin(phase * 1.7f) * amount,
                Mathf.Cos(phase * 2.3f) * amount,
                0f);

            cameraTransform.localPosition = restPosition + offset;
        }

        /* Thoát play mode hoặc đổi scene giữa lúc đang khựng hình thì timeScale phải trả về 1,
           nếu không cả game đứng im ở lần chạy sau. */
        public override void CleanUp()
        {
            if (!stopped) return;
            stopped = false;
            Time.timeScale = 1f;
        }
    }
}
