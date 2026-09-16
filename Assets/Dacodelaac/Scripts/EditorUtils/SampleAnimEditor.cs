using UnityEditor;

namespace Dacodelaac.Scripts.EditorUtils
{
    [CustomEditor(typeof(SampleAnim))]
    public class SampleAnimEditor : UnityEditor.Editor
    {
        SampleAnim sampleAnim;
        int frame;

        void OnEnable()
        {
            sampleAnim = target as SampleAnim;
        }

        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (sampleAnim.Clip != null)
            {
                var pos = sampleAnim.transform.position;
                var rot = sampleAnim.transform.rotation;
                var scale = sampleAnim.transform.localScale;

                var frames = (int)(sampleAnim.Clip.length * sampleAnim.Clip.frameRate);
                frame = EditorGUILayout.IntSlider("Frame", frame, 0, frames);
                var t = frame / sampleAnim.Clip.frameRate;
                sampleAnim.Clip.SampleAnimation(sampleAnim.gameObject, t);

                sampleAnim.transform.position = pos;
                sampleAnim.transform.rotation = rot;
                sampleAnim.transform.localScale = scale;
            }
        }
    }
}
