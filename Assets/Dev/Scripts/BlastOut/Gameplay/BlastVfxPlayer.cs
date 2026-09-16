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
        [SerializeField] Transform container;

        [Tooltip("Số vụ nổ có thể chồng lên nhau. Dây chuyền dài nhất trong các level hiện tại là 2.")]
        [SerializeField] int poolSize = 6;

        [Tooltip("Bán kính vụ nổ mà prefab được dựng theo. Hiệu ứng phóng to/thu nhỏ theo tỉ lệ này " +
                 "để vụ nổ của thùng trông to hơn của đạn thường.")]
        [SerializeField] float referenceRadius = 1.9f;

        ParticleSystem[] instances;
        int next;

        public override void Initialize()
        {
            base.Initialize();

            instances = new ParticleSystem[Mathf.Max(1, poolSize)];
            for (var i = 0; i < instances.Length; i++)
            {
                instances[i] = Instantiate(explosionPrefab, container);
                instances[i].Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
            }
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
    }
}
