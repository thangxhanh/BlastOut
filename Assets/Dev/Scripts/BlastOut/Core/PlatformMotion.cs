using System;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Core
{
    public enum MotionKind
    {
        /* Đứng yên — mặc định, giữ nguyên hành vi của những level không dùng chuyển động. */
        Static,
        Horizontal,
        Vertical,
        Circle
    }

    /* Quỹ đạo của một bệ chuyển động, tính thuần bằng toán từ thời gian — không giữ trạng thái.
       Nhờ vậy vị trí chỉ phụ thuộc t: level chơi lại là mọi bệ về đúng chỗ cũ, và hai bệ cùng
       tham số nhưng khác Phase thì lệch nhau ổn định chứ không trôi dần theo số frame đã chạy. */
    [Serializable]
    public struct PlatformMotion
    {
        [SerializeField] MotionKind kind;

        [Tooltip("Biên độ: nửa quãng đường với Horizontal/Vertical, bán kính với Circle.")]
        [SerializeField] float distance;

        [Tooltip("Số vòng (hoặc số lần đi–về) mỗi giây.")]
        [SerializeField] float speed;

        [Tooltip("Lệch pha 0..1. Hai bệ cùng tham số nhưng khác pha sẽ không đi song song.")]
        [SerializeField] float phase;

        public MotionKind Kind => kind;
        public bool IsMoving => kind != MotionKind.Static && distance > 0f && speed != 0f;

        /* Độ lệch so với vị trí gốc tại thời điểm time. */
        /* Quãng khởi động mềm. Quỹ đạo dạng sin có một cái bẫy: tại t = 0 thì vị trí bằng 0 nhưng
           VẬN TỐC đã là cực đại, nên bệ lao đi hết tốc lực ngay từ trạng thái đứng yên. Ma sát
           không kịp bắt và vật đặt trên trượt lại một đoạn — nhìn thấy rõ ở giây đầu của level.

           Nhân biên độ với một hàm tăng dần (smoothstep) làm cả vận tốc lẫn gia tốc đều xuất phát
           từ 0, nên bệ "nhấn ga" thay vì "giật mình". */
        const float StartRamp = 1.1f;

        public Vector2 Offset(float time)
        {
            return RawOffset(time) * Ramp(time);
        }

        Vector2 RawOffset(float time)
        {
            if (!IsMoving) return Vector2.zero;

            var angle = (time * speed + phase) * Mathf.PI * 2f;

            switch (kind)
            {
                case MotionKind.Horizontal:
                    return new Vector2(Mathf.Sin(angle) * distance, 0f);
                case MotionKind.Vertical:
                    return new Vector2(0f, Mathf.Sin(angle) * distance);
                case MotionKind.Circle:
                    return new Vector2(Mathf.Cos(angle), Mathf.Sin(angle)) * distance;
                default:
                    return Vector2.zero;
            }
        }

        static float Ramp(float time)
        {
            var u = Mathf.Clamp01(time / StartRamp);
            return u * u * (3f - 2f * u);
        }

        static float RampSlope(float time)
        {
            if (time >= StartRamp) return 0f;
            var u = Mathf.Clamp01(time / StartRamp);
            return 6f * u * (1f - u) / StartRamp;
        }

        /* Vận tốc tại thời điểm time — đạo hàm của Offset.
           Bệ được đẩy bằng vận tốc chứ không gán thẳng vị trí: có vận tốc thì vật đặt trên mới
           được ma sát kéo theo, gán vị trí thì bệ trượt ngay dưới chân vật. */
        public Vector2 Velocity(float time)
        {
            if (!IsMoving) return Vector2.zero;

            /* Vị trí là TÍCH của quỹ đạo và hệ số khởi động, nên vận tốc phải theo quy tắc nhân.
               Lấy mỗi đạo hàm của quỹ đạo là vận tốc lệch khỏi vị trí trong suốt quãng khởi động,
               và bộ hiệu chỉnh trong MovingPlatform sẽ phải kéo ngược liên tục — đúng cái giật mà
               việc khởi động mềm đang muốn loại bỏ. */
            return RawVelocity(time) * Ramp(time) + RawOffset(time) * RampSlope(time);
        }

        Vector2 RawVelocity(float time)
        {
            var omega = speed * Mathf.PI * 2f;
            var angle = (time * speed + phase) * Mathf.PI * 2f;

            switch (kind)
            {
                case MotionKind.Horizontal:
                    return new Vector2(Mathf.Cos(angle) * distance * omega, 0f);
                case MotionKind.Vertical:
                    return new Vector2(0f, Mathf.Cos(angle) * distance * omega);
                case MotionKind.Circle:
                    return new Vector2(-Mathf.Sin(angle), Mathf.Cos(angle)) * distance * omega;
                default:
                    return Vector2.zero;
            }
        }
    }
}
