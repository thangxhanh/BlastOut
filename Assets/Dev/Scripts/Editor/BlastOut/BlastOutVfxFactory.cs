using UnityEditor;
using UnityEngine;

namespace Dev.Scripts.Editor.BlastOut
{
    /* Dựng prefab hiệu ứng nổ. Bốn lớp chồng lên nhau, mỗi lớp trả lời một câu hỏi của người chơi:
         flash  — "nổ Ở ĐÂU": loé sáng đúng tâm, thấy ngay cả khi mắt đang nhìn chỗ khác;
         ring   — "nổ TO CỠ NÀO": vòng lan đúng bằng bán kính thật, nên tầm ảnh hưởng đọc được;
         spark  — "nổ MẠNH KHÔNG": tia bắn ra theo mọi hướng;
         smoke  — dư âm, để vụ nổ không biến mất đột ngột.

       Hệ hạt dựng bằng API module (main/emission/shape...), không qua SerializedFieldWriter —
       ParticleSystem không phơi các thông số này ra dạng field serialize thường. */
    public static class BlastOutVfxFactory
    {
        const string VfxFolder = "Assets/Dev/Sprites/Kenney/VFX";

        static readonly Color FlashColor = new Color32(0xFF, 0xF3, 0xC4, 0xFF);
        static readonly Color SparkColor = new Color32(0xFF, 0xC4, 0x5A, 0xFF);
        static readonly Color SmokeColor = new Color32(0x9A, 0xA4, 0xB8, 0xB0);
        static readonly Color RingColor = new Color32(0xFF, 0xE2, 0x8A, 0xFF);

        public static ParticleSystem CreateExplosion()
        {
            var root = new GameObject("explosion_vfx");

            /* Hệ gốc không tự phát gì: nó chỉ là chỗ neo để Play(true) chạy cả bốn lớp con cùng lúc. */
            var host = root.AddComponent<ParticleSystem>();
            var hostMain = host.main;
            hostMain.playOnAwake = false;
            hostMain.duration = 1f;
            hostMain.loop = false;
            var hostEmission = host.emission;
            hostEmission.enabled = false;

            BuildFlash(root.transform);
            BuildRing(root.transform);
            BuildSparks(root.transform);
            BuildSmoke(root.transform);

            return SavePrefab(root).GetComponent<ParticleSystem>();
        }

        /* Loé sáng: to rất nhanh rồi tắt trong ~0.15s. Ngắn có chủ đích — kéo dài thành ra chói. */
        static void BuildFlash(Transform parent)
        {
            var ps = NewLayer(parent, "Flash", "vfx_flash.png", FlashColor, sortingOrder: 12);

            var main = ps.main;
            main.duration = 0.3f;
            main.startLifetime = 0.16f;
            main.startSize = 3.2f;
            main.startSpeed = 0f;

            Burst(ps, 1);
            SizeCurve(ps, 0.35f, 1f);
            FadeOut(ps);
        }

        /* Vòng xung kích: bán kính cuối khớp bán kính nổ thật, nhờ prefab được scale theo radius. */
        static void BuildRing(Transform parent)
        {
            var ps = NewLayer(parent, "Ring", "vfx_ring.png", RingColor, sortingOrder: 11);

            var main = ps.main;
            main.duration = 0.4f;
            main.startLifetime = 0.32f;
            main.startSize = 0.6f;
            main.startSpeed = 0f;

            Burst(ps, 1);
            /* 0.6 × 6.3 ≈ 3.8 world unit đường kính = đúng đường kính của bán kính nổ 1.9 mà prefab
               được dựng theo. Vòng nhỏ hơn vùng sát thương thật thì người chơi học sai tầm nổ. */
            SizeCurve(ps, 0.2f, 6.3f);
            FadeOut(ps);
        }

