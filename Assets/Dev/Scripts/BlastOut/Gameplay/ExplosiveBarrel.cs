using System;
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

        [Tooltip("Thùng phình to thêm bao nhiêu phần trăm trong lúc đếm ngược.")]
        [SerializeField] float armedScaleGain = 0.35f;


        BlastResolver resolver;
        Action<Vector2> debrisRequested;
        bool triggered;

        public Rigidbody2D Body => body;

        public void Bind(BlastResolver blastResolver)
        {
            resolver = blastResolver;
            triggered = false;
        }

        /* Mảnh vỡ tách khỏi vụ nổ chung: chỉ thùng mới văng mảnh, còn đạn nổ giữa không trung thì
           không có gì để vỡ. Controller nối lại sau mỗi lần dựng level. */
        public void BindDebris(Action<Vector2> handler)
        {
            debrisRequested = handler;
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

        /* Quãng đếm ngược này là thứ cho người chơi ĐỌC được chuỗi nổ. Đổi màu một lần thì gần như
           không kịp thấy trong hơn một phần mười giây; phình to kèm nháy sáng thì thấy ngay cả khi
           mắt đang nhìn chỗ khác trên màn hình. */
        IEnumerator DetonateAfterDelay()
        {
            var duration = Mathf.Max(0.01f, tuning.BarrelChainDelay);
            var baseScale = transform.localScale;
            var baseColor = view ? view.color : Color.white;

            var elapsed = 0f;
            while (elapsed < duration)
            {
                elapsed += Time.deltaTime;
                var t = Mathf.Clamp01(elapsed / duration);

                /* Phình và sáng DẦN LÊN tới đỉnh rồi nổ, không nháy. Quãng đếm ngược chỉ 0.16s nên
                   mỗi lần sáng–tối chưa tới 3 frame ở 60fps — nháy trong khoảng đó ra nhiễu chứ
                   không ra tín hiệu. Một đường tăng đều thì đọc được ngay cả bằng mắt ngoại vi. */
                transform.localScale = baseScale * (1f + t * armedScaleGain);
                if (view) view.color = Color.Lerp(baseColor, armedColor, t);

                yield return null;
            }

            var origin = body.position;
            gameObject.SetActive(false);

            /* Nổ SAU khi tự tắt: nếu còn bật, chính nó cũng nằm trong vùng quét và bị đẩy vô ích. */
            debrisRequested?.Invoke(origin);
            resolver?.Blast(origin, tuning.BarrelForce, tuning.BarrelRadius);
        }
    }
}
