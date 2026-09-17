using Dev.Scripts.BlastOut.Config;
using Dev.Scripts.BlastOut.Core;
using UnityEditor;
using UnityEngine;

namespace Dev.Scripts.Editor.BlastOut
{
    /* Sinh bộ level. Mỗi level là một asset dữ liệu, gameplay không biết level nào tồn tại.

       Thứ tự level là một mạch DẠY dần: mỗi màn thêm đúng MỘT thứ mới để người chơi phải nghĩ
       khác đi, chứ không phải tăng số lượng mục tiêu.
         1. kéo–thả                  2. kích nổ giữa không trung
         3. bệ chạy ngang (canh giờ) 4. thùng nổ dây chuyền
         5. chính khẩu pháo chạy     6. đạn tách + bệ chạy vòng

       Khung nhìn: camera orthographicSize 8, tỉ lệ dọc 9:16 ⇒ x ∈ [-4.5, 4.5], y ∈ [-8, 8]. */
    public static class BlastOutLevelFactory
    {
        /* Mặt bệ nằm cao hơn tâm bệ nửa độ dày; khối và thùng đặt sao cho đáy chạm mặt bệ.
           Tính sẵn để level data ghi thẳng toạ độ, khỏi đoán bằng mắt. */
        const float PlatformHalfThickness = 0.16f;
        const float BlockHalfHeight = 0.42f;
        const float BarrelHalfHeight = 0.45f;

        public static BlastLevelConfig[] CreateAll()
        {
            return new[]
            {
                Level01(), Level02(), Level03(), Level04(), Level05(), Level06(), Level07()
            };
        }

        /* Level 1 — chỉ có kéo, thả. Một khối, một bệ, không vật cản: KHÔNG THỂ thua khi còn đạn. */
        static BlastLevelConfig Level01()
        {
            var level = Begin(1, "DRAG ANYWHERE TO AIM", new Vector2(-3.4f, -5f));
            var w = new SerializedFieldWriter(level);

            Ammo(w, AmmoType.Bomb, AmmoType.Bomb);
            var platforms = w.ArrayOf("platforms", 1);
            Platform(platforms, 0, new Vector2(1.4f, -1f), 3.2f);

            var blocks = w.ArrayOf("blocks", 1);
            Block(blocks, 0, 1.4f, -1f);

            w.ArrayOf("barrels", 0);
            return End(level, w);
        }

        /* Level 2 — hai khối tách xa nhau trên cùng một bệ. Bắn trúng từng cái thì thiếu đạn;
           nổ giữa không trung ở khoảng giữa mới hạ được cả hai. */
        static BlastLevelConfig Level02()
        {
            var level = Begin(2, "TAP AGAIN WHILE FLYING TO DETONATE", new Vector2(-3.4f, -5f));
            var w = new SerializedFieldWriter(level);

            Ammo(w, AmmoType.Bomb, AmmoType.Bomb);
            var platforms = w.ArrayOf("platforms", 1);
            Platform(platforms, 0, new Vector2(1.3f, 0.2f), 4.2f);

            var blocks = w.ArrayOf("blocks", 2);
            Block(blocks, 0, 0.1f, 0.2f);
            Block(blocks, 1, 2.5f, 0.2f);

            w.ArrayOf("barrels", 0);
            return End(level, w);
        }

        /* Level 3 — bệ chạy ngang. Đường ngắm đúng nhưng bắn sai thời điểm là trượt. */
        static BlastLevelConfig Level03()
        {
            var level = Begin(3, "TIME YOUR SHOT", new Vector2(-3.4f, -5f));
            var w = new SerializedFieldWriter(level);

            Ammo(w, AmmoType.Bomb, AmmoType.Bomb);
            var platforms = w.ArrayOf("platforms", 1);
            Platform(platforms, 0, new Vector2(0.9f, -0.6f), 2.6f, MotionKind.Horizontal, 1.7f, 0.15f);

            var blocks = w.ArrayOf("blocks", 1);
            Block(blocks, 0, 0.9f, -0.6f);

            w.ArrayOf("barrels", 0);
            return End(level, w);
        }

