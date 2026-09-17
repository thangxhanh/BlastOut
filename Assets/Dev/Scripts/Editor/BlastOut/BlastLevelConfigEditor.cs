using Dev.Scripts.BlastOut.Config;
using Dev.Scripts.BlastOut.Core;
using UnityEditor;
using UnityEngine;

namespace Dev.Scripts.Editor.BlastOut
{
    /* Sửa level ngay trong Scene View: chọn asset level là thấy toàn bộ bố cục, kéo từng vật thể
       bằng chuột, sửa tới đâu ghi vào asset tới đó — không có bước "load rồi nhớ save".

       Thứ quan trọng nhất ở đây KHÔNG phải việc kéo thả, mà là VÒNG BÁN KÍNH vẽ kèm. Level 7 từng
       không thể thắng vì vùng cấm nổ quanh mục tiêu cấm nuốt gần hết chỗ được phép nổ — nhìn dãy
       số trong Inspector thì không đời nào thấy, còn nhìn hai vòng tròn chồng nhau thì thấy ngay. */
    [CustomEditor(typeof(BlastLevelConfig))]
    public class BlastLevelConfigEditor : UnityEditor.Editor
    {
        const string TuningPath = "Assets/Dev/Data/BlastOut/blast_tuning.asset";
        const float PlatformThickness = 0.32f;

        static readonly Color PlatformFill = new Color(0.55f, 0.62f, 0.72f, 0.35f);
        static readonly Color BlockFill = new Color(0.72f, 0.48f, 0.25f, 0.85f);
        static readonly Color BarrelFill = new Color(0.9f, 0.25f, 0.2f, 0.85f);
        static readonly Color ForbiddenFill = new Color(0.35f, 1f, 0.25f, 0.9f);
        static readonly Color DangerZone = new Color(1f, 0.2f, 0.15f, 0.9f);
        static readonly Color BarrelZone = new Color(1f, 0.6f, 0.1f, 0.5f);
        static readonly Color PathColor = new Color(0.4f, 0.85f, 1f, 0.8f);

        BlastTuning tuning;

        void OnEnable()
        {
            tuning = AssetDatabase.LoadAssetAtPath<BlastTuning>(TuningPath);
            SceneView.duringSceneGui += OnSceneGui;
        }

        void OnDisable()
        {
            SceneView.duringSceneGui -= OnSceneGui;
        }

        public override void OnInspectorGUI()
        {
            EditorGUILayout.HelpBox(
                "Mở Scene View để sửa bố cục bằng chuột. Vòng ĐỎ quanh mục tiêu cấm là vùng KHÔNG " +
                "được nổ; chỗ bắn hợp lệ là phần nằm ngoài nó. Vòng CAM là tầm nổ của thùng.",
                MessageType.Info);

            EditorGUILayout.Space();
            base.OnInspectorGUI();
        }

        void OnSceneGui(SceneView view)
        {
            var level = (BlastLevelConfig)target;
            if (!level) return;

            DrawCollectZone(level);
            DrawPlatforms(level);
            DrawLauncher(level);

            /* Vẽ vùng ảnh hưởng TRƯỚC vật thể: vòng tròn là nền, vật thể nằm đè lên cho dễ bắt. */
            DrawDangerZones(level);

            DrawBlocks(level);
            DrawBarrels(level);
            DrawForbidden(level);
        }

        void DrawPlatforms(BlastLevelConfig level)
        {
            var list = level.Platforms;
            if (list == null) return;

            for (var i = 0; i < list.Length; i++)
            {
                var center = list[i].Center;
                var half = new Vector2(list[i].Width * 0.5f, PlatformThickness * 0.5f);

                Handles.color = PlatformFill;
                Handles.DrawSolidRectangleWithOutline(new[]
                {
                    (Vector3)(center + new Vector2(-half.x, -half.y)),
                    (Vector3)(center + new Vector2(-half.x, half.y)),
                    (Vector3)(center + new Vector2(half.x, half.y)),
                    (Vector3)(center + new Vector2(half.x, -half.y))
                }, PlatformFill, Color.white);

                DrawMotionPath(center, list[i].Motion);

                if (Move(ref center, PlatformFill, 0.14f, level, "Move Platform"))
                {
                    level.Platforms[i].Center = center;
                }
            }
        }

