using Dacodelaac.Attributes;
using DG.Tweening;
using UnityEngine;

namespace Dacodelaac.UI.Layouts
{
    public class VerticalLayoutItem : MonoBehaviour
    {
        [SerializeField, GUID] string tweenId;
        [SerializeField] private float extraHeight;

        public float ExtraHeight => extraHeight;
        public int Index { get; set; }
        public float Height { get; set; }
        public bool Showing { get; set; }

        VerticalLayoutGroupCustom group;
        RectTransform rt;
        float targetY;

        string TweenIdScale => "TweenIdScale" + tweenId;
        string TweenIdY => "TweenIdY" + tweenId;

        public void Init()
        {
            if (!group)
            {
                group = GetComponentInParent<VerticalLayoutGroupCustom>();
            }

            group.Init();
        }

        public void Setup()
        {
            targetY = 0;
            rt = GetComponent<RectTransform>();

            Height = rt.rect.height + extraHeight;
            targetY = rt.anchoredPosition.y;

            Showing = true;
        }

        public void Toggle(bool show, bool animated, bool groupAnimated, System.Action onCompleted = null)
        {
            Init();

            if (show)
            {
                Show(animated, groupAnimated, onCompleted);
            }
            else
            {
                Hide(animated, groupAnimated, onCompleted);
            }
        }

        public void Show(bool animated, bool groupAnimated, System.Action onCompleted = null)
        {
            Init();

            if (Showing) return;
            Showing = true;

            Scaling(Vector3.one, animated, onCompleted);
            group.Show(this, groupAnimated);
        }

        public void Hide(bool animated, bool groupAnimated, System.Action onCompleted = null)
        {
            Init();

            if (!Showing) return;
            Showing = false;

            Scaling(Vector3.zero, animated, onCompleted);
            group.Hide(this, groupAnimated);
        }

        public void Scaling(Vector3 scale, bool animated, System.Action onCompleted)
        {
            Init();

            DOTween.Kill(TweenIdScale);
            if (animated)
            {
                rt.DOScale(scale, 0.75f).SetTarget(TweenIdScale).OnComplete(() => { onCompleted?.Invoke(); });
            }
            else
            {
                rt.transform.localScale = scale;
            }
        }

        public void MoveY(float y, bool animated)
        {
            Init();

            targetY += y;
            DOTween.Kill(TweenIdY);
            if (animated)
            {
                rt.DOAnchorPosY(targetY, 0.75f).SetTarget(TweenIdY);
            }
            else
            {
                rt.anchoredPosition = new Vector2(rt.anchoredPosition.x, targetY);
            }
        }

        [ContextMenu("Show")]
        public void Show()
        {
            Toggle(true, true, true);
        }

        [ContextMenu("Hide")]
        public void Hide()
        {
            Toggle(false, true, true);
        }
    }
}