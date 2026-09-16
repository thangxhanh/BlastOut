using System;

using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Dacodelaac.UI.SwipeUI
{
    public class BaseSwiper : ScrollRect
    {
        int currentIndex = -1;
        public int CurrentIndex
        {
            get => currentIndex;
            set
            {
                if (currentIndex != value)
                {
                    currentIndex = value;
                    if (pages == null) return;
                    for (var i = 0; i < pages.Length; i++)
                    {
                        if (currentIndex == i)
                        {
                            labels[i].OnBeginShow();
                            pages[i].OnBeginShow();    
                        }
                        else
                        {
                            labels[i].OnBeginDismiss();
                            pages[i].OnBeginDismiss();   
                        }
                    }
                }
            }
        }

        public BasePage CurrentPage => pages[CurrentIndex];
        
        BasePageLabel[] labels;
        BasePage[] pages;
        
        float step;
        Vector2 targetNormalizedPosition;

        public virtual void Initialize()
        {
            labels = GetComponentsInChildren<BasePageLabel>();
            pages = GetComponentsInChildren<BasePage>();
            
            step = pages.Length > 1 ? 1f / (pages.Length - 1) : 0;
            
            for(var i = 0; i < pages.Length; i++)
            {
                pages[i].Position = horizontal ? new Vector2(i * step, 0) : new Vector2(0, i * step);
            }

            targetNormalizedPosition = Vector2.zero;
            CurrentIndex = 0;
        }

        public override void OnDrag(PointerEventData eventData)
        {
            base.OnDrag(eventData);
            CurrentIndex = CalculateCurrentIndex();
            targetNormalizedPosition = normalizedPosition;
        }

        public override void OnEndDrag(PointerEventData eventData)
        {
            base.OnEndDrag(eventData);
            CurrentIndex = CalculateCurrentIndex();
            targetNormalizedPosition = pages[CurrentIndex].Position;
        }

        protected override void LateUpdate()
        {
            if (pages != null && pages.Length > 1 && targetNormalizedPosition != normalizedPosition)
            {
                CurrentIndex = CalculateCurrentIndex();
                normalizedPosition = Vector2.Lerp(normalizedPosition, targetNormalizedPosition, Time.deltaTime * 10);
            }
            else
            {
                base.LateUpdate();   
            }
        }

        public int CalculateCurrentIndex()
        {
            if (step <= 0) return 0;
            var pos = normalizedPosition;
            return Mathf.Clamp(Mathf.RoundToInt(horizontal ? (pos.x / step) : (pos.y / step)), 0, pages.Length - 1);
        }
    }
}