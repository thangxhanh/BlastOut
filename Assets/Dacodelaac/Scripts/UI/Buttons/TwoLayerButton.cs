using System;
using System.Collections;
using Dacodelaac.Core;
using Dacodelaac.Events;
using DG.Tweening;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
#if UNITY_EDITOR
using UnityEditor;

#endif

namespace Dacodelaac.UI.Buttons
{
    public class TwoLayerButton : MonoBehaviour, IPointerDownHandler, IPointerUpHandler
    {
        [SerializeField] Ease ease = Ease.OutQuint;
        [SerializeField] Image front;
        [SerializeField] Vector2 offset;
        [SerializeField] Type type;
        [SerializeField] bool hold = false;
        [SerializeField] private bool isInteractable = true;
        [SerializeField] Button.ButtonClickedEvent m_OnClick;

        enum Type
        {
            Vertical,
            Horizontal
        }

        RectTransform FrontRect => front ? front.rectTransform : null;

        bool clicking;
        Coroutine holdRoutine;

        void OnEnable()
        {
            clicking = false;
        }

        void OnDisable()
        {
            StopHold();
        }

        public void OnPointerDown(PointerEventData eventData)
        {
            if (!isInteractable) return;

            // globalSfx.PlayButtonClick();
            clicking = true;
            DoOffset();

            /* Chỉ chạy vòng lặp mỗi frame trong lúc đang giữ, thay vì Update() thường trực
               trên mọi button chỉ để kiểm tra một điều kiện gần như luôn sai. */
            if (hold) holdRoutine = StartCoroutine(HoldLoop());
        }

        public void OnPointerUp(PointerEventData eventData)
        {
            StopHold();

            if (!hold && isInteractable)
            {
                m_OnClick.Invoke();
            }

            clicking = false;
            ResetOffset();
        }

        void StopHold()
        {
            if (holdRoutine == null) return;

            StopCoroutine(holdRoutine);
            holdRoutine = null;
        }

        IEnumerator HoldLoop()
        {
            while (clicking && isInteractable)
            {
                m_OnClick.Invoke();
                yield return null;
            }

            holdRoutine = null;
        }

        void DoOffset()
        {
            DOTween.Kill(this);
            var targetPos = type == Type.Vertical ? new Vector2(offset.x, 0) : new Vector2(0, offset.y);
            FrontRect.DOAnchorPos(targetPos, 0.15f).SetEase(ease).SetUpdate(true).SetTarget(this);
        }

        void ResetOffset()
        {
            if (!FrontRect) return;

            DOTween.Kill(this);
            FrontRect.anchoredPosition = offset;
        }

#if UNITY_EDITOR
        void OnValidate()
        {
            ResetOffset();
        }
#endif
    }
}