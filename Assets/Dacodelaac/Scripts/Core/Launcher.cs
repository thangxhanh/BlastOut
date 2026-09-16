using Dacodelaac.Events;
using Dacodelaac.Utils;
using Dacodelaac.Variables;
//using TokTok.InAppPurchasing;
using UnityEngine;
using Dacodelaac.DataStorage;

namespace Dacodelaac.Core
{
    public class Launcher : BaseLauncher
    {
        [SerializeField] LoadingScreenEvent loadingScreenEvent;

        /* Hệ thống dạng ScriptableObject (SoundManager, LevelManager...) — vòng đời của chúng là
           CẢ PHIÊN CHƠI, nên khởi tạo ở đây chứ không ở BaseLauncher: Launcher chỉ có trong
           LauncherScene nên chạy đúng MỘT lần, còn GameLauncher thì mỗi scene một lần.
           Khai báo kiểu BaseSO nên framework không cần biết type cụ thể của từng game. */
        [SerializeField] private BaseSO[] systems;

        [Tooltip("Khung hình mục tiêu. 60 là mức hợp lý cho game puzzle — cao hơn hầu như không " +
                 "ai thấy khác, mà máy nóng và tụt pin nhanh hơn hẳn.")]
        [SerializeField] private int targetFrameRate = 60;

        private void Start()
        {
            Initialize();
        }

        public override void Initialize()
        {
            /* Init TRƯỚC base.Initialize(), vì base sẽ spawn prefab và mono trong đó
               có thể gọi ngay tới các hệ thống này. */
            foreach (var so in systems)
            {
                if (so != null) so.Initialize();
            }

            base.Initialize();
            /* vSync phải TẮT thì targetFrameRate mới có tác dụng — Unity bỏ qua targetFrameRate khi
               vSyncCount > 0. Đặt ở đây chứ không chỉ trong QualitySettings, vì mỗi mức quality giữ
               thiết lập riêng: đổi mức lúc chạy là mất khống chế và máy 120Hz sẽ render gấp đôi. */
            QualitySettings.vSyncCount = 0;
            Application.targetFrameRate = targetFrameRate;

            /*loadingScreenEvent.Raise(new LoadingScreenData
            {
                IsLaunching = true,
                Scene = "GameScene",
                MinLoadTime = 4,

                // Đang hiện popup xin quyền THÔNG BÁO thì GIỮ màn loading, đừng activate scene đầu —
                // nếu không, hết MinLoadTime là vào thẳng game sau lưng popup (RequestUserPermission
                // không chặn gì cả). NotificationService tự có timeout nên cờ chắc chắn nhả.
                LaunchCondition = () => !NotificationService.PermissionPending,
            });*/
        }
    }
}