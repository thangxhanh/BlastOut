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

        const int SplitCount = 3;
        const float SplitSpreadDegrees = 26f;

        BlastResolver resolver;
        Action<BlastProjectile> despawned;
        Action<Vector2, Vector2> splitRequested;
        AmmoType ammoType;
        bool canSplit;
        bool consumed;
        float aliveTime;

        public bool Consumed => consumed;
        public Vector2 Position => body.position;

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
            ammoType = type;
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

            if (trail)
            {
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

            var origin = body.position;

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
                splitRequested?.Invoke(origin, direction * speed);
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            /* Chạm đất/bệ mà chưa kích nổ thì vẫn nổ — người chơi không bao giờ mất lượt vì quên chạm. */
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
