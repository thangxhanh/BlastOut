using Dacodelaac.Core;
using Dev.Scripts.BlastOut.Core;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Bệ chạy theo quỹ đạo khai báo trong level data. Thùng và khối đặt lên trên được ma sát kéo
       theo, nên người chơi phải canh thời điểm bắn chứ không chỉ canh đường ngắm.

       Body là Kinematic: bệ đẩy được vật khác nhưng không bị vụ nổ thổi bay — một bệ bị bắn lệch
       khỏi quỹ đạo thì level thành không thể đoán. */
    [RequireComponent(typeof(Rigidbody2D))]
    public class MovingPlatform : BaseMono
    {
        [SerializeField] Rigidbody2D body;

        PlatformMotion motion;
        Vector2 origin;

        public void Bind(Vector2 startPosition, PlatformMotion value)
        {
            origin = startPosition;
            motion = value;

            body.bodyType = RigidbodyType2D.Kinematic;
            body.position = origin + motion.Offset(0f);
            body.linearVelocity = Vector2.zero;
        }

        /* Gọi từ BlastGameController mỗi frame — không tự Update để thứ tự trong một frame còn
           đọc được (xem chú thích ở controller). */
        public void ManualTick(float time)
        {
            if (!motion.IsMoving) return;

            /* Vận tốc lý thuyết + hiệu chỉnh về đúng quỹ đạo. Chỉ dùng vận tốc thì sai số tích
               phân dồn lại làm bệ trôi dần khỏi đường đi sau vài chục giây. */
            const float correction = 4f;

            var target = origin + motion.Offset(time);
            body.linearVelocity = motion.Velocity(time) + (target - body.position) * correction;
        }
    }
}
