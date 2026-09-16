#if UNITY_EDITOR
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Dacodelaac.EditorUtils.Atlas
{
    /// <summary>Quét project tìm lỗi gom atlas. Dùng trước mỗi lần build.</summary>
    public class SpriteAtlasAudit
    {
        public class Entry
        {
            public string path { get; set; }
            public string note { get; set; }
        }

        /// <summary>Sprite chưa nằm trong atlas nào — mỗi cái là 1 draw call riêng.</summary>
        public readonly List<Entry> Orphans = new List<Entry>();

        /// <summary>Sprite nằm trong ≥2 atlas — bị nhân bản, tốn RAM và dung lượng build.</summary>
        public readonly List<Entry> Duplicates = new List<Entry>();

        /// <summary>Sprite quá lớn — nhét vào atlas chỉ làm atlas phình, nên để riêng.</summary>
        public readonly List<Entry> Oversized = new List<Entry>();

        /// <summary>Tổng quan từng atlas đang có trong project.</summary>
        public readonly List<Entry> Atlases = new List<Entry>();

        public bool HasIssue => Orphans.Count > 0 || Duplicates.Count > 0 || Oversized.Count > 0;

        public static SpriteAtlasAudit Run(string scopeFolder, int oversizeThreshold)
        {
            var result = new SpriteAtlasAudit();
            if (string.IsNullOrWhiteSpace(scopeFolder) || !AssetDatabase.IsValidFolder(scopeFolder))
            {
                Debug.LogWarning($"[SpriteAtlas] Phạm vi quét không hợp lệ: {scopeFolder}");
                return result;
            }

            // spritePath -> danh sách atlas đang chứa nó
            var ownedBy = new Dictionary<string, List<string>>();

            foreach (var guid in AssetDatabase.FindAssets("t:SpriteAtlas"))
            {
                var atlasPath = AssetDatabase.GUIDToAssetPath(guid);
                var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(atlasPath);
                if (atlas == null) continue;

                var atlasName = Path.GetFileNameWithoutExtension(atlasPath);
                var members = ExpandPackables(atlas);

                result.Atlases.Add(new Entry
                {
                    path = atlasPath,
                    note = $"{members.Count} sprite · includeInBuild={atlas.IsIncludeInBuild()}",
                });

                foreach (var member in members)
                {
                    if (!ownedBy.TryGetValue(member, out var list))
                        ownedBy[member] = list = new List<string>();
                    if (!list.Contains(atlasName)) list.Add(atlasName);
                }
            }

            foreach (var kv in ownedBy.Where(kv => kv.Value.Count > 1))
            {
                result.Duplicates.Add(new Entry
                {
                    path = kv.Key,
                    note = "nằm trong: " + string.Join(", ", kv.Value),
                });
            }

            foreach (var guid in AssetDatabase.FindAssets("t:Sprite", new[] { scopeFolder }))
            {
                var spritePath = AssetDatabase.GUIDToAssetPath(guid);

                if (!ownedBy.ContainsKey(spritePath))
                    result.Orphans.Add(new Entry { path = spritePath, note = "chưa thuộc atlas nào" });

                var tex = AssetDatabase.LoadAssetAtPath<Texture2D>(spritePath);
                if (tex != null && (tex.width > oversizeThreshold || tex.height > oversizeThreshold))
                {
                    result.Oversized.Add(new Entry
                    {
                        path = spritePath,
                        note = $"{tex.width}x{tex.height} — nên để NGOÀI atlas",
                    });
                }
            }

            return result;
        }

        /// <summary>Packable có thể là folder — bung ra thành danh sách sprite thật.</summary>
        static List<string> ExpandPackables(SpriteAtlas atlas)
        {
            var paths = new List<string>();
            var packables = atlas.GetPackables();
            if (packables == null) return paths;

            foreach (var obj in packables)
            {
                if (obj == null) continue;
                var path = AssetDatabase.GetAssetPath(obj);
                if (string.IsNullOrEmpty(path)) continue;

                if (AssetDatabase.IsValidFolder(path))
                {
                    paths.AddRange(AssetDatabase
                        .FindAssets("t:Sprite", new[] { path })
                        .Select(AssetDatabase.GUIDToAssetPath));
                }
                else
                {
                    paths.Add(path);
                }
            }

            return paths.Distinct().ToList();
        }

        public string Summary() =>
            $"{Atlases.Count} atlas · {Orphans.Count} sprite chưa gom · " +
            $"{Duplicates.Count} bị trùng · {Oversized.Count} quá lớn";
    }
}
#endif
