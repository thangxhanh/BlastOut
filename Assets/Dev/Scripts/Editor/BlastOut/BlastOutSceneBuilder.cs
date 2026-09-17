using System.Collections.Generic;
using Dev.Scripts.BlastOut;
using Dev.Scripts.BlastOut.Gameplay;
using Dev.Scripts.BlastOut.UI;
using TMPro;
using UnityEditor;
using UnityEditor.SceneManagement;
using UnityEngine;

namespace Dev.Scripts.Editor.BlastOut
{
    /* Dựng toàn bộ scene gameplay bằng một lệnh menu.

       Tồn tại vì hai lý do, không phải để khoe tool:
       1. Scene/prefab là asset nhị phân — dựng bằng code thì diff trên git đọc được là "đã đổi gì",
          và dựng lại được y hệt trên máy khác.
       2. Bố cục level nằm trong asset dữ liệu, còn scene chỉ là khung. Thêm level không cần chạy
          lại tool này — chỉ tạo thêm một BlastLevelConfig rồi kéo vào controller. */
    public static class BlastOutSceneBuilder
    {
        const string ScenePath = "Assets/Dev/Scenes/GameScene.unity";
        static readonly Color SteelColor = new Color32(0x6E, 0x76, 0x88, 0xFF);
        static readonly Color ZoneColor = new Color32(0x2E, 0xD0, 0x9A, 0x44);

        [MenuItem("Tools/Blast Out/Build Game Scene", false, 0)]
        public static void Build()
        {
            /* Mở scene trắng TRƯỚC khi tạo prefab: SaveAsPrefabAsset dựng object tạm trong scene
               đang mở rồi xoá đi, nên nếu làm ngược lại thì scene của người dùng bị đánh dấu dirty
               và NewScene bật hộp thoại "Save changes?" giữa chừng.

               Lưu thẳng thay vì hỏi: hộp thoại xác nhận chặn cả Editor khi lệnh này được gọi từ
               script hoặc tool bên ngoài, và công việc đang mở vẫn được giữ chứ không mất. */
            EditorSceneManager.SaveOpenScenes();
            var scene = EditorSceneManager.NewScene(NewSceneSetup.EmptyScene, NewSceneMode.Single);

            var square = BlastOutAssetFactory.CreateSquareSprite("square", 16);
            var circle = BlastOutAssetFactory.CreateCircleSprite("circle", 64);

            var tuning = BlastOutAssetFactory.CreateTuning();
            var levels = BlastOutLevelFactory.CreateAll();
            var levelSet = BlastOutAssetFactory.CreateLevelSet(levels);
            var trailMaterial = BlastOutPrefabFactory.CreateTrailMaterial();

            /* Vật thể trong thế giới dùng art Kenney (CC0); riêng đạn và chấm quỹ đạo vẫn là hình
               tròn tự sinh vì chúng cần tô màu theo trạng thái. */
            var blockArt = BlastOutAssetFactory.LoadKenney("Gameplay/block_wood.png");
            var barrelArt = BlastOutAssetFactory.LoadKenney("Gameplay/barrel_explosive.png");
            var platformArt = BlastOutAssetFactory.LoadKenney("Gameplay/platform_metal.png");

            var platformPrefab = BlastOutPrefabFactory.CreatePlatform(platformArt);
            var movingPlatformPrefab = BlastOutPrefabFactory.CreateMovingPlatform(platformArt);
            var blockPrefab = BlastOutPrefabFactory.CreateBlock(blockArt, tuning);
            var barrelPrefab = BlastOutPrefabFactory.CreateBarrel(barrelArt, tuning);
            /* Dùng CHUNG sprite với khối mục tiêu: cùng hình, cùng cỡ, chỉ khác màu. Người chơi đọc
               ra ngay "cái này cũng là một khối, nhưng đừng đụng vào" — khác hình dạng thì phải học
               thêm một quy ước nữa, mà màu đã đủ để phân biệt. */
            var forbiddenPrefab = BlastOutPrefabFactory.CreateForbidden(blockArt);
            var projectilePrefab = BlastOutPrefabFactory.CreateProjectile(circle, tuning, trailMaterial);
            var dotPrefab = BlastOutPrefabFactory.CreateDot(circle);
            var vfxPrefab = BlastOutVfxFactory.CreateExplosion();
            var debrisPrefab = BlastOutVfxFactory.CreateDebris();

            AssetDatabase.SaveAssets();

            var camera = BuildCamera();
            var launcher = BuildLauncher(out var barrelPivot, out var muzzle);
            var preview = BuildPreview(dotPrefab, tuning);
            var collectZone = BuildCollectZone(square);
            var containers = new GameObject("World").transform;
            var levelRoot = NewChild(containers, "Level");
            var projectileRoot = NewChild(containers, "Projectiles");

            var vfx = BuildVfx(vfxPrefab, debrisPrefab, containers);
            var audioPlayer = BuildAudio();
            var impact = BuildImpactFeedback(camera.transform);
            var hud = BuildHud();
            BuildEventSystem();
            var aim = BuildAim(launcher, muzzle, barrelPivot, preview, tuning);
            var builder = BuildLevelBuilder(levelRoot, platformPrefab, movingPlatformPrefab, blockPrefab,
                barrelPrefab, forbiddenPrefab, platformArt);
            var controller = BuildController(levelSet, tuning, camera, aim, builder, collectZone, preview,
                launcher.transform, projectileRoot, projectilePrefab, hud, vfx, audioPlayer, impact);

            /* vfx/audio/impact trước controller: controller đăng ký nghe vụ nổ lúc Initialize, lúc đó
               pool hiệu ứng và vị trí gốc của camera phải sẵn sàng rồi. */
            BuildSceneLauncher(preview, aim, builder, collectZone, hud, vfx, audioPlayer, impact, controller);

            BlastOutAssetFactory.EnsureFolder("Assets/Dev/Scenes");
            EditorSceneManager.SaveScene(scene, ScenePath);
            RegisterInBuildSettings();

            AssetDatabase.SaveAssets();
            AssetDatabase.Refresh();

            Debug.Log("[BlastOut] Scene đã dựng xong: " + ScenePath +
                      "\nĐặt Game view sang tỉ lệ dọc (9:16) rồi bấm Play. Kéo bất kỳ đâu để ngắm, " +
                      "thả để bắn, chạm lần nữa lúc đạn đang bay để kích nổ.");
        }

