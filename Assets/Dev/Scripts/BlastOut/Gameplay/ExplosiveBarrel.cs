using System.Collections;
using Dacodelaac.Core;
using Dev.Scripts.BlastOut.Config;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Thùng nổ: nhận sóng nổ rồi tự nổ tiếp. Đây là mechanic quyết định số 2 — người chơi phải
       chọn BẮN CÁI GÌ TRƯỚC chứ không chỉ bắn vào đâu.

       Có độ trễ trước khi nổ lây, vì dây chuyền nổ tức thì trông như một vụ nổ to và người chơi
       không đọc được là mình vừa kích hoạt chuỗi. */
    [RequireComponent(typeof(Rigidbody2D))]
    public class ExplosiveBarrel : BaseMono, IBlastable
    {
        [SerializeField] Rigidbody2D body;
        [SerializeField] BlastTuning tuning;
        [SerializeField] SpriteRenderer view;
        [SerializeField] Color armedColor = new Color(1f, 0.95f, 0.65f);

        BlastResolver resolver;
        bool triggered;

        public Rigidbody2D Body => body;

        public void Bind(BlastResolver blastResolver)
        {
            resolver = blastResolver;
            triggered = false;
        }

        public void ApplyBlast(Vector2 origin, float force, float radius)
        {
            /* Cờ này là thứ duy nhất chặn đệ quy vô hạn khi hai thùng nằm trong bán kính của nhau. */
            if (triggered) return;

            var distance = Vector2.Distance(body.position, origin);
            if (distance > radius) return;

            triggered = true;
            StartCoroutine(DetonateAfterDelay());
        }

        IEnumerator DetonateAfterDelay()
        {
            if (view) view.color = armedColor;

            yield return new WaitForSeconds(tuning.BarrelChainDelay);

            var origin = body.position;
            gameObject.SetActive(false);

            /* Nổ SAU khi tự tắt: nếu còn bật, chính nó cũng nằm trong vùng quét và bị đẩy vô ích. */
            resolver?.Blast(origin, tuning.BarrelForce, tuning.BarrelRadius);
        }
    }
}
