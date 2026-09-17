using System;

namespace Dev.Scripts.BlastOut.Core
{
    /* Model thuần C#: giữ luật thắng/thua và số đạn còn lại.
       Không biết Unity, không biết prefab, không biết popup — chỉ báo ra phase đã đổi.
       Tách ra để luật chơi test được mà không cần dựng scene. */
    public class BlastSession
    {
        public event Action<BlastPhase> PhaseChanged;
        public event Action<int> AmmoChanged;
        public event Action<int> TargetsChanged;

        public BlastPhase Phase { get; private set; }
        public int AmmoRemaining { get; private set; }
        public int TargetsRemaining { get; private set; }

        public bool IsOver => Phase == BlastPhase.Won || Phase == BlastPhase.Lost;
        public bool CanAim => Phase == BlastPhase.Aiming && AmmoRemaining > 0;

        public void Begin(int ammo, int targets)
        {
            AmmoRemaining = ammo;
            TargetsRemaining = targets;

            AmmoChanged?.Invoke(AmmoRemaining);
            TargetsChanged?.Invoke(TargetsRemaining);

            /* Level không có target nào là lỗi dữ liệu, không phải thắng ngay. Builder đã chặn. */
            SetPhase(BlastPhase.Aiming);
        }

        public bool TryFire()
        {
            if (!CanAim) return false;

            AmmoRemaining--;
            AmmoChanged?.Invoke(AmmoRemaining);
            SetPhase(BlastPhase.Flying);
            return true;
        }

        /* Đạn đã nổ hoặc đã biến mất — bắt đầu chờ vật lý lắng. */
        public void OnProjectileSpent()
        {
            if (Phase != BlastPhase.Flying) return;
            SetPhase(BlastPhase.Resolving);
        }

        /* Một khối đã rơi vào vùng thu. Có thể xảy ra bất cứ lúc nào, kể cả đạn còn đang bay. */
        public void OnTargetCollected()
        {
            if (IsOver) return;

            TargetsRemaining--;
            if (TargetsRemaining < 0) TargetsRemaining = 0;
            TargetsChanged?.Invoke(TargetsRemaining);

            if (TargetsRemaining == 0) SetPhase(BlastPhase.Won);
        }

        /* Vụ nổ chạm tới mục tiêu cấm. Thua NGAY, không chờ vật lý lắng như các trường hợp khác:
           ở đây không còn gì để chờ nữa — kết quả đã được định đoạt ngay khoảnh khắc nổ, và bắt
           người chơi ngồi nhìn thêm vài giây chỉ làm chậm lần thử lại. */
        public void OnForbiddenHit()
        {
            if (IsOver) return;
            SetPhase(BlastPhase.Lost);
        }

        /* Vật lý đã đứng yên (hoặc hết giờ chờ) — giờ mới kết luận được thắng/thua.
           Chờ đến lúc này thay vì kết luận ngay khi nổ: một khối còn đang lăn vẫn có thể rơi vào vùng thu. */
        public void OnPhysicsSettled()
        {
            if (Phase != BlastPhase.Resolving) return;

            if (TargetsRemaining <= 0) SetPhase(BlastPhase.Won);
            else if (AmmoRemaining <= 0) SetPhase(BlastPhase.Lost);
            else SetPhase(BlastPhase.Aiming);
        }

        void SetPhase(BlastPhase value)
        {
            if (Phase == value) return;
            Phase = value;
            PhaseChanged?.Invoke(value);
        }
    }
}
