using Dacodelaac.Core;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Config
{
    /* Toàn bộ số liệu vật lý của game nằm ở đây, không hardcode trong MonoBehaviour (CLAUDE.md §9.3).
       Chỉnh game feel = sửa asset này, không phải sửa code và build lại. */
    [CreateAssetMenu(menuName = "BlastOut/Tuning", fileName = "blast_tuning")]
    public class BlastTuning : BaseSO
    {
        [Header("Bắn")]
        [Tooltip("Kéo ngắn nhất tính bằng unit thế giới mới được coi là một cú kéo thật.")]
        [SerializeField] float minDragDistance = 0.35f;
        [Tooltip("Kéo dài hơn mức này thì lực bắn không tăng nữa — trần lực để người chơi đoán được.")]
        [SerializeField] float maxDragDistance = 3.5f;
        [SerializeField] float minLaunchSpeed = 7f;
        [SerializeField] float maxLaunchSpeed = 19f;
        [SerializeField] float projectileGravityScale = 2.2f;
        [Tooltip("Đạn tự biến mất sau ngần này giây nếu không chạm gì và người chơi không kích nổ.")]
        [SerializeField] float projectileLifetime = 6f;

        [Header("Vụ nổ")]
        [SerializeField] float blastRadius = 3f;
        [SerializeField] float blastForce = 15f;
        [Tooltip("Cộng thêm thành phần hướng lên để khối bị hất bổng thay vì trượt ngang — dễ đọc hơn nhiều.")]
        [SerializeField] float blastUpwardBias = 0.45f;
        [SerializeField] float blastTorque = 6f;

        [Header("Thùng nổ")]
        [SerializeField] float barrelRadius = 3.8f;
        [SerializeField] float barrelForce = 21f;
        [Tooltip("Độ trễ trước khi thùng nổ lây. Có độ trễ thì người chơi mới ĐỌC được dây chuyền.")]
        [SerializeField] float barrelChainDelay = 0.12f;

        [Header("Lắng vật lý")]
        [Tooltip("Tốc độ dưới ngưỡng này coi như đứng yên.")]
        [SerializeField] float settleSpeedThreshold = 0.25f;
        [Tooltip("Phải đứng yên liên tục ngần này giây mới kết luận thắng/thua.")]
        [SerializeField] float settleHoldTime = 0.4f;
        [Tooltip("Chốt chặn: dù vật lý chưa lắng cũng kết luận sau ngần này giây, tránh treo lượt.")]
        [SerializeField] float settleTimeout = 4f;

        [Header("Đường ngắm")]
        [SerializeField] int trajectoryPointCount = 14;
        [SerializeField] float trajectoryTimeStep = 0.07f;

        public float MinDragDistance => minDragDistance;
        public float MaxDragDistance => maxDragDistance;
        public float MinLaunchSpeed => minLaunchSpeed;
        public float MaxLaunchSpeed => maxLaunchSpeed;
        public float ProjectileGravityScale => projectileGravityScale;
        public float ProjectileLifetime => projectileLifetime;

        public float BlastRadius => blastRadius;
        public float BlastForce => blastForce;
        public float BlastUpwardBias => blastUpwardBias;
        public float BlastTorque => blastTorque;

        public float BarrelRadius => barrelRadius;
        public float BarrelForce => barrelForce;
        public float BarrelChainDelay => barrelChainDelay;

        public float SettleSpeedThreshold => settleSpeedThreshold;
        public float SettleHoldTime => settleHoldTime;
        public float SettleTimeout => settleTimeout;

        public int TrajectoryPointCount => trajectoryPointCount;
        public float TrajectoryTimeStep => trajectoryTimeStep;

        /* Gia tốc thật mà đạn chịu. Đường ngắm phải dùng đúng giá trị này thì mới khớp đường bay thật. */
        public Vector2 ProjectileGravity => Physics2D.gravity * projectileGravityScale;

        public float SpeedFromDrag(float dragDistance)
        {
            var t = Mathf.InverseLerp(minDragDistance, maxDragDistance, dragDistance);
            return Mathf.Lerp(minLaunchSpeed, maxLaunchSpeed, t);
        }
    }
}