        /* Bệ chạy: vẽ luôn đường nó sẽ đi, nếu không thì nhìn asset tĩnh chẳng đoán được nó tới đâu. */
        void DrawMotionPath(Vector2 center, PlatformMotion motion)
        {
            if (!motion.IsMoving) return;

            Handles.color = PathColor;

            if (motion.Kind == MotionKind.Circle)
            {
                Handles.DrawWireDisc(center, Vector3.forward, motion.Offset(0f).magnitude > 0.001f
                    ? motion.Offset(0f).magnitude
                    : 1f);
                return;
            }

            /* Lấy biên bằng cách quét một chu kỳ: đỡ phải phơi thêm thuộc tính chỉ để vẽ. */
            var min = Vector2.zero;
            var max = Vector2.zero;
            for (var t = 0f; t <= 1f; t += 0.02f)
            {
                var offset = motion.Offset(100f + t * 40f);
                min = Vector2.Min(min, offset);
                max = Vector2.Max(max, offset);
            }

            Handles.DrawLine(center + min, center + max);
            Handles.DrawWireCube(center + min, Vector3.one * 0.12f);
            Handles.DrawWireCube(center + max, Vector3.one * 0.12f);
        }

        void DrawBlocks(BlastLevelConfig level)
        {
            var list = level.Blocks;
            if (list == null) return;

            for (var i = 0; i < list.Length; i++)
            {
                var p = list[i].Position;
                Handles.color = BlockFill;
                Handles.DrawSolidDisc(p, Vector3.forward, 0.42f);

                if (Move(ref p, BlockFill, 0.12f, level, "Move Block")) level.Blocks[i].Position = p;
            }
        }

        void DrawBarrels(BlastLevelConfig level)
        {
            var list = level.Barrels;
            if (list == null) return;

            for (var i = 0; i < list.Length; i++)
            {
                var p = list[i].Position;
                Handles.color = BarrelFill;
                Handles.DrawSolidDisc(p, Vector3.forward, 0.35f);

                if (Move(ref p, BarrelFill, 0.12f, level, "Move Barrel")) level.Barrels[i].Position = p;
            }
        }

        void DrawForbidden(BlastLevelConfig level)
        {
            var list = level.Forbidden;
            if (list == null) return;

            for (var i = 0; i < list.Length; i++)
            {
                var p = list[i].Position;
                Handles.color = ForbiddenFill;
                Handles.DrawSolidDisc(p, Vector3.forward, 0.5f);

                if (Move(ref p, ForbiddenFill, 0.14f, level, "Move Forbidden"))
                {
                    level.Forbidden[i].Position = p;
                }
            }
        }

        /* Hai vùng quyết định level có chơi được không:
             đỏ  — nổ vào đây là thua, nên chỗ bắn hợp lệ phải nằm NGOÀI;
             cam — thùng nổ quét tới đâu, để biết dây chuyền có lan sang thứ không nên chạm không. */
        void DrawDangerZones(BlastLevelConfig level)
        {
            if (!tuning) return;

            var forbidden = level.Forbidden;
            if (forbidden != null)
            {
                for (var i = 0; i < forbidden.Length; i++)
                {
                    Handles.color = DangerZone;
                    Handles.DrawWireDisc(forbidden[i].Position, Vector3.forward, tuning.BlastRadius, 2f);
                }
            }

            var barrels = level.Barrels;
            if (barrels == null) return;

            for (var i = 0; i < barrels.Length; i++)
            {
                Handles.color = BarrelZone;
                Handles.DrawWireDisc(barrels[i].Position, Vector3.forward, tuning.BarrelRadius);
            }
        }

        void DrawLauncher(BlastLevelConfig level)
        {
            var p = level.LauncherPosition;

            Handles.color = Color.white;
            Handles.DrawWireCube(p, new Vector3(0.9f, 0.7f, 0f));
            Handles.Label(p + new Vector2(-0.4f, 0.6f), $"LV {level.DisplayNumber}");

            if (Move(ref p, Color.white, 0.14f, level, "Move Launcher"))
            {
                var so = new SerializedObject(level);
                so.FindProperty("launcherPosition").vector2Value = p;
                so.ApplyModifiedProperties();
            }
        }

        void DrawCollectZone(BlastLevelConfig level)
        {
            var y = level.CollectZoneTopY;
            Handles.color = new Color(0.2f, 0.9f, 0.6f, 0.8f);
            Handles.DrawLine(new Vector3(-6f, y), new Vector3(6f, y));
            Handles.Label(new Vector3(-5.9f, y + 0.2f), "COLLECT");
        }

        /* Trả về true nếu người dùng vừa kéo. Gộp Undo + SetDirty vào một chỗ để mọi lời gọi đều
           ghi được vào asset và hoàn tác được — thiếu SetDirty thì thay đổi biến mất lúc Unity
           nạp lại domain, mà lỗi đó rất khó nhận ra. */
        static bool Move(ref Vector2 position, Color color, float size, Object asset, string label)
        {
            Handles.color = color;

            EditorGUI.BeginChangeCheck();
            var handleSize = HandleUtility.GetHandleSize(position) * size;
            var moved = Handles.FreeMoveHandle(position, handleSize, Vector3.zero, Handles.DotHandleCap);
            if (!EditorGUI.EndChangeCheck()) return false;

            Undo.RecordObject(asset, label);
            position = new Vector2(moved.x, moved.y);
            EditorUtility.SetDirty(asset);
            return true;
        }
    }
}
