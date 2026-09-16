using System.Collections.Generic;
using Dacodelaac.Core;
using Dacodelaac.DebugUtils;
using Dev.Scripts.BlastOut.Config;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Dựng level từ asset dữ liệu. Thêm level mới = tạo thêm một BlastLevelConfig, không đụng code.

       Builder chỉ biết "đặt prefab ở đâu" — nó không biết luật thắng thua, không biết vụ nổ hoạt
       động thế nào. Thêm loại vật thể mới chỉ cần thêm một prefab + một mảng trong config. */
    public class LevelBuilder : BaseMono
    {
        [SerializeField] Transform container;
        [SerializeField] Transform platformPrefab;
        [Tooltip("Bệ chạy theo quỹ đạo: cùng hình dáng nhưng có Rigidbody2D Kinematic để kéo được " +
                 "vật đặt trên.")]
        [SerializeField] MovingPlatform movingPlatformPrefab;
        [Tooltip("Kích thước sprite bệ theo world unit, lấy lúc dựng prefab. Dùng để quy đổi chiều " +
                 "dài khai báo trong level data sang scale.")]
        [SerializeField] Vector2 platformSpriteSize = Vector2.one;
        [SerializeField] TargetBlock blockPrefab;
        [SerializeField] ExplosiveBarrel barrelPrefab;
        [Tooltip("Độ dày bệ. Chiều dài lấy từ level data, nên một prefab bệ dùng cho mọi kích thước.")]
        [SerializeField] float platformThickness = 0.32f;

        readonly List<TargetBlock> blocks = new List<TargetBlock>(16);
        readonly List<ExplosiveBarrel> barrels = new List<ExplosiveBarrel>(8);
        readonly List<GameObject> platforms = new List<GameObject>(8);
        readonly List<MovingPlatform> movingPlatforms = new List<MovingPlatform>(4);

        /* Transform của từng bệ theo đúng thứ tự khai báo trong level data — controller cần để cho
           khẩu pháo bám theo bệ nó đứng lên. */
        readonly List<Transform> platformAnchors = new List<Transform>(8);

        public IReadOnlyList<TargetBlock> Blocks => blocks;
        public IReadOnlyList<ExplosiveBarrel> Barrels => barrels;
        public IReadOnlyList<MovingPlatform> MovingPlatforms => movingPlatforms;

        public Transform GetPlatformAnchor(int index)
        {
            if (index < 0 || index >= platformAnchors.Count) return null;
            return platformAnchors[index];
        }

        public void Build(BlastLevelConfig config, BlastResolver resolver)
        {
            Clear();

            if (!config.IsValid(out var error))
            {
                Dacoder.LogError(error);
                return;
            }

            BuildPlatforms(config);
            BuildBlocks(config);
            BuildBarrels(config, resolver);
        }

        void BuildPlatforms(BlastLevelConfig config)
        {
            var list = config.Platforms;
            if (list == null) return;

            for (var i = 0; i < list.Length; i++)
            {
                var placement = list[i];
                var moving = placement.Motion.IsMoving && movingPlatformPrefab;

                var platform = moving
                    ? Instantiate(movingPlatformPrefab, container).transform
                    : Instantiate(platformPrefab, container);

                /* Giãn bằng localScale chứ không bằng SpriteRenderer.size: sprite 1×1 không có
                   border nên draw mode Sliced sẽ cảnh báo, còn scale thì kéo luôn cả BoxCollider2D
                   theo — một prefab bệ dùng được cho mọi chiều dài khai báo trong level data.

                   Chia cho kích thước sprite để Width/Thickness trong level data luôn là ĐỘ DÀI THẬT
                   theo world unit, không phụ thuộc art đang dùng sprite bao nhiêu pixel. */
                platform.position = placement.Center;
                platform.localScale = new Vector3(
                    placement.Width / Mathf.Max(0.0001f, platformSpriteSize.x),
                    platformThickness / Mathf.Max(0.0001f, platformSpriteSize.y),
                    1f);

                if (moving)
                {
                    var runner = platform.GetComponent<MovingPlatform>();
                    runner.Bind(placement.Center, placement.Motion);
                    movingPlatforms.Add(runner);
                }

                platforms.Add(platform.gameObject);
                platformAnchors.Add(platform);
            }
        }

        void BuildBlocks(BlastLevelConfig config)
        {
            var list = config.Blocks;
            for (var i = 0; i < list.Length; i++)
            {
                var block = Instantiate(blockPrefab, container);
                block.transform.position = list[i].Position;
                blocks.Add(block);
            }
        }

        void BuildBarrels(BlastLevelConfig config, BlastResolver resolver)
        {
            var list = config.Barrels;
            if (list == null) return;

            for (var i = 0; i < list.Length; i++)
            {
                var barrel = Instantiate(barrelPrefab, container);
                barrel.transform.position = list[i].Position;
                barrel.Bind(resolver);
                barrels.Add(barrel);
            }
        }

        public void Clear()
        {
            for (var i = 0; i < blocks.Count; i++) DestroySpawned(blocks[i] ? blocks[i].gameObject : null);
            for (var i = 0; i < barrels.Count; i++) DestroySpawned(barrels[i] ? barrels[i].gameObject : null);
            for (var i = 0; i < platforms.Count; i++) DestroySpawned(platforms[i]);

            blocks.Clear();
            barrels.Clear();
            platforms.Clear();
            movingPlatforms.Clear();
            platformAnchors.Clear();
        }

        static void DestroySpawned(GameObject target)
        {
            if (target) Destroy(target);
        }
    }
}
