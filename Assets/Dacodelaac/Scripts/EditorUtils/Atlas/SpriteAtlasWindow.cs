#if UNITY_EDITOR
using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

namespace Dacodelaac.EditorUtils.Atlas
{
    /// <summary>
    /// Cửa sổ gom sprite thành atlas + soát lỗi gom.
    /// Tool chỉ làm phần cơ khí — QUYẾT ĐỊNH gom sprite nào với nhau vẫn là của người,
    /// vì phải biết màn hình nào hiện sprite nào.
    /// </summary>
    public class SpriteAtlasWindow : EditorWindow
    {
        SpriteAtlasGroupConfig config;
        UnityEditor.Editor configEditor;
        SpriteAtlasAudit audit;
        Vector2 scroll;
        int tab;

        static readonly string[] Tabs = { "Gom atlas", "Soát lỗi" };

        [MenuItem("Window/Sprite Atlas Tool")]
        public static void Open() => GetWindow<SpriteAtlasWindow>("Sprite Atlas");

        void OnEnable() => LoadOrCreateConfig();

        void OnDisable()
        {
            if (configEditor != null) DestroyImmediate(configEditor);
        }

        void LoadOrCreateConfig()
        {
            config = AssetDatabase.LoadAssetAtPath<SpriteAtlasGroupConfig>(SpriteAtlasGroupConfig.DefaultPath);
            if (config == null)
            {
                SpriteAtlasBuilder.EnsureFolder(System.IO.Path
                    .GetDirectoryName(SpriteAtlasGroupConfig.DefaultPath)?.Replace('\\', '/'));

                config = CreateInstance<SpriteAtlasGroupConfig>();
                config.Groups.Add(new SpriteAtlasGroupConfig.Group { AtlasName = "UI_Common" });
                config.Groups.Add(new SpriteAtlasGroupConfig.Group { AtlasName = "UI_Home" });
                config.Groups.Add(new SpriteAtlasGroupConfig.Group { AtlasName = "UI_Gameplay" });

                AssetDatabase.CreateAsset(config, SpriteAtlasGroupConfig.DefaultPath);
                AssetDatabase.SaveAssets();
            }

            if (configEditor != null) DestroyImmediate(configEditor);
            configEditor = UnityEditor.Editor.CreateEditor(config);
        }

        void OnGUI()
        {
            if (config == null) LoadOrCreateConfig();

            DrawPackerStatus();
            tab = GUILayout.Toolbar(tab, Tabs);
            EditorGUILayout.Space();

            scroll = EditorGUILayout.BeginScrollView(scroll);
            if (tab == 0) DrawGroupsTab(); else DrawAuditTab();
            EditorGUILayout.EndScrollView();
        }

        void DrawPackerStatus()
        {
            if (SpriteAtlasBuilder.PackerEnabled)
            {
                EditorGUILayout.HelpBox($"Sprite Packer: {SpriteAtlasBuilder.PackerModeName}", MessageType.None);
                return;
            }

            EditorGUILayout.HelpBox(
                "Sprite Packer đang TẮT — atlas có tạo cũng không được pack, tool này vô tác dụng.",
                MessageType.Error);

            if (GUILayout.Button("Bật Sprite Packer (Enabled for Builds)"))
                SpriteAtlasBuilder.EnablePacker();

            EditorGUILayout.Space();
        }

        void DrawGroupsTab()
        {
            EditorGUILayout.HelpBox(
                "Gom theo nhóm CÙNG xuất hiện trên màn hình (UI_Common / UI_Home / UI_Gameplay / mỗi theme một atlas).\n" +
                "KHÔNG gom cả game vào một atlas lớn — mở màn nào cũng nạp trọn khối đó vào RAM.",
                MessageType.Info);

            if (configEditor != null) configEditor.OnInspectorGUI();

            EditorGUILayout.Space();
            using (new EditorGUILayout.HorizontalScope())
            {
                if (GUILayout.Button("Tạo / Cập nhật tất cả", GUILayout.Height(28)))
                    SpriteAtlasBuilder.BuildAll(config);

                if (GUILayout.Button("Pack thử", GUILayout.Height(28)))
                    SpriteAtlasBuilder.PackPreview(config);
            }

            EditorGUILayout.Space();
            EditorGUILayout.LabelField("Tạo riêng từng nhóm", EditorStyles.boldLabel);
            foreach (var group in config.Groups)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    EditorGUILayout.LabelField(group.Describe());
                    if (GUILayout.Button("Tạo", GUILayout.Width(60)))
                    {
                        var atlas = SpriteAtlasBuilder.BuildOrUpdate(group, config.OutputFolder);
                        if (atlas != null) EditorGUIUtility.PingObject(atlas);
                    }
                }
            }
        }

        void DrawAuditTab()
        {
            if (GUILayout.Button("Quét", GUILayout.Height(28)))
                audit = SpriteAtlasAudit.Run(config.AuditScope, config.OversizeThreshold);

            if (audit == null)
            {
                EditorGUILayout.HelpBox($"Bấm Quét để soát phạm vi: {config.AuditScope}", MessageType.Info);
                return;
            }

            EditorGUILayout.HelpBox(audit.Summary(),
                audit.HasIssue ? MessageType.Warning : MessageType.Info);

            DrawSection("Sprite trùng atlas (tốn RAM gấp đôi)", audit.Duplicates);
            DrawSection("Sprite quá lớn (nên để ngoài atlas)", audit.Oversized);
            DrawSection("Sprite chưa gom vào atlas nào", audit.Orphans);
            DrawSection("Atlas hiện có", audit.Atlases);

            EditorGUILayout.Space();
            EditorGUILayout.HelpBox(
                "Atlas KHÔNG giảm draw call nếu sprite dùng material/shader khác nhau, hoặc nằm trên nhiều Canvas.\n" +
                "Đo bằng Frame Debugger trước và sau khi gom — đừng tin là gom xong ắt nhanh.",
                MessageType.Info);
        }

        static void DrawSection(string title, List<SpriteAtlasAudit.Entry> entries)
        {
            EditorGUILayout.Space();
            EditorGUILayout.LabelField($"{title} — {entries.Count}", EditorStyles.boldLabel);
            if (entries.Count == 0)
            {
                EditorGUILayout.LabelField("   (không có)", EditorStyles.miniLabel);
                return;
            }

            foreach (var entry in entries)
            {
                using (new EditorGUILayout.HorizontalScope())
                {
                    if (GUILayout.Button("→", GUILayout.Width(24)))
                    {
                        var obj = AssetDatabase.LoadAssetAtPath<Object>(entry.path);
                        if (obj != null) EditorGUIUtility.PingObject(obj);
                    }

                    EditorGUILayout.LabelField(
                        new GUIContent(System.IO.Path.GetFileName(entry.path), entry.path),
                        GUILayout.Width(200));
                    EditorGUILayout.LabelField(entry.note, EditorStyles.miniLabel);
                }
            }
        }
    }
}
#endif
