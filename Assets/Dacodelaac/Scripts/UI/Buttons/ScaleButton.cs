using Dacodelaac.Core;
using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;
using Event = Dacodelaac.Events.Event;

namespace Dacodelaac.UI.Buttons
{
    public class ScaleButton : BaseMono
    {
        [SerializeField] private Ease ease = Ease.OutQuint;
        [SerializeField] private float scale = 0.9f;
        [SerializeField] private Button button;
        [SerializeField] private Button.ButtonClickedEvent m_OnClick;
        [SerializeField] private Event buttonClickEvent;

        private Vector3 originScale = Vector3.one;
        private bool isClick = false;

        public override void DoEnable()
        {
            base.DoEnable();
            if (button != null) button.onClick.AddListener(DoScale);
        }

        public override void DoDisable()
        {
            base.DoDisable();
            if (button != null) button.onClick.RemoveListener(DoScale);
            DOTween.Kill(this);
        }

        private void OnValidate()
        {
            if (button != null) button.onClick.RemoveListener(DoScale);
            button = gameObject.GetComponent<Button>();
        }

        private void DoScale()
        {
            if (isClick) return;
            isClick = true;
            
            DOTween.Kill(this);
            DOTween.Sequence()
                .Append(transform.DOScale(originScale * scale, 0.15f)
                    .SetEase(ease)
                    .SetUpdate(true)
                    .SetTarget(this))
                .Append(transform.DOScale(originScale, 0.1f)
                    .SetEase(ease)
                    .SetUpdate(true)
                    .SetTarget(this)
                    .OnComplete(() =>
                    {
                        m_OnClick?.Invoke();
                        buttonClickEvent.Raise();
                        isClick = false;
                    }))
                .Play();
        }
    }
}