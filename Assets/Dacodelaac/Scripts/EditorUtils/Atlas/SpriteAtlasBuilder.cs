#if UNITY_EDITOR
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEditor.U2D;
using UnityEngine;
using UnityEngine.U2D;

namespace Dacodelaac.EditorUtils.Atlas
{
    /// <summary>Sinh / cập nhật file .spriteatlas từ <see cref="SpriteAtlasGroupConfig"/>.</summary>
    public static class SpriteAtlasBuilder
    {
        /// <summary>Sprite Packer tắt thì atlas có tạo cũng không được pack — vô nghĩa.</summary>
        public static bool PackerEnabled => EditorSettings.spritePackerMode != SpritePackerMode.Disabled;

        public static string PackerModeName => EditorSettings.spritePackerMode.ToString();

        public static void EnablePacker()
        {
            EditorSettings.spritePackerMode = SpritePackerMode.BuildTimeOnlyAtlas;
            Debug.Log("[SpriteAtlas] Sprite Packer -> BuildTimeOnlyAtlas (pack khi build, Editor giữ sprite rời cho dễ debug).");
        }

        public static SpriteAtlas BuildOrUpdate(SpriteAtlasGroupConfig.Group group, string outputFolder)
        {
            if (group == null || string.IsNullOrWhiteSpace(group.AtlasName)) return null;
            EnsureFolder(outputFolder);

            var path = $"{outputFolder.TrimEnd('/')}/{group.AtlasName}.spriteatlas";
            var atlas = AssetDatabase.LoadAssetAtPath<SpriteAtlas>(path);
            if (atlas == null)
            {
                atlas = new SpriteAtlas();
                AssetDatabase.CreateAsset(atlas, path);
            }

            atlas.SetIncludeInBuild(group.IncludeInBuild);

            atlas.SetPackingSettings(new SpriteAtlasPackingSettings
            {
                enableRotation = false,                    // xoay sprite làm sai layout UI
                enableTightPacking = group.TightPacking,
                padding = group.Padding,
                blockOffset = 1,
            });

            atlas.SetTextureSettings(new SpriteAtlasTextureSettings
            {
                readable = false,                          // readable = giữ thêm 1 bản trên RAM
                generateMipMaps = false,                   // UI không cần mipmap
                sRGB = true,
                filterMode = FilterMode.Bilinear,
            });

            foreach (var platform in new[] { "Android", "iPhone" })
            {
                atlas.SetPlatformSettings(new TextureImporterPlatformSettings
                {
                    name = platform,
                    overridden = true,
                    maxTextureSize = group.MaxTextureSize,
                    textureCompression = TextureImporterCompression.Compressed,
                    format = TextureImporterFormat.ASTC_6x6,
                });
            }

            // Thay toàn bộ packable thay vì cộng dồn — nếu không, mục đã xoá khỏi config vẫn nằm lại atlas.
            var old = atlas.GetPackables();
            if (old != null && old.Length > 0) atlas.Remove(old);

            var packables = group.Packables.Where(o => o != null).Distinct().ToArray();
            if (packables.Length > 0) atlas.Add(packables);

            EditorUtility.SetDirty(atlas);
            AssetDatabase.SaveAssets();
            return atlas;
        }

        public static int BuildAll(SpriteAtlasGroupConfig config)
        {
            if (config == null) return 0;
            var count = 0;
            foreach (var g in config.Groups)
                if (BuildOrUpdate(g, config.OutputFolder) != null) count++;

            AssetDatabase.Refresh();
            Debug.Log($"[SpriteAtlas] Đã tạo/cập nhật {count} atlas tại {config.OutputFolder}.");
            return count;
        }

        /// <summary>Pack thử ngay trong Editor để xem atlas có vừa maxTextureSize không.</summary>
        public static void PackPreview(SpriteAtlasGroupConfig config)
        {
            var atlases = config.Groups
                .Select(g => AssetDatabase.LoadAssetAtPath<SpriteAtlas>(
                    $"{config.OutputFolder.TrimEnd('/')}/{g.AtlasName}.spriteatlas"))
                .Where(a => a != null)
                .ToArray();

            if (atlases.Length == 0)
            {
                Debug.LogWarning("[SpriteAtlas] Chưa có atlas nào để pack. Bấm 'Tạo / Cập nhật tất cả' trước.");
                return;
            }

            SpriteAtlasUtility.PackAtlases(atlases, EditorUserBuildSettings.activeBuildTarget);
            Debug.Log($"[SpriteAtlas] Đã pack thử {atlases.Length} atlas cho {EditorUserBuildSettings.activeBuildTarget}.");
        }

        public static void EnsureFolder(string folder)
        {
            if (string.IsNullOrWhiteSpace(folder)) return;
            folder = folder.Replace('\\', '/').TrimEnd('/');
            if (AssetDatabase.IsValidFolder(folder)) return;

            var parent = Path.GetDirectoryName(folder)?.Replace('\\', '/');
            var name = Path.GetFileName(folder);
            if (string.IsNullOrEmpty(parent) || string.IsNullOrEmpty(name)) return;

            EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, name);
        }
    }
}
#endif