        /* Đưa scene lên đầu Build Settings: thiếu bước này thì bấm Play trong Editor vẫn chạy,
           nhưng build APK ra lại mở một scene khác — một cái bẫy rất dễ mất thời gian. */
        static void RegisterInBuildSettings()
        {
            var scenes = new List<EditorBuildSettingsScene>(EditorBuildSettings.scenes);
            scenes.RemoveAll(s => s.path == ScenePath);
            scenes.Insert(0, new EditorBuildSettingsScene(ScenePath, true));
            EditorBuildSettings.scenes = scenes.ToArray();
        }

        static Camera BuildCamera()
        {
            var go = new GameObject("Main Camera");
            go.tag = "MainCamera";
            go.transform.position = new Vector3(0f, 0f, -10f);

            var camera = go.AddComponent<Camera>();
            camera.orthographic = true;
            camera.orthographicSize = 8f;
            camera.clearFlags = CameraClearFlags.SolidColor;
            camera.backgroundColor = BlastOutAssetFactory.SkyColor;

            /* Scene dựng từ EmptyScene nên không có sẵn AudioListener như scene mặc định của Unity.
               Thiếu nó thì mọi AudioSource vẫn chạy nhưng không phát ra tiếng nào. */
            go.AddComponent<AudioListener>();

            return camera;
        }

