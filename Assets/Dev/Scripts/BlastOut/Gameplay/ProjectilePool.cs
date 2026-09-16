using System.Collections.Generic;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Pool cục bộ cho đạn.

       Cố ý KHÔNG dùng Dacodelaac.Pools ở đây: Pools cần một PoolData khai báo sẵn trong asset và
       quản lý container DontDestroyOnLoad — thừa cho một prefab duy nhất sống trong đúng một scene.
       Cái CLAUDE.md §9.4 cấm là Instantiate/Destroy trong vòng lặp gameplay, và class này đạt đúng
       điều đó: sau vài viên đầu là tái dùng, không cấp phát thêm. Nếu sau này có nhiều loại đạn
       prefab riêng thì chuyển sang Pools của framework. */
    public class ProjectilePool
    {
        readonly List<BlastProjectile> all = new List<BlastProjectile>(8);
        readonly BlastProjectile prefab;
        readonly Transform container;

        public ProjectilePool(BlastProjectile prefab, Transform container)
        {
            this.prefab = prefab;
            this.container = container;
        }

        public BlastProjectile Get(Vector2 position)
        {
            for (var i = 0; i < all.Count; i++)
            {
                var candidate = all[i];
                if (candidate.gameObject.activeSelf) continue;

                candidate.transform.position = position;
                candidate.gameObject.SetActive(true);
                return candidate;
            }

            var created = Object.Instantiate(prefab, position, Quaternion.identity, container);
            all.Add(created);
            return created;
        }

        public void DeactivateAll()
        {
            for (var i = 0; i < all.Count; i++)
            {
                if (all[i]) all[i].gameObject.SetActive(false);
            }
        }
    }
}
