using System;
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

        [Tooltip("Tốc độ va chạm tối thiểu mới coi là một cú đập đáng nghe. Thấp hơn thì khối chỉ " +
                 "đang cọ vào bệ, phát tiếng sẽ thành lạo xạo liên tục.")]
        [SerializeField] float hardImpactSpeed = 2.5f;

        Action impacted;

        public Rigidbody2D Body => body;

        /* Builder nối lại mỗi lần dựng level — khối bị huỷ và tạo lại nên không giữ được đăng ký cũ. */
        public void BindImpact(Action handler)
        {
            impacted = handler;
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            if (collision.relativeVelocity.sqrMagnitude < hardImpactSpeed * hardImpactSpeed) return;
            impacted?.Invoke();
        }

        public void ApplyBlast(Vector2 origin, float force, float radius)
        {
            /* Đo bằng transform, KHÔNG phải body.position: level builder đặt vị trí qua transform,
               còn Rigidbody2D chỉ chép lại ở bước vật lý kế tiếp — nên ở lượt bắn đầu tiên mọi
               body.position đều còn là gốc toạ độ, và cú nổ đẩy sai cả hướng lẫn độ mạnh. */
            var toBlock = (Vector2)transform.position - origin;
            var distance = toBlock.magnitude;
            if (distance > radius) return;

            /* Giảm tuyến tính theo khoảng cách: đứng sát tâm nổ thì bay, đứng mép thì chỉ rung.
               Người chơi cần cảm nhận được rằng "nổ gần hơn = đẩy mạnh hơn" để chọn điểm nổ. */
            var falloff = 1f - distance / radius;

            /* Nổ gần như trùng tâm khối thì hướng đẩy vô nghĩa: lệch vài phần trăm unit là ra hướng
               xuống, khối bị ép vào bệ và đứng im — người chơi bắn trúng mà tưởng game hỏng.
               Ngưỡng lấy nhỏ hơn nửa cạnh khối, nên chỉ những cú nổ THỰC SỰ nằm trong khối mới rơi
               vào đây, và khi đó hất thẳng lên là hợp trực giác nhất. */
            const float minDirectionDistance = 0.25f;
            var direction = distance > minDirectionDistance ? toBlock / distance : Vector2.up;
            direction = (direction + Vector2.up * tuning.BlastUpwardBias).normalized;

            body.AddForce(direction * (force * falloff), ForceMode2D.Impulse);
            /* Chỉ rõ UnityEngine: file có using System nên Random trần là nhập nhằng. */
            var spin = UnityEngine.Random.Range(-1f, 1f);
            body.AddTorque(spin * tuning.BlastTorque * falloff, ForceMode2D.Impulse);
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
