using System;
using Dacodelaac.Core;
using Dev.Scripts.BlastOut.Core;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Config
{
    /* Một level = một asset dữ liệu. Thêm level mới không phải viết thêm một dòng gameplay nào,
       cũng không phải tạo scene mới — đúng yêu cầu "level chỉnh sửa/tạo thêm không viết lại core". */
    [CreateAssetMenu(menuName = "BlastOut/Level", fileName = "level_01")]
    public class BlastLevelConfig : BaseSO
    {
        [Serializable]
        public struct PlatformPlacement
        {
            [Tooltip("Tâm của bệ, toạ độ thế giới. Với bệ chuyển động, đây là tâm của quỹ đạo.")]
            public Vector2 Center;
            public float Width;
            [Tooltip("Để Static nếu bệ đứng yên.")]
            public PlatformMotion Motion;
        }

        [Serializable]
        public struct BlockPlacement
        {
            public Vector2 Position;
        }

        [Serializable]
        public struct BarrelPlacement
        {
            public Vector2 Position;
        }

        [Header("Hiển thị")]
        [SerializeField] int displayNumber = 1;
        [Tooltip("Dòng gợi ý hiện ở đáy màn. Để trống thì không hiện.")]
        [SerializeField] string hint = "DRAG TO AIM";

        [Header("Bố cục")]
        [SerializeField] Vector2 launcherPosition = new Vector2(-4.2f, -6.4f);

        [Tooltip("Chỉ số bệ mà khẩu pháo đứng lên (-1 = đứng yên trên nền). Pháo bám theo bệ đó, " +
                 "nên nếu bệ chạy thì cả điểm bắn cũng chạy.")]
        [SerializeField] int launcherPlatformIndex = -1;
        [Tooltip("Mọi khối rơi xuống dưới mức y này được tính là đã thu.")]
        [SerializeField] float collectZoneTopY = -8f;

        [Tooltip("Thứ tự trong mảng = thứ tự nạp đạn mặc định.")]
        [SerializeField] AmmoType[] ammo = { AmmoType.Bomb, AmmoType.Bomb };

        [SerializeField] PlatformPlacement[] platforms;
        [SerializeField] BlockPlacement[] blocks;
        [SerializeField] BarrelPlacement[] barrels;

        public int DisplayNumber => displayNumber;
        public string Hint => hint;
        public Vector2 LauncherPosition => launcherPosition;
        public int LauncherPlatformIndex => launcherPlatformIndex;
        public float CollectZoneTopY => collectZoneTopY;
        public AmmoType[] Ammo => ammo;
        public PlatformPlacement[] Platforms => platforms;
        public BlockPlacement[] Blocks => blocks;
        public BarrelPlacement[] Barrels => barrels;

        public int AmmoCount => ammo?.Length ?? 0;
        public int TargetCount => blocks?.Length ?? 0;

        /* Gọi từ builder trước khi dựng: bắt lỗi dữ liệu ngay lúc mở level thay vì để người chơi
           gặp một màn không thể thắng. */
        public bool IsValid(out string error)
        {
            if (TargetCount == 0)
            {
                error = $"{name}: level không có khối mục tiêu nào.";
                return false;
            }

            if (AmmoCount == 0)
            {
                error = $"{name}: level không có viên đạn nào.";
                return false;
            }

            error = null;
            return true;
        }
    }
}
