#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using Dacodelaac.DataStorage;
using Dacodelaac.DebugUtils;
using Dacodelaac.Utils;

namespace Dacodelaac.EditorUtils
{
    [InitializeOnLoad]
    public class EditorMenu
    {
        [MenuItem("Game/Enable Run in Background", false, 22)]
        public static void EnableRunInBG()
        {
            Application.runInBackground = true;
        }

        [MenuItem("Game/Enable Run in Background", true, 22)]
        public static bool EnableRunInBGConditional()
        {
            return !Application.runInBackground;
        }

        [MenuItem("Game/Disable Run in Background", false, 22)]
        public static void DisableRunInBG()
        {
            Application.runInBackground = false;
        }

        [MenuItem("Game/Disable Run in Background", true, 22)]
        public static bool DisableRunInBGConditional()
        {
            return Application.runInBackground;
        }

        [MenuItem("Game/Clear Data", false, 44)]
        public static void ClearData()
        {
            PlayerPrefs.DeleteAll();
            var persistentDataPath = DataStorage.DataStorage.GetPersistentDataPath();
            if (Directory.Exists(persistentDataPath))
            {
                Dacoder.LogFormat("Deleted data directory {0}", persistentDataPath);
                Directory.Delete(persistentDataPath, true);
            }

            var remoteConfigPath =
                Path.Combine(Directory.GetParent(Application.dataPath).FullName, "remote_config_data");
            if (File.Exists(remoteConfigPath))
            {
                Dacoder.LogFormat("Deleted remote config {0}", remoteConfigPath);
                File.Delete(remoteConfigPath);
            }

            GameData.Clear();
        }

        [MenuItem("Game/Pause", false, 55)]
        public static void PauseGame()
        {
            Time.timeScale = 0;
        }

        [MenuItem("Game/Pause", true, 55)]
        public static bool PauseGameCondition()
        {
            return Time.timeScale > 0;
        }

        [MenuItem("Game/Resume", false, 55)]
        public static void ResumeGame()
        {
            Time.timeScale = 1;
        }

        [MenuItem("Game/Resume", true, 55)]
        public static bool ResumeGameCondition()
        {
            return Time.timeScale < 1;
        }

        [MenuItem("Tools/Add CustomViewSize")]
        public static void AddCustomViewSize()
        {
            GameViewUtils.AddCustomSize();
        }

        #region FLAGS

        /* REMOTE_CONFIG và NOTIFICATION đã chuyển thành checkbox trong Inspector
           (RemoteConfig.useRemoteConfig, NotificationService.enableNotifications) — bật tắt không
           cần recompile, và code luôn được biên dịch nên lỗi lộ ra ngay. */

        /* Chỉ còn DEBUG_LOG_ON là cờ biên dịch thật. Nó BẮT BUỘC phải là cờ, không thay bằng
           biến bool được: Dacoder gắn [Conditional] nên cả lời gọi lẫn việc dựng chuỗi $"..."
           đều bị xoá ở call site. Một biến bool thì chuỗi vẫn được dựng rồi mới bỏ đi.

           Các cờ ADMOB / MAX / IRON_SOURCE / FIREBASE / ADJUST / FACEBOOK / GA / DACODER_RELEASE
           đã bỏ: không dòng code nào đọc chúng, bật tắt chỉ thêm define rỗng vào Player Settings
           và tạo cảm giác sai rằng đang tắt được SDK. */

        [MenuItem("Flags/DEBUG_LOG_ON")]
        public static void DebugLogFlag()
        {
            SwitchFlag("DEBUG_LOG_ON");
        }

        [MenuItem("Flags/DEBUG_LOG_ON", true)]
        public static bool IsDebugLogEnabled()
        {
            Menu.SetChecked("Flags/DEBUG_LOG_ON", IsFlagEnabled("DEBUG_LOG_ON"));
            return true;
        }

        static void SwitchFlag(string flag)
        {
            PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                out var defines);
            var enabled = defines.Contains(flag);
            defines = enabled ? defines.Where(value => value != flag).ToArray() : defines.Append(flag).ToArray();
            PlayerSettings.SetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup, defines);
        }

        static bool IsFlagEnabled(string flag)
        {
            PlayerSettings.GetScriptingDefineSymbolsForGroup(EditorUserBuildSettings.selectedBuildTargetGroup,
                out var defines);
            return defines.Contains(flag);
        }

        #endregion
    }
}
#endif