        static GameObject BuildLauncher(out Transform barrelPivot, out Transform muzzle)
        {
            var baseArt = BlastOutAssetFactory.LoadKenney("Gameplay/launcher_base.png");
            var barrelArt = BlastOutAssetFactory.LoadKenney("Gameplay/launcher_barrel.png");

            var root = new GameObject("Launcher");

            var basePart = NewSprite(root.transform, "Base", baseArt, Color.white, 1);
            basePart.localPosition = Vector3.zero;

            barrelPivot = NewChild(root.transform, "BarrelPivot");

            var barrel = NewSprite(barrelPivot, "Barrel", barrelArt, Color.white, 0);
            /* Sprite nòng vẽ hướng LÊN; xoay -90° cho chĩa sang phải, khớp với AimController
               (góc 0 = bắn sang phải). */
            barrel.localRotation = Quaternion.Euler(0f, 0f, -90f);

            /* Nòng lệch sang phải so với pivot: xoay pivot là nòng quét quanh gốc pháo, đúng cảm
               giác. Lấy chiều dài từ chính sprite nên đổi art không phải chỉnh lại số. */
            var barrelLength = BlastOutPrefabFactory.SpriteSize(barrelArt).y;
            barrel.localPosition = new Vector3(barrelLength * 0.5f, 0f, 0f);

            muzzle = NewChild(barrelPivot, "Muzzle");
            /* Đầu nòng, đẩy ra thêm chút để viên đạn không sinh ra bên trong chính khẩu pháo. */
            muzzle.localPosition = new Vector3(barrelLength + 0.15f, 0f, 0f);

            return root;
        }

        static BlastAudio BuildAudio()
        {
            const string folder = "Assets/Dev/Audio/SFX/BlastOut";
            var go = new GameObject("BlastAudio");
            var player = go.AddComponent<BlastAudio>();

            /* Bốn source quay vòng: nổ dây chuyền phát vài tiếng sát nhau, một source thì tiếng sau
               cắt ngang tiếng trước. */
            var sources = new Object[4];
            for (var i = 0; i < sources.Length; i++)
            {
                var source = go.AddComponent<AudioSource>();
                source.playOnAwake = false;
                /* spatialBlend 0 = 2D: game gọn trong một màn hình, gắn âm theo vị trí chỉ làm tiếng
                   nổ ở mép màn nhỏ đi vô cớ. */
                source.spatialBlend = 0f;
                sources[i] = source;
            }

            new SerializedFieldWriter(player)
                .Refs("sources", sources)
                .Ref("shoot", LoadClip(folder, "sfx_shoot"))
                .Ref("explosion", LoadClip(folder, "sfx_explosion"))
                .Ref("impact", LoadClip(folder, "sfx_impact"))
                .Ref("collect", LoadClip(folder, "sfx_collect"))
                .Ref("win", LoadClip(folder, "sfx_win"))
                .Ref("lose", LoadClip(folder, "sfx_lose"))
                .Float("sfxVolume", 0.8f)
                .Float("bigBlastRadius", 2.5f)
                .Float("impactCooldown", 0.08f)
                .Apply();
            BlastOutPrefabFactory.BindBase(player);

            return player;
        }

        static AudioClip LoadClip(string folder, string name)
        {
            var clip = AssetDatabase.LoadAssetAtPath<AudioClip>($"{folder}/{name}.ogg");
            if (!clip) Debug.LogError($"[BlastOut] Thiếu clip: {folder}/{name}.ogg");
            return clip;
        }

        static BlastImpactFeedback BuildImpactFeedback(Transform cameraTransform)
        {
            var go = new GameObject("BlastImpactFeedback");
            var feedback = go.AddComponent<BlastImpactFeedback>();

            new SerializedFieldWriter(feedback)
                .Ref("cameraTransform", cameraTransform)
                .Float("hitStopDuration", 0.055f)
                .Float("shakeDuration", 0.22f)
                .Float("shakeStrength", 0.22f)
                .Float("shakeFrequency", 26f)
                .Float("referenceRadius", 1.9f)
                .Apply();
            BlastOutPrefabFactory.BindBase(feedback);

            return feedback;
        }

        static BlastVfxPlayer BuildVfx(ParticleSystem prefab, ParticleSystem debrisPrefab, Transform parent)
        {
            var go = new GameObject("BlastVfx");
            var player = go.AddComponent<BlastVfxPlayer>();
            var container = NewChild(parent, "Vfx");

            new SerializedFieldWriter(player)
                .Ref("explosionPrefab", prefab)
                .Ref("debrisPrefab", debrisPrefab)
                .Ref("container", container)
                .Int("poolSize", 6)
                /* Khớp blastRadius trong tuning: prefab dựng theo bán kính này rồi phóng theo tỉ lệ,
                   nên vụ nổ của thùng (bán kính lớn hơn) tự trông to hơn. */
                .Float("referenceRadius", 1.9f)
                .Apply();
            BlastOutPrefabFactory.BindBase(player);

            return player;
        }

