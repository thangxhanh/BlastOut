using System.Linq;
using Dacodelaac.Core;
using Dacodelaac.Utils;
using UnityEngine;
#if UNITY_EDITOR
using UnityEditor;
#endif

namespace Dacodelaac.LevelSystem
{
    [CreateAssetMenu(menuName = "LevelSystem/LevelData")]
    public class LevelData : BaseSO
    {
        [SerializeField] GameObject levelPrefab;

        public int Index { get; private set; }
        public int DisplayIndex { get; set; }
        public GameObject LevelPrefab => levelPrefab;

        public void Initialize(int index)
        {
            Index = index;
            DisplayIndex = index;
        }

#if UNITY_EDITOR
        [ContextMenu("Find Prefab")]
        public void FindPrefab()
        {
            var levels = AssetUtils.FindAssetAtFolder<GameObject>(new string[] {"Assets/Levels", "Assets/Levels-Trinh"});
            levelPrefab = levels.FirstOrDefault(l => l.name.StartsWith(name));
            EditorUtility.SetDirty(this);
        }
#endif
    }
}