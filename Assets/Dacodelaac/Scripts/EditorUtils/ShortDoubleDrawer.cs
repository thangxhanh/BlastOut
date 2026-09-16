using Dacodelaac.DataType;
using UnityEditor;
using UnityEngine;

namespace Dacodelaac.Scripts.EditorUtils
{
    [CustomPropertyDrawer(typeof(ShortDouble))]
    public class ShortDoubleDrawer : PropertyDrawer
    {
        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var indent = EditorGUI.indentLevel;
            EditorGUI.indentLevel = 0;
            EditorGUI.PropertyField(position, property.FindPropertyRelative("value"), GUIContent.none);
            EditorGUI.indentLevel = indent;

            EditorGUI.EndProperty();
        }
    }
}