        static void BuildSparks(Transform parent)
        {
            var ps = NewLayer(parent, "Sparks", "vfx_spark.png", SparkColor, sortingOrder: 12);

            var main = ps.main;
            main.duration = 0.5f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.25f, 0.5f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.3f, 0.7f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(4f, 9f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            /* Tia lửa chịu trọng lực nhẹ để rơi xuống thay vì bay thẳng mãi — đọc ra "vụn văng ra". */
            main.gravityModifier = 1.2f;

            Burst(ps, 14);
            Circle(ps);
            SizeCurve(ps, 1f, 0.2f);
            FadeOut(ps);
        }

        static void BuildSmoke(Transform parent)
        {
            var ps = NewLayer(parent, "Smoke", "vfx_smoke.png", SmokeColor, sortingOrder: 10);

            var main = ps.main;
            main.duration = 0.8f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.45f, 0.8f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.8f, 1.5f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(0.8f, 2.2f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            /* Khói nhẹ hơn không khí nên bốc lên, ngược hẳn với tia lửa. */
            main.gravityModifier = -0.15f;

            Burst(ps, 8);
            Circle(ps);
            SizeCurve(ps, 0.6f, 1.8f);
            FadeOut(ps);
        }

        /* Mảnh vỡ của thùng. Tách khỏi prefab nổ chung vì chỉ thùng mới có gì để vỡ — đạn nổ giữa
           không trung mà văng mảnh gỗ thì vô lý.

           Mảnh dùng sprite thật (không additive như lửa) và xoay trong lúc bay, nên đọc ra là "vật
           thể bị xé" chứ không phải một đốm sáng nữa. */
        public static ParticleSystem CreateDebris()
        {
            var root = new GameObject("debris_vfx");

            var ps = root.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.duration = 1f;
            main.startLifetime = new ParticleSystem.MinMaxCurve(0.5f, 0.9f);
            main.startSize = new ParticleSystem.MinMaxCurve(0.22f, 0.42f);
            main.startSpeed = new ParticleSystem.MinMaxCurve(3.5f, 7.5f);
            main.startRotation = new ParticleSystem.MinMaxCurve(0f, Mathf.PI * 2f);
            main.startColor = Color.white;
            main.gravityModifier = 2.2f;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = ps.emission;
            emission.rateOverTime = 0f;
            Burst(ps, 9);
            Circle(ps);

            /* Xoay trong lúc bay: mảnh gỗ văng ra mà giữ nguyên góc thì trông như sticker trượt. */
            var rotation = ps.rotationOverLifetime;
            rotation.enabled = true;
            rotation.z = new ParticleSystem.MinMaxCurve(-6f, 6f);

            FadeOut(ps);

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = LoadOrCreateMaterial("debris_wood.png", additive: false);
            renderer.sortingOrder = 11;
            renderer.alignment = ParticleSystemRenderSpace.View;

            return SavePrefab(root).GetComponent<ParticleSystem>();
        }

        static ParticleSystem NewLayer(Transform parent, string name, string spriteFile, Color color,
            int sortingOrder)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var ps = go.AddComponent<ParticleSystem>();
            var main = ps.main;
            main.loop = false;
            main.playOnAwake = false;
            main.startColor = color;
            main.simulationSpace = ParticleSystemSimulationSpace.World;
            /* Scale theo transform: prefab được phóng to nhỏ theo bán kính nổ, hạt phải theo cùng. */
            main.scalingMode = ParticleSystemScalingMode.Hierarchy;

            var emission = ps.emission;
            emission.rateOverTime = 0f;

            var renderer = ps.GetComponent<ParticleSystemRenderer>();
            renderer.sharedMaterial = LoadOrCreateMaterial(spriteFile);
            renderer.sortingOrder = sortingOrder;
            renderer.alignment = ParticleSystemRenderSpace.View;

            return ps;
        }

        static void Burst(ParticleSystem ps, int count)
        {
            var emission = ps.emission;
            emission.SetBursts(new[] { new ParticleSystem.Burst(0f, count) });
        }

        static void Circle(ParticleSystem ps)
        {
            var shape = ps.shape;
            shape.enabled = true;
            shape.shapeType = ParticleSystemShapeType.Circle;
            shape.radius = 0.25f;
            /* Phát từ trong lòng hình tròn, không chỉ ở viền — đặc ở giữa trông giống vụ nổ hơn. */
            shape.radiusThickness = 1f;
        }

        static void SizeCurve(ParticleSystem ps, float from, float to)
        {
            var size = ps.sizeOverLifetime;
            size.enabled = true;
            size.size = new ParticleSystem.MinMaxCurve(1f, AnimationCurve.EaseInOut(0f, from, 1f, to));
        }

        /* Mờ dần ở NỬA SAU vòng đời: mờ ngay từ đầu thì vụ nổ trông yếu ngay khoảnh khắc mạnh nhất. */
        static void FadeOut(ParticleSystem ps)
        {
            var color = ps.colorOverLifetime;
            color.enabled = true;

            var gradient = new Gradient();
            gradient.SetKeys(
                new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
                new[]
                {
                    new GradientAlphaKey(1f, 0f), new GradientAlphaKey(1f, 0.45f),
                    new GradientAlphaKey(0f, 1f)
                });

            color.color = new ParticleSystem.MinMaxGradient(gradient);
        }

        /* Material là ASSET trên đĩa, nên shader chắc chắn được build tham chiếu tới. Shader.Find chỉ
           an toàn ở đây — gọi lúc chạy thì shader không ai tham chiếu sẽ bị strip và ra màu hồng. */
        static Material LoadOrCreateMaterial(string spriteFile, bool additive = true)
        {
            var name = spriteFile.Replace(".png", string.Empty);
            var path = $"{BlastOutAssetFactory.PrefabFolder}/{name}.mat";

            var existing = AssetDatabase.LoadAssetAtPath<Material>(path);
            if (existing) return existing;

            var texture = AssetDatabase.LoadAssetAtPath<Texture2D>($"{VfxFolder}/{spriteFile}");
            if (!texture) Debug.LogError($"[BlastOut] Thiếu texture VFX: {VfxFolder}/{spriteFile}");

            var material = new Material(Shader.Find("Particles/Standard Unlit"));
            material.SetTexture("_MainTex", texture);
            material.SetFloat("_Mode", 4f);
            material.SetOverrideTag("RenderType", "Transparent");
            material.SetInt("_SrcBlend", (int)UnityEngine.Rendering.BlendMode.SrcAlpha);

            /* Additive cho lửa và tia sáng: hạt chồng lên nhau thì sáng dồn lên. Mảnh gỗ thì không —
               vật thể đặc mà cộng sáng sẽ thành đốm phát quang, mất hẳn cảm giác là mảnh vỡ. */
            material.SetInt("_DstBlend", (int)(additive
                ? UnityEngine.Rendering.BlendMode.One
                : UnityEngine.Rendering.BlendMode.OneMinusSrcAlpha));

            material.SetInt("_ZWrite", 0);
            material.DisableKeyword("_ALPHATEST_ON");
            material.EnableKeyword("_ALPHABLEND_ON");
            material.renderQueue = 3000;

            BlastOutAssetFactory.EnsureFolder(BlastOutAssetFactory.PrefabFolder);
            AssetDatabase.CreateAsset(material, path);
            return material;
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