        static TrajectoryPreview BuildPreview(SpriteRenderer dotPrefab, Object tuning)
        {
            var go = new GameObject("TrajectoryPreview");
            var preview = go.AddComponent<TrajectoryPreview>();

            new SerializedFieldWriter(preview)
                .Ref("dotPrefab", dotPrefab)
                .Ref("tuning", tuning)
                .Tint("dotColor", BlastOutAssetFactory.DotColor)
                .Float("headScale", 0.22f)
                .Float("tailScale", 0.09f)
                .Apply();
            BlastOutPrefabFactory.BindBase(preview);

            return preview;
        }

        static CollectZone BuildCollectZone(Sprite square)
        {
            var go = new GameObject("CollectZone");

            var box = go.AddComponent<BoxCollider2D>();
            box.isTrigger = true;

            var visual = NewSprite(go.transform, "Visual", square, ZoneColor, -1);

            var zone = go.AddComponent<CollectZone>();
            new SerializedFieldWriter(zone).Ref("area", box).Ref("visual", visual).Apply();
            BlastOutPrefabFactory.BindBase(zone);

            return zone;
        }

        static AimController BuildAim(GameObject launcher, Transform muzzle, Transform barrelPivot,
            TrajectoryPreview preview, Object tuning)
        {
            var aim = launcher.AddComponent<AimController>();

            /* Lấy cả thân lẫn nòng để làm mờ cùng lúc — mờ mỗi một phần thì nhìn như lỗi hiển thị. */
            var parts = launcher.GetComponentsInChildren<SpriteRenderer>();

            new SerializedFieldWriter(aim)
                .Ref("muzzle", muzzle)
                .Ref("barrelPivot", barrelPivot)
                .Ref("preview", preview)
                .Ref("tuning", tuning)
                .Refs("launcherParts", parts)
                .Tint("busyTint", new Color(1f, 1f, 1f, 0.4f))
                .Apply();
            BlastOutPrefabFactory.BindBase(aim);

            return aim;
        }

        static LevelBuilder BuildLevelBuilder(Transform container, Transform platformPrefab,
            MovingPlatform movingPlatformPrefab, TargetBlock blockPrefab, ExplosiveBarrel barrelPrefab,
            ForbiddenTarget forbiddenPrefab, Sprite platformArt)
        {
            var go = new GameObject("LevelBuilder");
            var builder = go.AddComponent<LevelBuilder>();

            /* Kích thước sprite theo world unit = pixel / pixelsPerUnit. Builder cần số này để quy
               đổi chiều dài bệ trong level data sang scale. */
            var spriteSize = platformArt
                ? platformArt.rect.size / platformArt.pixelsPerUnit
                : Vector2.one;

            new SerializedFieldWriter(builder)
                .Ref("container", container)
                .Ref("platformPrefab", platformPrefab)
                .Ref("movingPlatformPrefab", movingPlatformPrefab)
                .Ref("blockPrefab", blockPrefab)
                .Ref("barrelPrefab", barrelPrefab)
                .Ref("forbiddenPrefab", forbiddenPrefab)
                .Vec2("platformSpriteSize", spriteSize)
                .Float("platformThickness", 0.32f)
                .Apply();
            BlastOutPrefabFactory.BindBase(builder);

            return builder;
        }

        static BlastGameController BuildController(Object levelSet, Object tuning, Camera camera,
            AimController aim, LevelBuilder builder, CollectZone zone, TrajectoryPreview preview,
            Transform launcherRoot, Transform projectileRoot, BlastProjectile projectilePrefab,
            BlastHudView hud, BlastVfxPlayer vfx, BlastAudio audioPlayer, BlastImpactFeedback impact)
        {
            var go = new GameObject("BlastGameController");
            var controller = go.AddComponent<BlastGameController>();

            new SerializedFieldWriter(controller)
                .Ref("levelSet", levelSet)
                .Ref("tuning", tuning)
                .Ref("view", camera)
                .Ref("aim", aim)
                .Ref("builder", builder)
                .Ref("collectZone", zone)
                .Ref("preview", preview)
                .Ref("launcherRoot", launcherRoot)
                .Ref("projectileContainer", projectileRoot)
                .Ref("projectilePrefab", projectilePrefab)
                .Ref("hud", hud)
                .Ref("vfx", vfx)
                .Ref("audioPlayer", audioPlayer)
                .Ref("impact", impact)
                .Apply();

            /* Controller là BaseMono DUY NHẤT đăng ký nhịp — mọi thứ khác được nó gọi xuống.
               FixedTick dành riêng cho bệ chạy, thứ duy nhất phải đi theo nhịp vật lý. */
            BlastOutPrefabFactory.BindBase(controller, tick: true, fixedTick: true);

            return controller;
        }

