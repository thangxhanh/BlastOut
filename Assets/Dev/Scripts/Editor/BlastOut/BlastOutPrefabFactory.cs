using Dacodelaac.Core;
using Dacodelaac.ObjectPooling;
using Dev.Scripts.BlastOut.Config;
using Dev.Scripts.BlastOut.Gameplay;
using UnityEditor;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Authoring
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
        public static void BindBase(BaseMono mono, bool tick = false)
        {
            new SerializedFieldWriter(mono)
                .Ref("pools", Pools)
                .Ref("ticker", Ticker)
                .Bool("earlyTick", false)
                .Bool("tick", tick)
                .Bool("lateTick", false)
                .Bool("fixedTick", false)
                .Apply();
        }

        public static Transform CreatePlatform(Sprite square)
        {
            var go = NewSprite("platform", square, BlastOutAssetFactory.PlatformColor, 0);
            var box = go.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;

            /* Không có Rigidbody2D ⇒ collider tĩnh. Bệ không bị sóng nổ đẩy, cũng không
               implement IBlastable — nên BlastResolver tự bỏ qua nó. */
            return SavePrefab(go).transform;
        }

        public static TargetBlock CreateBlock(Sprite square, BlastTuning tuning)
        {
            var go = NewSprite("target_block", square, BlastOutAssetFactory.BlockColor, 2);
            go.transform.localScale = new Vector3(0.84f, 0.84f, 1f);

            var box = go.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;

            var body = AddBody(go, 1f);
            var block = go.AddComponent<TargetBlock>();

            new SerializedFieldWriter(block).Ref("body", body).Ref("tuning", tuning).Apply();
            BindBase(block);

            return SavePrefab(go).GetComponent<TargetBlock>();
        }

        public static ExplosiveBarrel CreateBarrel(Sprite square, BlastTuning tuning)
        {
            var go = NewSprite("explosive_barrel", square, BlastOutAssetFactory.BarrelColor, 2);
            go.transform.localScale = new Vector3(0.62f, 0.9f, 1f);

            var box = go.AddComponent<BoxCollider2D>();
            box.size = Vector2.one;

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
