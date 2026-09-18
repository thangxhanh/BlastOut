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
         7. mục tiêu cấm chạm

       Hai màn cuối KHÔNG dạy gì mới — chúng bắt dùng hai thứ đã học cùng lúc, và chỉ ở đó người
       chơi mới phải cân nhắc đánh đổi thay vì áp dụng một quy tắc:
         8. bệ chạy + mục tiêu cấm   9. thùng dây chuyền + mục tiêu cấm

       Khung nhìn: camera orthographicSize 8, tỉ lệ dọc 9:16 ⇒ x ∈ [-4.5, 4.5], y ∈ [-8, 8]. */
    public static class BlastOutLevelFactory
    {
        /* Mặt bệ nằm cao hơn tâm bệ nửa độ dày; khối và thùng đặt sao cho đáy chạm mặt bệ.
           Tính sẵn để level data ghi thẳng toạ độ, khỏi đoán bằng mắt. */
        const float PlatformHalfThickness = 0.16f;
        const float BlockHalfHeight = 0.42f;
        const float BarrelHalfHeight = 0.45f;
        /* Bằng khối mục tiêu: mục tiêu cấm giờ dùng chung sprite và cùng cỡ, chỉ khác màu. */
        const float ForbiddenHalfHeight = BlockHalfHeight;

        /* Sinh lại TOÀN BỘ level, ghi đè mọi chỉnh tay. Chỉ dùng qua lệnh menu riêng, KHÔNG gọi từ
           lệnh dựng scene — level chỉnh bằng Scene View là công sức cân bằng thật, mà dựng scene
           lại là việc hay làm, nên gộp hai thứ vào một lệnh thì sớm muộn cũng xoá nhầm. */
        [MenuItem("Tools/Blast Out/Regenerate All Levels (ghi đè chỉnh tay)", false, 20)]
        public static void RegenerateAll()
        {
            if (!EditorUtility.DisplayDialog("Sinh lại toàn bộ level?",
                    "Mọi chỉnh sửa bằng tay trên 9 level sẽ bị ghi đè bằng bố cục gốc trong code.",
                    "Ghi đè", "Huỷ"))
            {
                return;
            }

            CreateAll();
            AssetDatabase.SaveAssets();
            Debug.Log("[BlastOut] Đã sinh lại 9 level từ code.");
        }

        public static BlastLevelConfig[] CreateAll()
        {
            return new[]
            {
                Level01(), Level02(), Level03(), Level04(), Level05(), Level06(), Level07(),
                Level08(), Level09()
            };
        }

        /* Đọc level đã có trên đĩa; thiếu cái nào thì mới sinh cái đó. Lệnh dựng scene dùng hàm này
           nên chạy bao nhiêu lần cũng không đụng tới bố cục đã chỉnh. */
        public static BlastLevelConfig[] LoadOrCreateAll()
        {
            var levels = new BlastLevelConfig[LevelCount];
            var missing = 0;

            for (var i = 0; i < LevelCount; i++)
            {
                var path = $"{BlastOutAssetFactory.DataFolder}/level_{i + 1:00}.asset";
                levels[i] = AssetDatabase.LoadAssetAtPath<BlastLevelConfig>(path);
                if (!levels[i]) missing++;
            }

            if (missing == 0) return levels;

            Debug.Log($"[BlastOut] Thiếu {missing} level trên đĩa — sinh lại từ code.");
            return CreateAll();
        }

        const int LevelCount = 9;

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
            var level = Begin(5, "THE CANNON IS MOVING", new Vector2(-2.1f, -4.3f));
            var w = new SerializedFieldWriter(level);

            /* Pháo bám bệ số 0 — bệ đầu tiên trong mảng phải là bệ đỡ pháo. */
            w.Int("launcherPlatformIndex", 0);

            Ammo(w, AmmoType.Bomb, AmmoType.Bomb);
            var platforms = w.ArrayOf("platforms", 2);

            /* Mép ngoài cùng của bệ này = |tâm| + nửa rộng + biên độ = 2.1 + 1.3 + 1.3 = 4.7, nằm
               trong requiredHalfWidth (4.9). Bản trước để 2.4/1.5 cho ra 5.2 nên bệ bị cắt mất một
               đoạn ngay ở tỉ lệ 9:16 — mà đây lại là bệ chở khẩu pháo. */
            Platform(platforms, 0, new Vector2(-2.1f, -4.8f), 2.6f, MotionKind.Horizontal, 1.3f, 0.13f);
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

           Khoảng cách khối ↔ mục tiêu cấm (3.0) lớn hơn hẳn BlastRadius (1.9), và khối nằm sát mép
           phải nên chỉ cần đẩy nhẹ là rơi. Đây là level đầu tiên mà "nổ càng gần càng tốt" là sai.

           Cửa sổ nổ hợp lệ bị kẹp giữa HAI ràng buộc ngược nhau, và phải kiểm lại bằng số mỗi khi
           đổi bố cục:
             cận dưới — xa mục tiêu cấm hơn BlastRadius, tức x > 0.2 + 1.9 = 2.1;
             cận trên — cách khối ít nhất 0.25 (dưới ngưỡng đó TargetBlock hất THẲNG LÊN thay vì
               sang ngang, khối rơi lại đúng chỗ cũ), tức x < 3.2 - 0.25 = 2.95.
           Bản đầu tiên để khối ở 2.9 và mục tiêu cấm ở 0.7: hai cận chồng lên nhau chỉ còn 0.05
           unit — level trông hợp lý nhưng không ai thắng được. */
        static BlastLevelConfig Level07()
        {
            var level = Begin(7, "DON'T BLAST THE GREEN ONE", new Vector2(-3.4f, -5f));
            var w = new SerializedFieldWriter(level);

            Ammo(w, AmmoType.Bomb, AmmoType.Bomb);
            var platforms = w.ArrayOf("platforms", 1);
            Platform(platforms, 0, new Vector2(1.4f, -0.6f), 4.6f);

            var blocks = w.ArrayOf("blocks", 1);
            Block(blocks, 0, 3.2f, -0.6f);

            w.ArrayOf("barrels", 0);

            Forbidden(w.ArrayOf("forbidden", 1), 0, 0.2f, -0.6f);
            return End(level, w);
        }

        /* Level 8 — TỔNG HỢP: bệ chạy (L3) gặp mục tiêu cấm (L7).

           Khối đi qua đi lại, và có lúc nó ở gần mục tiêu cấm tới mức không thể bắn. Đường ngắm
           đúng không còn đủ, thời điểm đúng cũng không còn đủ — phải là thời điểm AN TOÀN.
           Bệ chạy trong khoảng x ∈ [-0.9, 2.5]; mục tiêu cấm đứng yên ở 3.6, nên càng về bên phải
           thì cửa sổ bắn càng hẹp lại. */
        static BlastLevelConfig Level08()
        {
            var level = Begin(8, "WAIT FOR THE SAFE MOMENT", new Vector2(-3.4f, -5f));
            var w = new SerializedFieldWriter(level);

            Ammo(w, AmmoType.Bomb, AmmoType.Bomb);
            var platforms = w.ArrayOf("platforms", 2);
            Platform(platforms, 0, new Vector2(0.8f, -0.6f), 2.6f, MotionKind.Horizontal, 1.7f, 0.13f);

            /* Bệ đỡ mục tiêu cấm phải nằm NGOÀI tầm với của bệ chạy, nếu không hai bệ lồng vào nhau
               ở biên phải và nhìn ra một khối dính liền:
                 mép phải bệ chạy = 0.8 + 1.3 + 1.7 = 3.8
                 mép trái bệ này  = 4.3 - 0.4       = 3.9
               Bản trước để 3.6 / rộng 1.4 nên mép trái chỉ 2.90 — chồng lên nhau 0.90 unit. */
            Platform(platforms, 1, new Vector2(4.3f, -0.6f), 0.8f);

            Block(w.ArrayOf("blocks", 1), 0, 0.8f, -0.6f);
            w.ArrayOf("barrels", 0);
            Forbidden(w.ArrayOf("forbidden", 1), 0, 4.3f, -0.6f);

            return End(level, w);
        }

        /* Level 9 — TỔNG HỢP: thùng nổ dây chuyền (L4) gặp mục tiêu cấm (L7).

           Hai thùng nổ, nhìn y hệt nhau. Thùng trái hạ được cả hai khối và ở đủ xa mục tiêu cấm;
           thùng phải thì bán kính của nó trùm luôn mục tiêu cấm — bắn nhầm là thua ngay.

           Ba số liệu phải đồng thời đúng, nếu không quyết định biến mất:
             hai thùng cách nhau 3.6 > BarrelRadius 3.2 ⇒ thùng trái KHÔNG kích nổ thùng phải,
               nếu kích thì mọi đường bắn đều dẫn tới cùng một kết cục. Chừa hẳn một khoảng dư
               thay vì bám sát ngưỡng: thùng là vật thể động, chỉ cần nhích vài phần mười là
               dây chuyền xảy ra ngoài ý muốn;
             MỘT viên đạn ⇒ không thể bỏ qua thùng mà bắn thẳng từng khối. Thử với hai viên thì
               người chơi giải xong mà chẳng cần nhìn tới cái thùng nào;
             bệ đủ ngắn ⇒ vụ nổ của thùng trái hất được CẢ HAI khối ra khỏi mép. Bệ rộng quá thì
               khối xa chỉ trượt một đoạn rồi nằm lại, và level thành không thể thắng. */
        static BlastLevelConfig Level09()
        {
            var level = Begin(9, "ONE SHOT - PICK THE RIGHT BARREL", new Vector2(-3.4f, -5f));
            var w = new SerializedFieldWriter(level);

            Ammo(w, AmmoType.Bomb);
            var platforms = w.ArrayOf("platforms", 2);
            Platform(platforms, 0, new Vector2(0f, -0.8f), 5f);

            /* Bệ của mục tiêu cấm phải nằm CAO hơn hẳn bệ chính. Để ngang nhau thì khối bị hất sang
               phải đáp luôn lên nó và nằm lại đó — không rơi xuống được, mà chỉ có một viên đạn,
               nên level thành không thể thắng. */
            Platform(platforms, 1, new Vector2(3.6f, 1f), 1.4f);

            var blocks = w.ArrayOf("blocks", 2);
            Block(blocks, 0, -1.8f, -0.8f);
            Block(blocks, 1, 0.4f, -0.8f);

            var barrels = w.ArrayOf("barrels", 2);
            Barrel(barrels, 0, -1.2f, -0.8f);
            Barrel(barrels, 1, 2.4f, -0.8f);

            Forbidden(w.ArrayOf("forbidden", 1), 0, 3.6f, 1f);

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

        static void Forbidden(SerializedProperty array, int index, float x, float platformCenterY)
        {
            if (array == null) return;
            array.GetArrayElementAtIndex(index).FindPropertyRelative("Position").vector2Value =
                new Vector2(x, platformCenterY + PlatformHalfThickness + ForbiddenHalfHeight);
        }
    }
}
