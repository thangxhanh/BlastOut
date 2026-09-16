using System.IO;
using Dev.Scripts.BlastOut.Config;
using Dev.Scripts.BlastOut.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Dev.Scripts.Editor.BlastOut
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

        /* Vật thể dùng art Kenney: tô TRẮNG để sprite hiện đúng màu gốc, không bị nhuộm đè. */
        public static readonly Color BlockColor = Color.white;
        public static readonly Color BarrelColor = Color.white;
        public static readonly Color PlatformColor = Color.white;
        public static readonly Color ShellColor = new Color32(0xE4, 0xF4, 0xFA, 0xFF);
        public static readonly Color DotColor = new Color32(0x8F, 0xDC, 0xEA, 0xFF);
        public static readonly Color SkyColor = new Color32(0x0B, 0x12, 0x24, 0xFF);

        /* Art từ Kenney (CC0) nằm sẵn trên đĩa — khác với sprite hình học do tool tự sinh.
           Sprite đã có màu riêng nên vật thể dùng art được tô trắng để giữ nguyên hình gốc. */
        public const string KenneyFolder = "Assets/Dev/Sprites/Kenney";

        public static Sprite LoadKenney(string relativePath)
        {
            var path = $"{KenneyFolder}/{relativePath}";
            var sprite = AssetDatabase.LoadAssetAtPath<Sprite>(path);
            if (!sprite) Debug.LogError($"[BlastOut] Thiếu sprite: {path}");
            return sprite;
        }

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
                .Float("projectileLifetime", 3.5f)

                /* Bán kính tính theo khổ màn: thế giới chỉ rộng 9 unit, nên bán kính 3 nghĩa là
                   một vụ nổ phủ 2/3 màn — bắn đâu cũng trúng và cú ngắm mất hết ý nghĩa. */
                .Float("blastRadius", 1.9f)
                .Float("blastForce", 11f)
                .Float("blastUpwardBias", 0.45f)
                .Float("blastTorque", 5f)

                /* Thùng nổ chỉ cần NẰM TRONG bán kính là phát nổ, không phụ thuộc lực còn lại bao
                   nhiêu. Bán kính rộng vì thế biến mọi cú bắn trượt thành cú bắn trúng. */
                .Float("barrelRadius", 2.4f)
                .Float("barrelForce", 11f)
                .Float("barrelChainDelay", 0.12f)

                .Float("settleSpeedThreshold", 0.3f)
                .Float("settleHoldTime", 0.3f)
                .Float("settleTimeout", 2.5f)
                .Int("trajectoryPointCount", 14)
                .Float("trajectoryTimeStep", 0.07f)
                .Apply();

            EditorUtility.SetDirty(tuning);
            return tuning;
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
