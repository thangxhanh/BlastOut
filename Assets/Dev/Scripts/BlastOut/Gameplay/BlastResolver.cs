using System;
using System.Collections.Generic;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Một vụ nổ = quét hình tròn rồi báo cho mọi IBlastable trong đó.
       Class thường (không MonoBehaviour) vì nó không cần vị trí, không cần vòng đời — chỉ cần
       giữ buffer để không cấp phát mỗi lần nổ. */
    public class BlastResolver
    {
        /* List này được Physics2D ghi đè mỗi lần quét. Giữ lại giữa các lần gọi để không sinh rác GC. */
        readonly List<Collider2D> hits = new List<Collider2D>(32);
        ContactFilter2D filter;

        public BlastResolver()
        {
            /* Cố ý KHÔNG lọc theo layer: lọc bằng TryGetComponent<IBlastable> vừa đủ nhanh ở quy mô
               này (vài chục collider/level) và bỏ được cả một nhóm bug "không có gì xảy ra vì
               quên set layer". useTriggers = false để vùng thu không bị tính là vật thể chịu lực. */
            filter = new ContactFilter2D { useTriggers = false };
            filter.NoFilter();
            filter.useTriggers = false;
        }

        /* Mọi vụ nổ trong game đều đi qua Blast(), kể cả nổ dây chuyền từ thùng — nên chỉ cần nghe
           ở đây là VFX tự khớp với mọi nguồn nổ, không phải rải lời gọi vào đạn lẫn thùng. */
        public event Action<Vector2, float> Blasted;

        public void Blast(Vector2 origin, float force, float radius)
        {
            Blasted?.Invoke(origin, radius);
            hits.Clear();
            Physics2D.OverlapCircle(origin, radius, filter, hits);

            for (var i = 0; i < hits.Count; i++)
            {
                if (hits[i].TryGetComponent<IBlastable>(out var blastable))
                {
                    blastable.ApplyBlast(origin, force, radius);
                }
            }
        }
    }
}
