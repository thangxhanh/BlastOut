using System.Collections.Generic;
using Dacodelaac.Core;
using Dacodelaac.DataStorage;
using Dev.Scripts.BlastOut.Config;
using Dev.Scripts.BlastOut.Core;
using Dev.Scripts.BlastOut.UI;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Gameplay
{
    /* Điều phối một lượt chơi: nối input ↔ model ↔ thế giới vật lý.

       Đây là ĐIỂM VÀO DUY NHẤT mỗi frame của toàn bộ gameplay (tick = true trong Inspector).
       Mọi thứ khác được gọi từ đây, không component nào có Update() riêng — nhờ vậy thứ tự trong
       một frame là đọc được, và bật/tắt cả gameplay chỉ là bật/tắt một object. */
    public class BlastGameController : BaseMono
    {
        [Header("Dữ liệu")]
        [SerializeField] BlastLevelSet levelSet;
        [SerializeField] BlastTuning tuning;

        /* Tiến độ lưu qua GameData để mở lại game là vào đúng level đang chơi dở. Key riêng của
           BlastOut, không đụng key nào của framework. */
        const string LevelKey = "blast_current_level";

        [Header("Thành phần scene")]
        [SerializeField] Camera view;
        [SerializeField] AimController aim;
        [SerializeField] LevelBuilder builder;
        [SerializeField] CollectZone collectZone;
        [SerializeField] TrajectoryPreview preview;
        [SerializeField] Transform launcherRoot;
        [SerializeField] Transform projectileContainer;
        [SerializeField] BlastProjectile projectilePrefab;
        [SerializeField] BlastHudView hud;
        [SerializeField] BlastVfxPlayer vfx;
        [SerializeField] BlastAudio audioPlayer;
        [SerializeField] BlastImpactFeedback impact;

        readonly BlastSession session = new BlastSession();
        readonly List<BlastProjectile> flying = new List<BlastProjectile>(8);

        BlastResolver resolver;
        ProjectilePool projectiles;
        BlastLevelConfig level;
        Transform launcherAnchor;
        Vector2 launcherOffset;
        Rect playArea;
        float levelTime;
        int levelIndex;
        int ammoIndex;
        float settleElapsed;
        float settleHeld;

        public BlastSession Session => session;

        public override void Initialize()
        {
            base.Initialize();

            resolver = new BlastResolver();
            projectiles = new ProjectilePool(projectilePrefab, projectileContainer);

            /* Nghe ở resolver thay vì gọi VFX từ đạn và từ thùng: mọi vụ nổ đều đi qua đó, kể cả
               nổ dây chuyền, nên hiệu ứng khớp mọi nguồn mà không rải lời gọi khắp nơi. */
            if (vfx) resolver.Blasted += vfx.PlayExplosion;
            if (audioPlayer) resolver.Blasted += audioPlayer.PlayExplosion;
            if (impact) resolver.Blasted += impact.OnBlast;

            preview.Initialize();
            aim.Bind(view);

            aim.Fired += OnFired;
            collectZone.BlockCollected += OnBlockCollected;
            session.PhaseChanged += OnPhaseChanged;
            session.AmmoChanged += OnAmmoChanged;
            hud.ActionPressed += OnHudAction;
            hud.RestartPressed += RestartLevel;

            levelIndex = Mathf.Clamp(GameData.Get<int>(LevelKey, 0), 0, Mathf.Max(0, levelSet.Count - 1));
            StartLevel();
        }

        public override void CleanUp()
        {
            if (vfx) resolver.Blasted -= vfx.PlayExplosion;
            if (audioPlayer) resolver.Blasted -= audioPlayer.PlayExplosion;
            if (impact) resolver.Blasted -= impact.OnBlast;
            aim.Fired -= OnFired;
            collectZone.BlockCollected -= OnBlockCollected;
            session.PhaseChanged -= OnPhaseChanged;
            session.AmmoChanged -= OnAmmoChanged;
            hud.ActionPressed -= OnHudAction;
            hud.RestartPressed -= RestartLevel;
        }

        /* Nút chính trên panel kết quả: thắng thì đi tiếp, thua thì chơi lại đúng level đó. */
        void OnHudAction()
        {
            if (session.Phase == BlastPhase.Won) AdvanceLevel();
            else if (session.Phase == BlastPhase.Lost) RestartLevel();
        }

        void StartLevel()
        {
            level = levelSet.Get(levelIndex);

            ammoIndex = 0;
            flying.Clear();
            projectiles.DeactivateAll();

            var halfHeight = view.orthographicSize;
            var halfWidth = halfHeight * view.aspect;
            var bounds = halfWidth * 2f;
            collectZone.SetTopEdge(level.CollectZoneTopY, bounds);

            /* Lề quanh khung nhìn: đủ để đạn vừa khuất mép vẫn còn cơ hội bay trở vào, nhưng không
               rộng tới mức phải chờ lâu khi cú bắn đã hỏng hẳn. */
            const float margin = 1.5f;
            playArea = Rect.MinMaxRect(
                -halfWidth - margin, level.CollectZoneTopY - margin,
                halfWidth + margin, halfHeight + margin);
            launcherRoot.position = level.LauncherPosition;

            builder.Build(level, resolver);

            /* Khối được dựng lại mỗi level nên phải nối lại tiếng va chạm sau mỗi lần Build. */
            if (audioPlayer)
            {
                var blocks = builder.Blocks;
                for (var i = 0; i < blocks.Count; i++) blocks[i].BindImpact(audioPlayer.PlayImpact);
            }

            /* Pháo có thể đứng trên một bệ đang chạy. Giữ khoảng lệch lúc dựng rồi bám theo mỗi
               frame, nhờ vậy pháo không bao giờ rời khỏi mặt bệ dù quỹ đạo là đường hay vòng. */
            levelTime = 0f;
            launcherAnchor = builder.GetPlatformAnchor(level.LauncherPlatformIndex);
            launcherOffset = launcherAnchor
                ? level.LauncherPosition - (Vector2)launcherAnchor.position
                : Vector2.zero;

            hud.ShowLevel(level.DisplayNumber, level.Hint);

            session.Begin(level.AmmoCount, level.TargetCount);
        }

        public void RestartLevel()
        {
            preview.Hide();
            StartLevel();
        }

        /* Thắng thì sang level kế. Hết bộ level thì quay vòng về đầu — với người chơi casual, cụt
           ở màn cuối khó chịu hơn là chơi lại từ đầu. Tiến độ lưu ngay để mở lại vào đúng chỗ. */
        void AdvanceLevel()
        {
            levelIndex = levelSet.Count > 0 ? (levelIndex + 1) % levelSet.Count : 0;
            GameData.Set(LevelKey, levelIndex);
            GameData.Save();
            RestartLevel();
        }

        public override void Tick()
        {
            var deltaTime = Time.deltaTime;

            /* Trước switch và không phụ thuộc phase: khựng hình đặt timeScale về 0, nên chính nó
               phải được tick bằng thời gian thực mới tự nhả ra được — bỏ sót là game treo. */
            if (impact) impact.ManualTick();

            /* Pháo bám bệ ở đây chứ không ở FixedTick: bệ bật interpolation nên vị trí hiển thị chỉ
               đúng ở nhịp render, đặt theo nhịp vật lý thì khẩu pháo giật so với bệ dưới chân. */
            if (launcherAnchor) launcherRoot.position = (Vector2)launcherAnchor.position + launcherOffset;

            switch (session.Phase)
            {
                case BlastPhase.Flying:
                    TickFlight(deltaTime);
                    break;
                case BlastPhase.Resolving:
                    TickSettle(deltaTime);
                    break;
                /* Thắng/thua thì dừng lại chờ người chơi bấm nút trên panel — không xử lý input ngắm
                   nữa. Việc "bấm gì thì làm gì" nằm ở OnHudAction, HUD chỉ bắn tín hiệu ra. */
                case BlastPhase.Won:
                case BlastPhase.Lost:
                    return;
            }

            aim.HandleInput(session.CanAim);
        }

        /* Bệ chạy được đẩy theo NHỊP VẬT LÝ. Đẩy theo nhịp render thì vận tốc đặt lệch pha với lúc
           va chạm được giải, ma sát không giữ nổi và khối tụt dần khỏi bệ dù ma sát đã tối đa.

           Chạy ở mọi phase, kể cả lúc đạn đang bay: dừng bệ giữa chừng thì cú bắn vừa canh thời
           điểm trở nên vô nghĩa. */
        public override void FixedTick()
        {
            levelTime += Time.fixedDeltaTime;

            var moving = builder.MovingPlatforms;
            for (var i = 0; i < moving.Count; i++)
            {
                var platform = moving[i];
                if (platform) platform.ManualTick(levelTime);
            }
        }

        /* Đạn đang bay: một cú chạm bất kỳ = kích nổ TẤT CẢ đạn đang bay.
           Không bắt chạm trúng viên đạn — viên đạn nhỏ và đang bay nhanh, bắt trúng là ức chế. */
        void TickFlight(float deltaTime)
        {
            var detonate = Input.GetMouseButtonDown(0);

            for (var i = flying.Count - 1; i >= 0; i--)
            {
                var projectile = flying[i];
                if (projectile.Consumed) continue;

                if (detonate) projectile.Detonate();
                else if (IsOutOfPlay(projectile.Position)) projectile.Discard();
                else projectile.ManualTick(deltaTime);
            }
        }

        /* Đạn đã ra khỏi khung nhìn và không còn đường quay lại. Không chờ hết tuổi thọ ở đây:
           đứng nhìn màn hình trống vài giây sau một cú bắn hỏng là quãng chờ khó chịu nhất.

           Phía TRÊN cố ý không tính là ngoài cuộc — đạn bắn vồng lên cao vẫn sẽ rơi xuống. */
        bool IsOutOfPlay(Vector2 position)
        {
            return position.x < playArea.xMin || position.x > playArea.xMax || position.y < playArea.yMin;
        }

        /* Chờ mọi thứ đứng yên rồi mới kết luận: một khối còn đang lăn vẫn có thể rơi vào vùng thu. */
        void TickSettle(float deltaTime)
        {
            settleElapsed += deltaTime;

            if (IsWorldSettled()) settleHeld += deltaTime;
            else settleHeld = 0f;

            if (settleHeld < tuning.SettleHoldTime && settleElapsed < tuning.SettleTimeout) return;

            session.OnPhysicsSettled();
        }

        bool IsWorldSettled()
        {
            var threshold = tuning.SettleSpeedThreshold * tuning.SettleSpeedThreshold;

            var blocks = builder.Blocks;
            for (var i = 0; i < blocks.Count; i++)
            {
                var block = blocks[i];
                if (!block || !block.gameObject.activeSelf) continue;
                if (!block.IsSettled(tuning.SettleSpeedThreshold)) return false;
            }

            /* Thùng nổ cũng phải tính: một thùng còn đang lăn có thể rơi trúng khối khác và làm
               đổi kết quả sau khi ta đã kết luận thua. */
            var barrels = builder.Barrels;
            for (var i = 0; i < barrels.Count; i++)
            {
                var barrel = barrels[i];
                if (!barrel || !barrel.gameObject.activeSelf) continue;
                if (barrel.Body.linearVelocity.sqrMagnitude > threshold) return false;
            }

            return true;
        }

        void OnFired(Vector2 velocity)
        {
            if (!session.TryFire()) return;

            var type = level.Ammo[Mathf.Min(ammoIndex, level.Ammo.Length - 1)];
            ammoIndex++;

            if (audioPlayer) audioPlayer.PlayShoot();
            Spawn(aim.MuzzlePosition, velocity, type);
        }

        void Spawn(Vector2 position, Vector2 velocity, AmmoType type)
        {
            var projectile = projectiles.Get(position);
            projectile.Launch(velocity, type, resolver, OnProjectileDespawned, OnSplitRequested);
            flying.Add(projectile);
        }

        /* Mảnh tách ra luôn là Bomb — chỉ viên gốc mới được tách (xem BlastProjectile.canSplit). */
        void OnSplitRequested(Vector2 position, Vector2 velocity)
        {
            Spawn(position, velocity, AmmoType.Bomb);
        }

        void OnProjectileDespawned(BlastProjectile projectile)
        {
            flying.Remove(projectile);

            /* Chỉ khi viên cuối cùng biến mất mới sang Resolving — nếu không, Splitter sẽ chuyển
               phase ngay lúc vừa tách, trong khi 3 mảnh còn đang bay. */
            if (flying.Count == 0) session.OnProjectileSpent();
        }

        void OnBlockCollected(TargetBlock block)
        {
            if (audioPlayer) audioPlayer.PlayCollect();
            session.OnTargetCollected();
        }

        void OnPhaseChanged(BlastPhase phase)
        {
            if (phase == BlastPhase.Resolving)
            {
                settleElapsed = 0f;
                settleHeld = 0f;
            }

            if (audioPlayer)
            {
                if (phase == BlastPhase.Won) audioPlayer.PlayWin();
                else if (phase == BlastPhase.Lost) audioPlayer.PlayLose();
            }

            hud.ShowPhase(phase);
        }

        void OnAmmoChanged(int remaining)
        {
            hud.ShowAmmo(remaining, level.AmmoCount);
        }
    }
}
