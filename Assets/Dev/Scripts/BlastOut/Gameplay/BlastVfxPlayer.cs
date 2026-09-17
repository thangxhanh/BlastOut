using Dacodelaac.Core;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Phát hiệu ứng nổ. Giữ sẵn một vòng các bản sao và quay vòng dùng lại — nổ dây chuyền có thể
       bắn ra vài vụ nổ liền nhau, mà Instantiate giữa lúc chơi là điều CLAUDE.md §9.4 cấm.

       Vòng quay không cần biết hiệu ứng nào đã tắt: quay đủ một vòng thì bản cũ nhất chắc chắn đã
       chạy xong, và kể cả chưa xong thì Play() lại từ đầu vẫn đúng hình. */
    public class BlastVfxPlayer : BaseMono
    {
        [SerializeField] ParticleSystem explosionPrefab;
        [Tooltip("Mảnh vỡ, chỉ dùng cho thùng nổ.")]
        [SerializeField] ParticleSystem debrisPrefab;
        [SerializeField] Transform container;

        [Tooltip("Số vụ nổ có thể chồng lên nhau. Dây chuyền dài nhất trong các level hiện tại là 2.")]
        [SerializeField] int poolSize = 6;

        [Tooltip("Bán kính vụ nổ mà prefab được dựng theo. Hiệu ứng phóng to/thu nhỏ theo tỉ lệ này " +
                 "để vụ nổ của thùng trông to hơn của đạn thường.")]
        [SerializeField] float referenceRadius = 1.9f;

        ParticleSystem[] instances;
        ParticleSystem[] debris;
        int next;
        int nextDebris;

        public override void Initialize()
        {
            base.Initialize();

            instances = Spawn(explosionPrefab, Mathf.Max(1, poolSize));

            /* Ít bản hơn vụ nổ: cùng lúc hiếm khi có quá hai thùng phát nổ. */
            debris = Spawn(debrisPrefab, 3);
        }

        ParticleSystem[] Spawn(ParticleSystem prefab, int count)
        {
            if (!prefab) return null;

            var pool = new ParticleSystem[count];
            for (var i = 0; i < count; i++)
            {
                pool[i] = Instantiate(prefab, container);
                pool[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }

            return pool;
        }

        public void PlayExplosion(Vector2 position, float radius)
        {
            if (instances == null || instances.Length == 0) return;

            var effect = instances[next];
            next = (next + 1) % instances.Length;

            var scale = referenceRadius > 0.001f ? radius / referenceRadius : 1f;
            effect.transform.SetPositionAndRotation(position, Quaternion.identity);
            effect.transform.localScale = new Vector3(scale, scale, 1f);

            /* withChildren = true: flash, khói, tia lửa và vòng xung kích là các hệ con riêng. */
            effect.Play(true);
        }

        /* Chỉ thùng nổ gọi tới — đạn nổ giữa không trung thì không có gì để vỡ. */
        public void PlayDebris(Vector2 position)
        {
            if (debris == null || debris.Length == 0) return;

            var effect = debris[nextDebris];
            nextDebris = (nextDebris + 1) % debris.Length;

            effect.transform.SetPositionAndRotation(position, Quaternion.identity);
            effect.Play(true);
        }
    }
}
