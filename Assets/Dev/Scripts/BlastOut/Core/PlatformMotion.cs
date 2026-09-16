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
        public Vector2 Offset(float time)
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

        /* Vận tốc tại thời điểm time — đạo hàm của Offset.
           Bệ được đẩy bằng vận tốc chứ không gán thẳng vị trí: có vận tốc thì vật đặt trên mới
           được ma sát kéo theo, gán vị trí thì bệ trượt ngay dưới chân vật. */
        public Vector2 Velocity(float time)
        {
            if (!IsMoving) return Vector2.zero;

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
