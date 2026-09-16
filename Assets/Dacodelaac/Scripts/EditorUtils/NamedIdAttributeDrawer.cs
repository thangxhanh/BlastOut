using Dacodelaac.Attributes;
using Dacodelaac.EditorUtils;
using UnityEditor;
using UnityEngine;

namespace Dacodelaac.Scripts.EditorUtils
{
    [CustomPropertyDrawer(typeof(NamedIdAttribute))]
    public class NamedIdAttributeDrawer : PropertyDrawer
    {
        NamedIdAttribute TargetAttribute => attribute as NamedIdAttribute;

        public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
        {
            EditorGUI.BeginProperty(position, label, property);

            Context(position, property);

            position = EditorGUI.PrefixLabel(position, GUIUtility.GetControlID(FocusType.Passive), label);

            var id = property.stringValue;
            if (string.IsNullOrEmpty(id))
            {
                id = NamedIdAttribute.ToSnakeCase(property.serializedObject.targetObject.name);
            }

            using (new EditorGUIUtils.DisabledGUI(true))
            {
                property.stringValue = EditorGUI.TextField(position, id);
            }

            property.serializedObject.ApplyModifiedProperties();

            EditorGUI.EndProperty();
        }

        void Context(Rect rect, SerializedProperty property)
        {
            var current = Event.current;

            if (rect.Contains(current.mousePosition) && current.type == EventType.ContextClick)
            {
                var menu = new GenericMenu();

                menu.AddItem(new GUIContent("Reset"), false,
                    () =>
                    {
                        property.stringValue = NamedIdAttribute.ToSnakeCase(property.serializedObject.targetObject.name);
                        property.serializedObject.ApplyModifiedProperties();
                    });
                menu.ShowAsContext();

                current.Use();
            }
        }
    }
}
