using Dacodelaac.UI.SwipeUI;
using UnityEditor;
using UnityEditor.UI;

namespace Dacodelaac.Scripts.EditorUtils
{
    [CustomEditor(typeof(BaseSwiper))]
    public class BaseSwiperEditor : ScrollRectEditor
    {
        public override void OnInspectorGUI()
        {
            DrawDefaultInspector();
        }
    }
}
