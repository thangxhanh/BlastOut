using System;
using Dacodelaac.Events;
using DG.Tweening;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Event = Dacodelaac.Events.Event;

namespace Dacodelaac.UI.Buttons
{
    public class RippleButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] Ease ease = Ease.OutQuint;
        [SerializeField] float scale = 0.9f;
        [SerializeField] bool hold = false;
        [SerializeField] private bool isInteractable = true;
        [SerializeField] UnityEvent m_OnClick;
        [SerializeField] Event buttonClickEvent;

        Vector3 originScale;
        bool clicking = false;

        private void Start()
        {
            originScale = transform.localScale;
        }

        void OnEnable()
        {
            clicking = false;
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isInteractable) return;
            
            buttonClickEvent.Raise();
            clicking = true;
            DoScale();
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            if (!hold && isInteractable)
            {
                m_OnClick.Invoke();
            }
            clicking = false;
            ResetScale();
        }

        void DoScale()
        {
            DOTween.Kill(this);
            transform.DOScale(originScale * scale, 0.15f).SetEase(ease).SetUpdate(true).SetTarget(this);
        }

        void ResetScale()
        {
            DOTween.Kill(this);
            transform.localScale = originScale;
        }

        void Update()
        {
            if (hold && clicking && isInteractable)
            {
                m_OnClick.Invoke();   
            }
        }
    }
}