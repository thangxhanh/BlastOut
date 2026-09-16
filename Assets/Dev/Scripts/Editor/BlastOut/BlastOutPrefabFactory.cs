using Dacodelaac.Core;
using Dacodelaac.ObjectPooling;
using Dev.Scripts.BlastOut.Config;
using Dev.Scripts.BlastOut.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Dev.Scripts.Editor.BlastOut
{
    /* Dựng prefab cho BlastOut. Mỗi prefab là một asset thật trên đĩa sau khi chạy — gameplay chỉ
       tham chiếu tới chúng qua [SerializeField], không sinh gì lúc chạy. */
    public static class BlastOutPrefabFactory
    {
        const string TickerPath = "Assets/Dacodelaac/Variables/ticker.asset";
        const string PoolsPath = "Assets/Dacodelaac/Variables/pools.asset";

        public static Ticker Ticker => AssetDatabase.LoadAssetAtPath<Ticker>(TickerPath);
        public static Pools Pools => AssetDatabase.LoadAssetAtPath<Pools>(PoolsPath);

        /* BaseMono giữ pools/ticker là field private. Bình thường Reset() của nó tự gán khi kéo
           component vào Inspector, nhưng object dựng bằng code không đi qua Reset() — nên gán tay. */
        public static void BindBase(BaseMono mono, bool tick = false, bool fixedTick = false)
        {
            new SerializedFieldWriter(mono)
                .Ref("pools", Pools)
                .Ref("ticker", Ticker)
                .Bool("earlyTick", false)
                .Bool("tick", tick)
                .Bool("lateTick", false)
                .Bool("fixedTick", fixedTick)
                .Apply();
        }

        /* Kích thước sprite theo world unit. Dùng để collider bám đúng hình nhìn thấy thay vì
           giả định sprite luôn vuông 1×1 — art thật gần như không bao giờ vuông. */
        public static Vector2 SpriteSize(Sprite sprite)
        {
            return sprite ? sprite.rect.size / sprite.pixelsPerUnit : Vector2.one;
        }

        public static Transform CreatePlatform(Sprite sprite)
        {
            var go = NewSprite("platform", sprite, BlastOutAssetFactory.PlatformColor, 0);
            var box = go.AddComponent<BoxCollider2D>();
            box.size = SpriteSize(sprite);
            box.sharedMaterial = GripMaterial();

            /* Không có Rigidbody2D ⇒ collider tĩnh. Bệ không bị sóng nổ đẩy, cũng không
               implement IBlastable — nên BlastResolver tự bỏ qua nó. */
            return SavePrefab(go).transform;
        }

        /* Bệ chạy: cùng hình dáng bệ thường nhưng có Rigidbody2D Kinematic, nhờ đó nó đẩy được vật
           đặt trên mà không bị vụ nổ thổi bay khỏi quỹ đạo. */
        public static MovingPlatform CreateMovingPlatform(Sprite sprite)
        {
            var go = NewSprite("platform_moving", sprite, BlastOutAssetFactory.PlatformColor, 0);

            var box = go.AddComponent<BoxCollider2D>();
            box.size = SpriteSize(sprite);
            box.sharedMaterial = GripMaterial();

            var body = go.AddComponent<Rigidbody2D>();
            body.bodyType = RigidbodyType2D.Kinematic;
            /* Interpolate: bệ chạy được đẩy trong bước vật lý, không nội suy thì rung ở 60fps. */
            body.interpolation = RigidbodyInterpolation2D.Interpolate;

            var platform = go.AddComponent<MovingPlatform>();
            new SerializedFieldWriter(platform).Ref("body", body).Apply();
            BindBase(platform);

            return SavePrefab(go).GetComponent<MovingPlatform>();
        }

        public static TargetBlock CreateBlock(Sprite sprite, BlastTuning tuning)
        {
            var go = NewSprite("target_block", sprite, BlastOutAssetFactory.BlockColor, 2);
            go.transform.localScale = new Vector3(0.84f, 0.84f, 1f);

            var box = go.AddComponent<BoxCollider2D>();
            box.size = SpriteSize(sprite);
            box.sharedMaterial = GripMaterial();

            var body = AddBody(go, 1f);
            var block = go.AddComponent<TargetBlock>();

            new SerializedFieldWriter(block).Ref("body", body).Ref("tuning", tuning).Apply();
            BindBase(block);

            return SavePrefab(go).GetComponent<TargetBlock>();
        }

        public static ExplosiveBarrel CreateBarrel(Sprite sprite, BlastTuning tuning)
        {
            var go = NewSprite("explosive_barrel", sprite, BlastOutAssetFactory.BarrelColor, 2);

            var box = go.AddComponent<BoxCollider2D>();
            /* Collider bám đúng kích thước sprite: thùng phuy không vuông, lấy Vector2.one thì
               vùng va chạm lệch hẳn so với hình nhìn thấy. */
            box.size = SpriteSize(sprite);
            box.sharedMaterial = GripMaterial();

            var body = AddBody(go, 0.8f);
            var barrel = go.AddComponent<ExplosiveBarrel>();

            new SerializedFieldWriter(barrel)
                .Ref("body", body)
                .Ref("tuning", tuning)
                .Ref("view", go.GetComponent<SpriteRenderer>())
                .Apply();
            BindBase(barrel);

            return SavePrefab(go).GetComponent<ExplosiveBarrel>();
        }

        public static BlastProjectile CreateProjectile(Sprite circle, BlastTuning tuning, Material trailMaterial)
        {
            var go = NewSprite("projectile", circle, BlastOutAssetFactory.ShellColor, 4);
            go.transform.localScale = new Vector3(0.36f, 0.36f, 1f);

            var collider = go.AddComponent<CircleCollider2D>();
            collider.radius = 0.5f;

            var body = AddBody(go, 0.6f);
            /* Đạn bay nhanh nhất trong scene — Continuous để không xuyên qua bệ mỏng giữa hai frame. */
            body.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            var trail = go.AddComponent<TrailRenderer>();
            trail.time = 0.22f;
            trail.startWidth = 0.2f;
            trail.endWidth = 0f;
            trail.sharedMaterial = trailMaterial;
            trail.numCapVertices = 4;
            trail.sortingOrder = 3;
            trail.startColor = BlastOutAssetFactory.DotColor;
            trail.endColor = new Color(BlastOutAssetFactory.DotColor.r, BlastOutAssetFactory.DotColor.g,
                BlastOutAssetFactory.DotColor.b, 0f);

            var projectile = go.AddComponent<BlastProjectile>();
            new SerializedFieldWriter(projectile)
                .Ref("body", body)
                .Ref("tuning", tuning)
                .Ref("trail", trail)
                .Apply();
            BindBase(projectile);

            return SavePrefab(go).GetComponent<BlastProjectile>();
        }

        public static SpriteRenderer CreateDot(Sprite circle)
        {
            var go = NewSprite("trajectory_dot", circle, BlastOutAssetFactory.DotColor, 3);
            return SavePrefab(go).GetComponent<SpriteRenderer>();
        }

        /* Ma sát cao cho mọi thứ đứng trên bệ. Với ma sát mặc định, bệ chạy trượt ngay dưới chân
           khối: khối tụt dần về một đầu bệ rồi rơi xuống vùng thu khi người chơi chưa bắn phát nào
           — level tự thắng. Bounciness 0 để khối không nảy lóc cóc sau mỗi vụ nổ. */
        public static PhysicsMaterial2D GripMaterial()
        {
            BlastOutAssetFactory.EnsureFolder(BlastOutAssetFactory.PrefabFolder);
            var path = $"{BlastOutAssetFactory.PrefabFolder}/grip.physicsMaterial2D";

            var existing = AssetDatabase.LoadAssetAtPath<PhysicsMaterial2D>(path);
            if (existing) return existing;

            var material = new PhysicsMaterial2D("grip") { friction = 1f, bounciness = 0f };
            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        public static Material CreateTrailMaterial()
        {
            BlastOutAssetFactory.EnsureFolder(BlastOutAssetFactory.PrefabFolder);
            var path = $"{BlastOutAssetFactory.PrefabFolder}/trail.mat";

            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing) return existing;

            /* Shader.Find bị cấm lúc RUNTIME (shader không được asset nào tham chiếu sẽ bị strip
               khỏi build). Ở đây thì ngược lại: material trở thành asset thật và prefab tham
               chiếu tới nó, nên shader chắc chắn vào được build. */
            var material = new Material(Shader.Find("Sprites/Default"));

            /* Đắp texture đốm mềm lên vệt đạn: Sprites/Default không có texture thì vệt là một dải
               đặc, viền cứng — nhìn như thanh nhựa kéo theo đạn chứ không phải luồng lửa. */
            var trailTexture = AssetDatabase.LoadAssetAtPath<Texture2D>(
                $"{BlastOutAssetFactory.KenneyFolder}/VFX/vfx_trail.png");
            if (trailTexture) material.mainTexture = trailTexture;

            AssetDatabase.CreateAsset(material, path);
            return material;
        }

        static Rigidbody2D AddBody(GameObject go, float mass)
        {
            var body = go.AddComponent<Rigidbody2D>();
            body.mass = mass;
            body.linearDamping = 0.05f;
            body.angularDamping = 0.6f;
            body.interpolation = RigidbodyInterpolation2D.Interpolate;
            return body;
        }

        static GameObject NewSprite(string name, Sprite sprite, Color color, int sortingOrder)
        {
            var go = new GameObject(name);
            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = sortingOrder;
            return go;
        }

        static GameObject SavePrefab(GameObject go)
        {
            BlastOutAssetFactory.EnsureFolder(BlastOutAssetFactory.PrefabFolder);
            var path = $"{BlastOutAssetFactory.PrefabFolder}/{go.name}.prefab";

            var prefab = PrefabUtility.SaveAsPrefabAsset(go, path);
            Object.DestroyImmediate(go);
            return prefab;
        }
    }
}