        static void BuildSceneLauncher(params Dacodelaac.Core.BaseMono[] systems)
        {
            var go = new GameObject("BlastSceneLauncher");
            var launcher = go.AddComponent<BlastSceneLauncher>();

            new SerializedFieldWriter(launcher)
                .Refs("sceneSystems", systems)
                .Refs("prefabs")
                .Apply();
            BlastOutPrefabFactory.BindBase(launcher);
        }

        static BlastHudView BuildHud()
        {
            /* Nút dùng art Kenney (CC0) nên tô trắng để giữ nguyên màu sprite. */
            var blueButton = BlastOutAssetFactory.LoadKenney("UI/button_blue.png");
            var greyButton = BlastOutAssetFactory.LoadKenney("UI/button_grey.png");

            var go = new GameObject("HUD", typeof(RectTransform));
            var canvas = go.AddComponent<Canvas>();
            canvas.renderMode = RenderMode.ScreenSpaceOverlay;

            var scaler = go.AddComponent<UnityEngine.UI.CanvasScaler>();
            scaler.uiScaleMode = UnityEngine.UI.CanvasScaler.ScaleMode.ScaleWithScreenSize;
            scaler.referenceResolution = new Vector2(1080f, 1920f);
            scaler.matchWidthOrHeight = 0.5f;
            go.AddComponent<UnityEngine.UI.GraphicRaycaster>();

            var levelLabel = NewLabel(go.transform, "LevelLabel", "LEVEL 01", 44f,
                new Vector2(0f, 1f), new Vector2(40f, -70f), TextAlignmentOptions.TopLeft);
            var ammoLabel = NewLabel(go.transform, "AmmoLabel", "AMMO 2/2", 44f,
                new Vector2(1f, 1f), new Vector2(-40f, -70f), TextAlignmentOptions.TopRight);
            var hintLabel = NewLabel(go.transform, "HintLabel", "DRAG TO AIM", 38f,
                new Vector2(0.5f, 0f), new Vector2(0f, 190f), TextAlignmentOptions.Center);
            hintLabel.color = new Color(0.66f, 0.71f, 0.83f);

            /* Nút Restart luôn hiện ở góc trên giữa để thử lại nhanh khi đang chơi. */
            var restartButton = NewButton(go.transform, "RestartButton", "RESTART", greyButton,
                new Vector2(0.5f, 1f), new Vector2(0f, -78f), new Vector2(240f, 96f), 34f, out _);

            var resultRoot = BuildResultPanel(go.transform, blueButton, out var resultLabel,
                out var actionButton, out var actionLabel);

            var hud = go.AddComponent<BlastHudView>();
            new SerializedFieldWriter(hud)
                .Ref("levelLabel", levelLabel)
                .Ref("ammoLabel", ammoLabel)
                .Ref("hintLabel", hintLabel)
                .Ref("resultRoot", resultRoot)
                .Ref("resultLabel", resultLabel)
                .Ref("actionButton", actionButton)
                .Ref("actionLabel", actionLabel)
                .Ref("restartButton", restartButton)
                .Apply();
            BlastOutPrefabFactory.BindBase(hud);

            return hud;
        }

