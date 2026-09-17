using System;
using Dacodelaac.Core;
using Dev.Scripts.BlastOut.Config;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Ngắm kiểu ná: chạm ở BẤT KỲ đâu rồi kéo ngược lại, thả ra là bắn.
       Cố ý không bắt người chơi phải chạm đúng vào khẩu pháo — ngón cái che mất pháo là chuyện
       thường ở portrait, và mọi game cùng thể loại đều cho kéo tự do. */
    public class AimController : BaseMono
    {
        [SerializeField] Transform muzzle;
        [SerializeField] Transform barrelPivot;
        [SerializeField] TrajectoryPreview preview;
        [SerializeField] BlastTuning tuning;

        [Tooltip("Thân và nòng pháo. Mờ đi khi chưa bắn được.")]
        [SerializeField] SpriteRenderer[] launcherParts;

        [Tooltip("Màu lúc chưa bắn được. Dòng chữ dưới đáy màn dễ bị bỏ qua vì mắt đang dán vào " +
                 "khẩu pháo và mục tiêu — làm mờ chính khẩu pháo thì không cần đọc cũng biết.")]
        [SerializeField] Color busyTint = new Color(1f, 1f, 1f, 0.4f);

        /* Bắn ra vận tốc ban đầu; ai nghe thì tự quyết làm gì với nó. */
        public event Action<Vector2> Fired;

        Camera view;
        Vector2 dragStart;
        bool dragging;

        /* Khởi tạo là false để lần gọi đầu luôn ghi màu, bất kể pháo bắt đầu ở trạng thái nào. */
        bool ready;

        public Vector2 MuzzlePosition => muzzle.position;

        public void Bind(Camera camera)
        {
            view = camera;
            CancelDrag();
        }

        /* Gọi từ BlastGameController mỗi frame. canAim = false lúc đạn đang bay hoặc level đã kết thúc. */
        public void HandleInput(bool canAim)
        {
            SetReady(canAim);

            if (!canAim)
            {
                if (dragging) CancelDrag();
                return;
            }

            if (Input.GetMouseButtonDown(0))
            {
                dragStart = ScreenToWorld(Input.mousePosition);
                dragging = true;
                return;
            }

            if (!dragging) return;

            if (Input.GetMouseButton(0))
            {
                UpdateAim(ScreenToWorld(Input.mousePosition));
                return;
            }

            if (Input.GetMouseButtonUp(0)) Release(ScreenToWorld(Input.mousePosition));
        }

        void UpdateAim(Vector2 current)
        {
            if (!TryBuildShot(current, out var velocity))
            {
                preview.Hide();
                return;
            }

            AimBarrel(velocity);
            preview.Show(MuzzlePosition, velocity);
        }

        void Release(Vector2 current)
        {
            dragging = false;
            preview.Hide();

            /* Kéo quá ngắn = chạm nhầm, không phải ý định bắn. Nuốt luôn, không tiêu đạn. */
            if (!TryBuildShot(current, out var velocity)) return;

            Fired?.Invoke(velocity);
        }

        bool TryBuildShot(Vector2 current, out Vector2 velocity)
        {
            /* Ngược hướng kéo, đúng như kéo dây ná: kéo xuống trái thì bắn lên phải. */
            var pull = dragStart - current;
            var distance = pull.magnitude;

            if (distance < tuning.MinDragDistance)
            {
                velocity = Vector2.zero;
                return false;
            }

            var clamped = Mathf.Min(distance, tuning.MaxDragDistance);
            velocity = pull / distance * tuning.SpeedFromDrag(clamped);
            return true;
        }

        void AimBarrel(Vector2 velocity)
        {
            if (!barrelPivot) return;
            var angle = Mathf.Atan2(velocity.y, velocity.x) * Mathf.Rad2Deg;
            barrelPivot.rotation = Quaternion.Euler(0f, 0f, angle);
        }

        void CancelDrag()
        {
            dragging = false;
            preview.Hide();
        }

        /* Chỉ ghi khi trạng thái ĐỔI: gán màu mỗi frame cho từng renderer là việc thừa, và
           SpriteRenderer.color mỗi lần gán là một lần chạm xuống native. */
        void SetReady(bool value)
        {
            if (ready == value) return;
            ready = value;

            if (launcherParts == null) return;

            var tint = value ? Color.white : busyTint;
            for (var i = 0; i < launcherParts.Length; i++)
            {
                if (launcherParts[i]) launcherParts[i].color = tint;
            }
        }

        Vector2 ScreenToWorld(Vector3 screenPosition)
        {
            /* view được cache lúc Bind, không gọi Camera.main mỗi frame (CLAUDE.md §9.4). */
            return view.ScreenToWorldPoint(screenPosition);
        }
    }
}
