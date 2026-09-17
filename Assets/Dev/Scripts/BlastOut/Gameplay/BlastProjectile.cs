using System;
using Dacodelaac.Core;
using Dev.Scripts.BlastOut.Config;
using Dev.Scripts.BlastOut.Core;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Viên đạn. Bay theo trọng lực, nổ khi người chơi chạm màn hình hoặc khi chạm vật thể.
       Không tự đếm lượt, không tự quyết thắng thua — chỉ gọi callback báo "tôi xong rồi". */
    [RequireComponent(typeof(Rigidbody2D))]
    public class BlastProjectile : BaseMono
    {
        [SerializeField] Rigidbody2D body;
        [SerializeField] BlastTuning tuning;
        [SerializeField] TrailRenderer trail;
        [SerializeField] SpriteRenderer view;

        /* Hai loại đạn hành xử khác hẳn nhau nhưng dùng chung một prefab, nên màu là thứ DUY NHẤT
           cho người chơi biết mình đang cầm viên gì. Giống hệt nhau thì mechanic tách đạn coi như
           vô hình: không ai canh được cú tách nếu không biết viên này tách được. */
        [SerializeField] Color bombTint = new Color(0.89f, 0.96f, 0.98f);
        [SerializeField] Color splitterTint = new Color(0.78f, 0.48f, 1f);

        const int SplitCount = 3;
        const float SplitSpreadDegrees = 26f;

        /* Lớn hơn bán kính collider của đạn (0.18 sau khi scale), để ba mảnh rời nhau ngay từ frame
           đầu thay vì chồng lên nhau rồi mới toả ra. */
        const float SplitSpawnOffset = 0.3f;

        BlastResolver resolver;
        Action<BlastProjectile> despawned;
        Action<Vector2, Vector2> splitRequested;
        bool canSplit;
        bool consumed;
        float aliveTime;

        public bool Consumed => consumed;
        /* transform chứ không phải body.position: đạn vừa được pool đặt vị trí qua transform, còn
           Rigidbody2D phải tới bước vật lý kế tiếp mới chép lại. Kích nổ ngay frame đầu mà lấy
           body.position thì vụ nổ xảy ra ở gốc toạ độ chứ không phải chỗ viên đạn đang bay. */
        public Vector2 Position => transform.position;

        /* Biến mất mà KHÔNG nổ — dùng khi đạn đã bay khỏi vùng chơi. Nổ ở ngoài màn thì người chơi
           không thấy gì, chỉ thấy game đứng im chờ, nên coi như cú bắn đã hỏng và kết thúc luôn. */
        public void Discard()
        {
            if (consumed) return;
            consumed = true;
            Despawn();
        }

        public void Launch(Vector2 velocity, AmmoType type, BlastResolver blastResolver,
            Action<BlastProjectile> onDespawned, Action<Vector2, Vector2> onSplitRequested)
        {
            resolver = blastResolver;
            despawned = onDespawned;
            splitRequested = onSplitRequested;

            /* Mảnh tách ra không được tách tiếp — nếu không, một viên Splitter sinh ra 3, rồi 9, rồi 27. */
            canSplit = type == AmmoType.Splitter;
            consumed = false;
            aliveTime = 0f;

            body.gravityScale = tuning.ProjectileGravityScale;
            body.linearVelocity = velocity;
            body.angularVelocity = 0f;

            var tint = canSplit ? splitterTint : bombTint;
            if (view) view.color = tint;

            if (trail)
            {
                /* Vệt đạn cũng đổi màu theo: lúc đạn đang bay nhanh thì vệt còn dễ thấy hơn cả đạn. */
                trail.startColor = tint;
                trail.endColor = new Color(tint.r, tint.g, tint.b, 0f);

                trail.Clear();
                trail.emitting = true;
            }
        }

        /* Được BlastGameController gọi, không phải Update() riêng — giữ đúng một điểm vào mỗi frame. */
        public void ManualTick(float deltaTime)
        {
            if (consumed) return;

            aliveTime += deltaTime;
            if (aliveTime >= tuning.ProjectileLifetime) Detonate();
        }

        /* Người chơi chạm màn hình lúc đạn đang bay. Đây là quyết định đắt giá nhất trong game. */
        public void Detonate()
        {
            if (consumed) return;
            consumed = true;

            var origin = Position;

            if (canSplit)
            {
                Split(origin);
            }
            else
            {
                resolver?.Blast(origin, tuning.BlastForce, tuning.BlastRadius);
            }

            Despawn();
        }

        void Split(Vector2 origin)
        {
            var velocity = body.linearVelocity;

            /* Tách lúc đạn gần như đứng yên (đỉnh parabol) thì hướng quạt không xác định — chọn hướng lên. */
            if (velocity.sqrMagnitude < 0.01f) velocity = Vector2.up * tuning.MinLaunchSpeed;

            var baseAngle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            var speed = velocity.magnitude;

            for (var i = 0; i < SplitCount; i++)
            {
                var offset = (i - (SplitCount - 1) * 0.5f) * SplitSpreadDegrees;
                var radians = (baseAngle + offset) * Mathf.Deg2Rad;
                var direction = new Vector2(Mathf.Cos(radians), Mathf.Sin(radians));

                /* Đẩy mỗi mảnh ra trước một đoạn theo hướng của nó. Sinh cả ba đúng một điểm thì
                   khoảnh khắc đầu tiên chúng chồng lên nhau, nhìn ra một chấm chứ không ra chùm
                   ba tia — cú tách chỉ "đọc" được nếu thấy chúng rời nhau ngay lúc tách. */
                splitRequested?.Invoke(origin + direction * SplitSpawnOffset, direction * speed);
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            /* Đạn KHÔNG kích nổ đạn. Ba mảnh của Splitter sinh ra sát nhau nên chạm nhau ngay frame
               đầu tiên; tính đó là va chạm thì cả ba nổ tức thì và người chơi chẳng bao giờ thấy
               cú tách — chỉ thấy một vụ nổ ngay chỗ vừa chạm màn hình. */
            if (collision.collider.TryGetComponent<BlastProjectile>(out _)) return;

            /* Chạm đất/bệ mà chưa kích nổ thì vẫn nổ — người chơi không bao giờ mất lượt vì quên chạm.
               Không phát tiếng va chạm ở đây: vụ nổ xảy ra ngay lập tức và tiếng nổ đã che mất. */
            Detonate();
        }

        void Despawn()
        {
            if (trail) trail.emitting = false;
            gameObject.SetActive(false);
            despawned?.Invoke(this);
        }
    }
}