        /* Panel phủ kín màn khi thắng/thua: nền mờ chặn thao tác phía sau, một dòng kết quả và một
           nút chính (Next khi thắng, Retry khi thua). Tắt sẵn, controller bật khi có kết quả. */
        static GameObject BuildResultPanel(Transform parent, Sprite buttonSprite, out TMP_Text resultLabel,
            out UnityEngine.UI.Button actionButton, out TMP_Text actionLabel)
        {
            var root = new GameObject("ResultPanel", typeof(RectTransform));
            root.transform.SetParent(parent, false);
            Stretch((RectTransform)root.transform);

            var dim = new GameObject("Dim", typeof(RectTransform));
            dim.transform.SetParent(root.transform, false);
            var dimImage = dim.AddComponent<UnityEngine.UI.Image>();
            dimImage.color = new Color(0.03f, 0.05f, 0.1f, 0.72f);
            Stretch(dimImage.rectTransform);

            resultLabel = NewLabel(root.transform, "ResultLabel", "LEVEL CLEAR", 74f,
                new Vector2(0.5f, 0.5f), new Vector2(0f, 150f), TextAlignmentOptions.Center);
            resultLabel.rectTransform.sizeDelta = new Vector2(1000f, 120f);

            actionButton = NewButton(root.transform, "ActionButton", "NEXT", buttonSprite,
                new Vector2(0.5f, 0.5f), new Vector2(0f, -40f), new Vector2(380f, 130f), 46f, out actionLabel);

            root.SetActive(false);
            return root;
        }

        static UnityEngine.UI.Button NewButton(Transform parent, string name, string text, Sprite sprite,
            Vector2 anchor, Vector2 offset, Vector2 size, float fontSize, out TMP_Text label)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var image = go.AddComponent<UnityEngine.UI.Image>();
            image.sprite = sprite;
            image.color = Color.white;

            /* Nút Kenney có viền bo và phần chân dày — Sliced mới giữ đúng tỉ lệ viền khi kéo giãn,
               Simple sẽ làm viền méo theo kích thước nút. */
            image.type = UnityEngine.UI.Image.Type.Sliced;

            var rect = (RectTransform)go.transform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = size;
            rect.anchoredPosition = offset;

            var button = go.AddComponent<UnityEngine.UI.Button>();
            button.targetGraphic = image;

            label = NewLabel(go.transform, "Label", text, fontSize,
                new Vector2(0.5f, 0.5f), Vector2.zero, TextAlignmentOptions.Center);
            /* Nút Kenney nền sáng — chữ trắng mặc định sẽ chìm, nên dùng màu tối. */
            label.color = new Color(0.1f, 0.13f, 0.2f);
            /* Nhãn phủ kín nút để căn giữa dù đổi kích thước. */
            Stretch(label.rectTransform);

            return button;
        }

        static void Stretch(RectTransform rect)
        {
            rect.anchorMin = Vector2.zero;
            rect.anchorMax = Vector2.one;
            rect.pivot = new Vector2(0.5f, 0.5f);
            rect.sizeDelta = Vector2.zero;
            rect.anchoredPosition = Vector2.zero;
        }

        /* UI Button cần EventSystem mới nhận được click. Scene dựng từ scratch nên phải tạo tay —
           thiếu nó thì nút hiện ra nhưng bấm không ăn, một cái bẫy rất dễ mất thời gian. */
        static void BuildEventSystem()
        {
            var go = new GameObject("EventSystem");
            go.AddComponent<UnityEngine.EventSystems.EventSystem>();
            go.AddComponent<UnityEngine.EventSystems.StandaloneInputModule>();
        }

        static TMP_Text NewLabel(Transform parent, string name, string text, float size,
            Vector2 anchor, Vector2 offset, TextAlignmentOptions alignment)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);

            var label = go.AddComponent<TextMeshProUGUI>();
            label.text = text;
            label.fontSize = size;
            label.alignment = alignment;
            label.color = Color.white;
            label.raycastTarget = false;

            var rect = label.rectTransform;
            rect.anchorMin = anchor;
            rect.anchorMax = anchor;
            rect.pivot = anchor;
            rect.sizeDelta = new Vector2(900f, 90f);
            rect.anchoredPosition = offset;

            return label;
        }

        static Transform NewSprite(Transform parent, string name, Sprite sprite, Color color, int order)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);

            var renderer = go.AddComponent<SpriteRenderer>();
            renderer.sprite = sprite;
            renderer.color = color;
            renderer.sortingOrder = order;

            return go.transform;
        }

        static Transform NewChild(Transform parent, string name)
        {
            var go = new GameObject(name);
            go.transform.SetParent(parent, false);
            return go.transform;
        }
    }
}
