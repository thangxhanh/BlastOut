using Dacodelaac.Core;
using Dev.Scripts.BlastOut.Config;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Khối mục tiêu. Không có máu, không vỡ — chỉ bị đẩy.
       Đó là quyết định thiết kế trung tâm: bỏ destruction thì vật lý đọc được, và bỏ luôn được
       cả hệ thống HP lẫn debris (xem README, mục Important decisions). */
    [RequireComponent(typeof(Rigidbody2D))]
    public class TargetBlock : BaseMono, IBlastable
    {
        [SerializeField] Rigidbody2D body;
        [SerializeField] BlastTuning tuning;

        public Rigidbody2D Body => body;

        public void ApplyBlast(Vector2 origin, float force, float radius)
        {
            var toBlock = body.position - origin;
            var distance = toBlock.magnitude;
            if (distance > radius) return;

            /* Giảm tuyến tính theo khoảng cách: đứng sát tâm nổ thì bay, đứng mép thì chỉ rung.
               Người chơi cần cảm nhận được rằng "nổ gần hơn = đẩy mạnh hơn" để chọn điểm nổ. */
            var falloff = 1f - distance / radius;

            /* Khoảng cách 0 xảy ra khi đạn nổ đúng tâm khối — không có hướng để đẩy, chọn hướng lên. */
            var direction = distance > 0.001f ? toBlock / distance : Vector2.up;
            direction = (direction + Vector2.up * tuning.BlastUpwardBias).normalized;

            body.AddForce(direction * (force * falloff), ForceMode2D.Impulse);
            body.AddTorque(Random.Range(-1f, 1f) * tuning.BlastTorque * falloff, ForceMode2D.Impulse);
        }

        public bool IsSettled(float speedThreshold)
        {
            return body.linearVelocity.sqrMagnitude <= speedThreshold * speedThreshold;
        }

        /* Cố ý KHÔNG khai báo Reset() ở đây: BaseMono đã có một Reset() private tự gán ticker/pools,
           và Unity chỉ gọi MỘT hàm cho mỗi message tính từ class cụ thể nhất — khai báo thêm ở đây
           sẽ che mất hàm của base và hai reference kia lặng lẽ để null. Ref được gán bằng
           BlastOutSceneBuilder lúc dựng prefab. */
    }
}
