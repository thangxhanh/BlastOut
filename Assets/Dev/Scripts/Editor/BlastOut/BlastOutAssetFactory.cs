using System.IO;
using Dev.Scripts.BlastOut.Config;
using Dev.Scripts.BlastOut.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Authoring
{
    /* Sinh sprite, config và prefab cho BlastOut.

       Đây là tool dựng scaffold chạy MỘT LẦN trong Editor, không phải code runtime — nên nó được
       phép tạo asset, khác hẳn với việc sinh asset lúc chạy mà CLAUDE.md §9.3 cấm. Sau khi chạy
       xong, mọi thứ đều là asset thật trên đĩa, gameplay chỉ tham chiếu qua [SerializeField]. */
    public static class BlastOutAssetFactory
    {
        public const string SpriteFolder = "Assets/Dev/Sprites/BlastOut";
        public const string DataFolder = "Assets/Dev/Data/BlastOut";
        public const string PrefabFolder = "Assets/Dev/Prefabs/BlastOut";

        public static readonly Color BlockColor = new Color32(0xE8, 0x50, 0x3A, 0xFF);
        public static readonly Color BarrelColor = new Color32(0xF0, 0xA6, 0x3C, 0xFF);
        public static readonly Color PlatformColor = new Color32(0x5B, 0x64, 0x79, 0xFF);
        public static readonly Color ShellColor = new Color32(0xE4, 0xF4, 0xFA, 0xFF);
        public static readonly Color DotColor = new Color32(0x8F, 0xDC, 0xEA, 0xFF);
        public static readonly Color SkyColor = new Color32(0x0B, 0x12, 0x24, 0xFF);

        public static void EnsureFolder(string path)
        {
            if (AssetDatabase.IsValidFolder(path)) return;

            var parent = Path.GetDirectoryName(path)?.Replace('\\', '/');
            if (!string.IsNullOrEmpty(parent)) EnsureFolder(parent);
            AssetDatabase.CreateFolder(parent, Path.GetFileName(path));
        }

        public static Sprite CreateSquareSprite(string name, int size)
        {
            return CreateSprite(name, size, (x, y) => true);
        }

        public static Sprite CreateCircleSprite(string name, int size)
        {
            var radius = size * 0.5f;
            return CreateSprite(name, size, (x, y) =>
            {
                var dx = x - radius + 0.5f;
                var dy = y - radius + 0.5f;
                return dx * dx + dy * dy <= radius * radius;
            });
        }

        /* Sprite trắng tinh, tô màu bằng SpriteRenderer.color — một texture dùng cho mọi vật thể,
           không phải vẽ art. Đúng tinh thần đề: "có thể dùng primitive". */
        static Sprite CreateSprite(string name, int size, System.Func<int, int, bool> inside)
        {
            EnsureFolder(SpriteFolder);
            var path = $"{SpriteFolder}/{name}.png";

            var texture = new Texture2D(size, size, TextureFormat.RGBA32, false);
            for (var y = 0; y < size; y++)
            {
                for (var x = 0; x < size; x++)
                {
                    texture.SetPixel(x, y, inside(x, y) ? Color.white : Color.clear);
                }
            }

            texture.Apply();
            File.WriteAllBytes(path, texture.EncodeToPNG());

            /* Texture tạo bằng code là UnityEngine.Object — mất reference KHÔNG đủ để GC thu hồi. */
            Object.DestroyImmediate(texture);

            AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceUpdate);

            var importer = (TextureImporter)AssetImporter.GetAtPath(path);
            importer.textureType = TextureImporterType.Sprite;
            importer.spritePixelsPerUnit = size;
            importer.mipmapEnabled = false;
            importer.alphaIsTransparency = true;
            importer.filterMode = FilterMode.Bilinear;

            /* FullRect: sprite bị scale nhiều, Tight mesh sẽ cắt mất viền. */
            var settings = new TextureImporterSettings();
            importer.ReadTextureSettings(settings);
            settings.spriteMeshType = SpriteMeshType.FullRect;
            settings.spriteExtrude = 0;
            importer.SetTextureSettings(settings);
            importer.SaveAndReimport();

            return AssetDatabase.LoadAssetAtPath<Sprite>(path);
        }

        public static BlastTuning CreateTuning()
        {
            EnsureFolder(DataFolder);
            var tuning = CreateOrLoad<BlastTuning>($"{DataFolder}/blast_tuning.asset");

            /* Số khởi điểm, cố ý để hơi "quá tay" một chút: dễ chỉnh xuống hơn là chỉnh lên khi
               ngồi cạnh máy thật. Đây là bộ số đầu tiên cần tinh chỉnh sau lần chơi thử đầu. */
            new SerializedFieldWriter(tuning)
                .Float("minDragDistance", 0.35f)
                .Float("maxDragDistance", 3.5f)
                .Float("minLaunchSpeed", 8f)
                .Float("maxLaunchSpeed", 20f)
                .Float("projectileGravityScale", 2.2f)
                .Float("projectileLifetime", 6f)
                .Float("blastRadius", 3f)
                .Float("blastForce", 12f)
                .Float("blastUpwardBias", 0.45f)
                .Float("blastTorque", 6f)
                .Float("barrelRadius", 3.8f)
                .Float("barrelForce", 16f)
                .Float("barrelChainDelay", 0.12f)
                .Float("settleSpeedThreshold", 0.25f)
                .Float("settleHoldTime", 0.4f)
                .Float("settleTimeout", 4f)
                .Int("trajectoryPointCount", 14)
                .Float("trajectoryTimeStep", 0.07f)
                .Apply();

            EditorUtility.SetDirty(tuning);
            return tuning;
        }

        /* Level 1 theo đúng mục tiêu thiết kế: dạy kéo–thả, KHÔNG THỂ THUA khi còn đạn.
           Một khối, một bệ hẹp, không thùng nổ, không vật cản — chỉ có "kéo, thả, chạm". */
        public static BlastLevelConfig CreateLevelOne()
        {
            EnsureFolder(DataFolder);
            var level = CreateOrLoad<BlastLevelConfig>($"{DataFolder}/level_01.asset");

            var writer = new SerializedFieldWriter(level);
            writer.Int("displayNumber", 1)
                .Text("hint", "DRAG TO AIM")
                .Vec2("launcherPosition", new Vector2(-3.4f, -5f))
                .Float("collectZoneTopY", -6.4f);

            var ammo = writer.ArrayOf("ammo", 2);
            if (ammo != null)
            {
                for (var i = 0; i < 2; i++) ammo.GetArrayElementAtIndex(i).enumValueIndex = 0;
            }

            var platforms = writer.ArrayOf("platforms", 1);
            if (platforms != null)
            {
                var platform = platforms.GetArrayElementAtIndex(0);
                platform.FindPropertyRelative("Center").vector2Value = new Vector2(1.4f, -1f);
                platform.FindPropertyRelative("Width").floatValue = 3.2f;
            }

            var blocks = writer.ArrayOf("blocks", 1);
            if (blocks != null)
            {
                blocks.GetArrayElementAtIndex(0)
                    .FindPropertyRelative("Position").vector2Value = new Vector2(1.4f, -0.42f);
            }

            writer.ArrayOf("barrels", 0);
            writer.Apply();

            EditorUtility.SetDirty(level);
            return level;
        }

        /* Gom các level đã tạo vào một BlastLevelSet — đây là "chỗ lưu level" mà controller đọc.
           Thêm level về sau chỉ cần dựng thêm config rồi kéo vào mảng này trong Inspector. */
        public static BlastLevelSet CreateLevelSet(params BlastLevelConfig[] levels)
        {
            EnsureFolder(DataFolder);
            var set = CreateOrLoad<BlastLevelSet>($"{DataFolder}/level_set.asset");

            var writer = new SerializedFieldWriter(set);
            var array = writer.ArrayOf("levels", levels.Length);
            if (array != null)
            {
                for (var i = 0; i < levels.Length; i++)
                {
                    array.GetArrayElementAtIndex(i).objectReferenceValue = levels[i];
                }
            }
            writer.Apply();

            EditorUtility.SetDirty(set);
            return set;
        }

        static T CreateOrLoad<T>(string path) where T : ScriptableObject
        {
            var existing = AssetDatabase.LoadAssetAtPath<T>(path);
            if (existing) return existing;

            var created = ScriptableObject.CreateInstance<T>();
            AssetDatabase.CreateAsset(created, path);
            return created;
        }
    }
}