        /* Level 4 — một viên đạn, hai khối. Bắn thẳng vào khối là chắc chắn thua; phải nhắm thùng
           nổ và để vụ nổ dây chuyền làm phần còn lại.

           Khoảng cách khối↔thùng (2.1) cố ý nằm GIỮA hai bán kính trong blast_tuning:
             lớn hơn BlastRadius (1.9)  ⇒ nổ trúng khối KHÔNG với tới thùng, cú bắn ẩu là thua;
             nhỏ hơn BarrelRadius (3.2) ⇒ thùng nổ thì quét được cả hai khối, cú bắn đúng thì thắng.
           Đặt hai khối sát thùng hơn là level tự giải: đập vào đâu cũng kích nổ dây chuyền. */
        static BlastLevelConfig Level04()
        {
            var level = Begin(4, "ONE SHOT - HIT THE BARREL", new Vector2(-3.4f, -5f));
            var w = new SerializedFieldWriter(level);

            Ammo(w, AmmoType.Bomb);
            var platforms = w.ArrayOf("platforms", 1);
            Platform(platforms, 0, new Vector2(1.6f, -0.8f), 5.2f);

            var blocks = w.ArrayOf("blocks", 2);
            Block(blocks, 0, -0.5f, -0.8f);
            Block(blocks, 1, 3.7f, -0.8f);

            var barrels = w.ArrayOf("barrels", 1);
            Barrel(barrels, 0, 1.6f, -0.8f);

            return End(level, w);
        }

        /* Level 5 — khẩu pháo đứng trên bệ chạy. Giờ chính ĐIỂM BẮN cũng đổi, nên người chơi phải
           chọn đứng ở đâu thì bắn, không còn canh mỗi mục tiêu. */
        static BlastLevelConfig Level05()
        {
            var level = Begin(5, "THE CANNON IS MOVING", new Vector2(-2.4f, -4.3f));
            var w = new SerializedFieldWriter(level);

            /* Pháo bám bệ số 0 — bệ đầu tiên trong mảng phải là bệ đỡ pháo. */
            w.Int("launcherPlatformIndex", 0);

            Ammo(w, AmmoType.Bomb, AmmoType.Bomb);
            var platforms = w.ArrayOf("platforms", 2);
            Platform(platforms, 0, new Vector2(-2.4f, -4.8f), 2.6f, MotionKind.Horizontal, 1.5f, 0.13f);
            Platform(platforms, 1, new Vector2(1.6f, 0.4f), 3.4f);

            var blocks = w.ArrayOf("blocks", 2);
            Block(blocks, 0, 0.8f, 0.4f);
            Block(blocks, 1, 2.4f, 0.4f);

            w.ArrayOf("barrels", 0);
            return End(level, w);
        }

        /* Level 6 — đạn tách ba, một bệ chạy vòng và một bệ đứng yên. Tách sớm thì chùm đạn toả
           quá rộng, tách muộn thì không kịp với tới bệ đang quay. */
        static BlastLevelConfig Level06()
        {
            var level = Begin(6, "SPLIT SHOT - TAP TO SPLIT", new Vector2(-3.4f, -5f));
            var w = new SerializedFieldWriter(level);

            Ammo(w, AmmoType.Splitter, AmmoType.Bomb);
            var platforms = w.ArrayOf("platforms", 2);
            Platform(platforms, 0, new Vector2(2.2f, 1.6f), 2.2f, MotionKind.Circle, 1.3f, 0.11f);
            Platform(platforms, 1, new Vector2(-0.4f, -1.8f), 2.8f);

            var blocks = w.ArrayOf("blocks", 2);
            Block(blocks, 0, 2.2f, 1.6f);
            Block(blocks, 1, -1.0f, -1.8f);

            var barrels = w.ArrayOf("barrels", 1);
            Barrel(barrels, 0, 0.3f, -1.8f);

            return End(level, w);
        }

