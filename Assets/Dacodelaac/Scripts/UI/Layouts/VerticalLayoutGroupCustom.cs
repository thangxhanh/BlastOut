using System;
using System.Linq;
using UnityEditor;
using UnityEngine;

namespace Dacodelaac.UI.Layouts
{
    public class VerticalLayoutGroupCustom : MonoBehaviour
    {
        [SerializeField] bool upDown = true;
        [SerializeField] float spacing;
        VerticalLayoutItem[] items;

        bool initialized = false;

        void Awake()
        {
            Init();
        }

        public void Init()
        {
            if (initialized) return;
            initialized = true;

            items = GetComponentsInChildren<VerticalLayoutItem>();
            if (!upDown)
            {
                items = items.Reverse().ToArray();
            }

            for (var i = 0; i < items.Length; i++)
            {
                items[i].Index = i;
                items[i].Setup();
            }
        }

        public void Show(VerticalLayoutItem item, bool animated)
        {
            Init();

            for (var i = item.Index + 1; i < items.Length; i++)
            {
                items[i].MoveY((upDown ? -1 : 1) * (item.Height + spacing), animated);
            }
        }

        public void Hide(VerticalLayoutItem item, bool animated)
        {
            Init();

            for (var i = item.Index + 1; i < items.Length; i++)
            {
                items[i].MoveY((upDown ? 1 : -1) * (item.Height + spacing), animated);
            }
        }

#if UNITY_EDITOR
        [ContextMenu("Expand")]
        public void Expand()
        {
            items = GetComponentsInChildren<VerticalLayoutItem>();
            if (!upDown)
            {
                items = items.Reverse().ToArray();
            }

            if (items.Length == 0) return;

            var rt = items[0].GetComponent<RectTransform>();
            rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, (rt.rect.height + items[0].ExtraHeight) * 0.5f);
            EditorUtility.SetDirty(rt);

            for (var i = 1; i < items.Length; i++)
            {
                var rt0 = items[i - 1].GetComponent<RectTransform>();
                rt = items[i].GetComponent<RectTransform>();
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x,
                    rt0.anchoredPosition.y + (rt0.rect.height + items[i - 1].ExtraHeight) * 0.5f + spacing +
                    (rt.rect.height + items[i].ExtraHeight) * 0.5f);

                EditorUtility.SetDirty(rt);
            }

            if (upDown)
            {
                for (var i = 0; i < items.Length; i++)
                {
                    rt = items[i].GetComponent<RectTransform>();
                    rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, -rt.anchoredPosition.y);

                    EditorUtility.SetDirty(rt);
                }
            }
        }
#endif
    }
}