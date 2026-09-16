using Dacodelaac.EditorUtils;
using Dacodelaac.LevelSystem;
using Dacodelaac.Utils;
using UnityEditor;
using UnityEngine;

namespace Dacodelaac.Scripts.EditorUtils
{
    [CustomEditor(typeof(LevelData))]
    [CanEditMultipleObjects]
    public class LevelDataEditor : Editor
    {
        public override void OnInspectorGUI()
        {
            base.OnInspectorGUI();
            if (!Application.isPlaying && GUILayout.Button("PLAY"))
            {
                var levelManager = AssetUtils.FindAssetAtFolder<LevelManager>(new[] {"Assets/Data/Levels"});
                if (levelManager.Length > 0)
                {
                    levelManager[0].SetCurrentLevel(target as LevelData);
                    SceneMenu.PlayLauncherScene();
                }
            }
        }
    }
}
