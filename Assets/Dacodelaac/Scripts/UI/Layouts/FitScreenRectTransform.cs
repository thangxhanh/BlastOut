using System;
using Dacodelaac.DebugUtils;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;

namespace Dacodelaac.UI.Layouts
{
    public class FitScreenRectTransform : MonoBehaviour
    {
        [SerializeField] Vector2 refSize;

        void Start()
        {
            Fit();
        }

        [ContextMenu("Fit")]
        public void Fit()
        {
            var canvas = GetComponentInParent<Canvas>();
            var canvasRect = canvas.GetComponent<RectTransform>();
            var rect = GetComponent<RectTransform>();

            var sizeDelta = canvasRect.sizeDelta;
            var canvasAspect = sizeDelta.x / sizeDelta.y;
            var rectAspect = refSize.x / refSize.y;

            if (rectAspect >= canvasAspect)
            {
                rect.sizeDelta = new Vector2(sizeDelta.y * rectAspect, sizeDelta.y);
            }
            else
            {
                rect.sizeDelta = new Vector2(sizeDelta.x, sizeDelta.x / rectAspect);
            }
#if UNITY_EDITOR
            EditorUtility.SetDirty(rect);
#endif
        }
    }
}