using Dacodelaac.Events;
using UnityEditor;
using UnityEngine;

namespace Dacodelaac.Scripts.EditorUtils
{
    [CustomEditor(typeof(BaseEvent), true)]
    public class BaseEventEditor : Editor
    {
        BaseEvent baseEvent;

        void OnEnable()
        {
            baseEvent = target as BaseEvent;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (GUILayout.Button("Raise"))
            {
                baseEvent.Raise();
            }
        }
    }
}
