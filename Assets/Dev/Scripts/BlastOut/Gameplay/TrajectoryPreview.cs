using Dacodelaac.Core;
using Dev.Scripts.BlastOut.Config;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Đường chấm dự đoán quỹ đạo.

       Tính bằng công thức đạn đạo p(t) = p0 + v0·t + ½·g·t², KHÔNG dùng PhysicsScene2D phụ.
       Lý do: trước va chạm đầu tiên quỹ đạo là parabol thuần, nên công thức cho kết quả ĐÚNG TUYỆT ĐỐI
       chứ không phải xấp xỉ — mà lại không phải nuôi một physics scene song song và giữ nó đồng bộ.
       Đường chấm cũng cố ý mờ dần về cuối: đủ để đọc hướng, không đủ để game tự giải hộ. */
    public class TrajectoryPreview : BaseMono
    {
        [SerializeField] SpriteRenderer dotPrefab;
        [SerializeField] BlastTuning tuning;
        [SerializeField] Color dotColor = new Color(0.56f, 0.86f, 0.92f);
        [SerializeField] float headScale = 0.22f;
        [SerializeField] float tailScale = 0.09f;

        SpriteRenderer[] dots;

        public override void Initialize()
        {
            base.Initialize();

            /* Idempotent: controller gọi Initialize sớm để chắc chắn có chấm trước cú kéo đầu tiên,
               và launcher của scene cũng gọi — không chặn thì bộ chấm bị tạo hai lần. */
            if (dots != null) return;

            /* Tạo một lần lúc vào level, không phải trong vòng lặp gameplay. */
            var count = Mathf.Max(2, tuning.TrajectoryPointCount);
            dots = new SpriteRenderer[count];

            for (var i = 0; i < count; i++)
            {
                var dot = Instantiate(dotPrefab, transform);
                dot.name = $"dot_{i:00}";

                var t = i / (float)(count - 1);
                var scale = Mathf.Lerp(headScale, tailScale, t);
                dot.transform.localScale = new Vector3(scale, scale, 1f);
                dot.color = new Color(dotColor.r, dotColor.g, dotColor.b, Mathf.Lerp(0.95f, 0.12f, t));

                dots[i] = dot;
            }

            Hide();
        }

        public void Show(Vector2 origin, Vector2 velocity)
        {
            if (dots == null) return;

            var gravity = tuning.ProjectileGravity;
            var step = tuning.TrajectoryTimeStep;

            for (var i = 0; i < dots.Length; i++)
            {
                var t = step * i;
                var point = origin + velocity * t + 0.5f * gravity * (t * t);

                var dot = dots[i];
                dot.transform.position = point;
                dot.enabled = true;
            }
        }

        public void Hide()
        {
            if (dots == null) return;
            for (var i = 0; i < dots.Length; i++) dots[i].enabled = false;
        }
    }
}