        /* Level 7 — mục tiêu cấm. Lần đầu tiên sức mạnh trở thành RỦI RO: khối cần hạ nằm ngay
           cạnh thứ không được chạm, mà bán kính nổ thì phủ cả hai nếu bắn vào giữa.

           Khoảng cách khối ↔ mục tiêu cấm (2.2) lớn hơn BlastRadius (1.9) đúng một chút: có một
           đường bắn đúng, nhưng phải nổ ở PHÍA NGOÀI của khối chứ không phải giữa hai vật. Đây là
           level đầu tiên mà "nổ càng gần càng tốt" là sai. */
        static BlastLevelConfig Level07()
        {
            var level = Begin(7, "DON'T BLAST THE BLUE ONE", new Vector2(-3.4f, -5f));
            var w = new SerializedFieldWriter(level);

            Ammo(w, AmmoType.Bomb, AmmoType.Bomb);
            var platforms = w.ArrayOf("platforms", 1);
            Platform(platforms, 0, new Vector2(1.4f, -0.6f), 4.6f);

            var blocks = w.ArrayOf("blocks", 1);
            Block(blocks, 0, 2.9f, -0.6f);

            w.ArrayOf("barrels", 0);

            var forbidden = w.ArrayOf("forbidden", 1);
            if (forbidden != null)
            {
                forbidden.GetArrayElementAtIndex(0).FindPropertyRelative("Position").vector2Value =
                    new Vector2(0.7f, -0.6f + PlatformHalfThickness + 0.5f);
            }

            return End(level, w);
        }

        static BlastLevelConfig Begin(int number, string hint, Vector2 launcher)
        {
            BlastOutAssetFactory.EnsureFolder(BlastOutAssetFactory.DataFolder);
            var path = $"{BlastOutAssetFactory.DataFolder}/level_{number:00}.asset";

            var level = AssetDatabase.LoadAssetAtPath<BlastLevelConfig>(path);
            if (!level)
            {
                level = ScriptableObject.CreateInstance<BlastLevelConfig>();
                AssetDatabase.CreateAsset(level, path);
            }

            var writer = new SerializedFieldWriter(level)
                .Int("displayNumber", number)
                .Text("hint", hint)
                .Vec2("launcherPosition", launcher)
                .Int("launcherPlatformIndex", -1)
                .Float("collectZoneTopY", -6.4f);

            /* Dọn sạch mảng mục tiêu cấm ngay từ đầu: asset được ghi đè chứ không tạo mới, nên level
               nào không khai báo lại sẽ giữ nguyên dữ liệu của lần sinh trước. */
            writer.ArrayOf("forbidden", 0);
            writer.Apply();

            return level;
        }

        static BlastLevelConfig End(BlastLevelConfig level, SerializedFieldWriter writer)
        {
            writer.Apply();
            EditorUtility.SetDirty(level);
            return level;
        }

        static void Ammo(SerializedFieldWriter writer, params AmmoType[] types)
        {
            var array = writer.ArrayOf("ammo", types.Length);
            if (array == null) return;

            for (var i = 0; i < types.Length; i++)
            {
                array.GetArrayElementAtIndex(i).enumValueIndex = (int)types[i];
            }
        }

        static void Platform(SerializedProperty array, int index, Vector2 center, float width,
            MotionKind kind = MotionKind.Static, float distance = 0f, float speed = 0f, float phase = 0f)
        {
            if (array == null) return;

            var element = array.GetArrayElementAtIndex(index);
            element.FindPropertyRelative("Center").vector2Value = center;
            element.FindPropertyRelative("Width").floatValue = width;

            var motion = element.FindPropertyRelative("Motion");
            motion.FindPropertyRelative("kind").enumValueIndex = (int)kind;
            motion.FindPropertyRelative("distance").floatValue = distance;
            motion.FindPropertyRelative("speed").floatValue = speed;
            motion.FindPropertyRelative("phase").floatValue = phase;
        }

        /* platformCenterY là tâm bệ; khối/thùng tự đặt sao cho đáy vừa chạm mặt bệ. */
        static void Block(SerializedProperty array, int index, float x, float platformCenterY)
        {
            if (array == null) return;
            array.GetArrayElementAtIndex(index).FindPropertyRelative("Position").vector2Value =
                new Vector2(x, platformCenterY + PlatformHalfThickness + BlockHalfHeight);
        }

        static void Barrel(SerializedProperty array, int index, float x, float platformCenterY)
        {
            if (array == null) return;
            array.GetArrayElementAtIndex(index).FindPropertyRelative("Position").vector2Value =
                new Vector2(x, platformCenterY + PlatformHalfThickness + BarrelHalfHeight);
        }
    }
}
