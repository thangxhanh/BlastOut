#if UNITY_IOS
using UnityEditor;
using UnityEditor.Callbacks;
using UnityEditor.iOS.Xcode;
using UnityEngine;

namespace Dacodelaac.Scripts.EditorUtils
{
    /* Hai thứ ATT cần mà Unity KHÔNG tự làm — thiếu cái nào cũng hỏng lặng lẽ:

       1. NSUserTrackingUsageDescription trong Info.plist. Thiếu chuỗi này, iOS KHÔNG hiện popup
          (và App Store từ chối bản build). Không có lỗi lúc build, chỉ là popup không bao giờ ra.

       2. Link AppTrackingTransparency.framework, dạng WEAK vì framework chỉ có từ iOS 14 —
          link cứng là app crash lúc khởi động trên iOS 13.

       Làm bằng post-process để không ai phải nhớ sửa tay sau mỗi lần export Xcode. */
    public static class AttPostProcessBuild
    {
        /* Chuỗi hiện trong popup ATT. Sửa cho khớp giọng của từng game. */
        private const string TrackingUsageDescription =
            "Chúng tôi dùng dữ liệu này để hiển thị quảng cáo phù hợp hơn với bạn.";

        [PostProcessBuild(100)]
        public static void OnPostProcessBuild(BuildTarget target, string pathToBuiltProject)
        {
            if (target != BuildTarget.iOS) return;

            AddUsageDescription(pathToBuiltProject);
            WeakLinkFramework(pathToBuiltProject);
        }

        private static void AddUsageDescription(string pathToBuiltProject)
        {
            var plistPath = pathToBuiltProject + "/Info.plist";

            var plist = new PlistDocument();
            plist.ReadFromFile(plistPath);
            plist.root.SetString("NSUserTrackingUsageDescription", TrackingUsageDescription);
            plist.WriteToFile(plistPath);

            Debug.Log("[ATT] Đã thêm NSUserTrackingUsageDescription vào Info.plist");
        }

        private static void WeakLinkFramework(string pathToBuiltProject)
        {
            var projectPath = PBXProject.GetPBXProjectPath(pathToBuiltProject);

            var project = new PBXProject();
            project.ReadFromFile(projectPath);

            var targetGuid = project.GetUnityFrameworkTargetGuid();
            project.AddFrameworkToProject(targetGuid, "AppTrackingTransparency.framework", true); // true = weak

            project.WriteToFile(projectPath);

            Debug.Log("[ATT] Đã weak-link AppTrackingTransparency.framework");
        }
    }
}
#endif
