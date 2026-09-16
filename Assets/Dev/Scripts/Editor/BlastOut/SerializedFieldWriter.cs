using System;
using UnityEditor;
using UnityEngine;

namespace Dev.Scripts.BlastOut.Authoring
{
    /* Mọi field trong BlastOut đều là [SerializeField] private (CLAUDE.md §9.5), nên tool dựng
       scene không gán thẳng được — phải đi qua SerializedObject. Class này gói lại để chỗ gọi
       đọc như gán field thường.

       Tên method cố ý KHÔNG trùng tên type (Vec2/Tint/EnumIndex/ArrayOf thay vì Vector2/Color/
       Enum/Array): member trùng tên type gây nhập nhằng khi chính tên đó xuất hiện ở vị trí type
       ngay trong class này.

       Sai tên field thì báo lỗi rõ ràng rồi bỏ qua, không ném exception giữa chừng làm hỏng dở
       cả scene đang dựng. */
    public class SerializedFieldWriter
    {
        readonly SerializedObject target;

        public SerializedFieldWriter(UnityEngine.Object obj)
        {
            target = new SerializedObject(obj);
        }

        public SerializedFieldWriter Ref(string f, UnityEngine.Object v) => Set(f, p => p.objectReferenceValue = v);
        public SerializedFieldWriter Float(string f, float v) => Set(f, p => p.floatValue = v);
        public SerializedFieldWriter Int(string f, int v) => Set(f, p => p.intValue = v);
        public SerializedFieldWriter Bool(string f, bool v) => Set(f, p => p.boolValue = v);
        public SerializedFieldWriter Text(string f, string v) => Set(f, p => p.stringValue = v);
        public SerializedFieldWriter Vec2(string f, Vector2 v) => Set(f, p => p.vector2Value = v);
        public SerializedFieldWriter Tint(string f, Color v) => Set(f, p => p.colorValue = v);
        public SerializedFieldWriter EnumIndex(string f, int v) => Set(f, p => p.enumValueIndex = v);

        public SerializedFieldWriter Refs(string f, params UnityEngine.Object[] values)
        {
            return Set(f, p =>
            {
                p.arraySize = values.Length;
                for (var i = 0; i < values.Length; i++)
                {
                    p.GetArrayElementAtIndex(i).objectReferenceValue = values[i];
                }
            });
        }

        /* Trả về property để chỗ gọi tự điền từng phần tử struct (Center/Width...). */
        public SerializedProperty ArrayOf(string field, int size)
        {
            var property = Find(field);
            if (property != null) property.arraySize = size;
            return property;
        }

        public void Apply()
        {
            target.ApplyModifiedPropertiesWithoutUndo();
        }

        SerializedFieldWriter Set(string field, Action<SerializedProperty> write)
        {
            var property = Find(field);
            if (property != null) write(property);
            return this;
        }

        SerializedProperty Find(string field)
        {
            var property = target.FindProperty(field);
            if (property == null)
            {
                Debug.LogError($"[BlastOut] Không tìm thấy field '{field}' trên {target.targetObject}. " +
                               "Đổi tên field trong script thì phải sửa cả tool dựng scene.");
            }

            return property;
        }
    }
}
