using System.Collections;
using System.Collections.Generic;
using Dacodelaac.Core;
using DG.Tweening;
using UnityEngine;

namespace Dev.Utils{
    
    public class ZoomEffect : BaseMono
    {
        [SerializeField] private float startScale = 0f; // Kích thước ban đầu
        [SerializeField] private float maxScale = 1.5f; // Kích thước phóng to
        [SerializeField] private float endScale = 1f; // Kích thước cuối cùng
        [SerializeField] private float scaleUpDuration = 0.15f; // Thời gian phóng to
        [SerializeField] private float scaleDownDuration = 0.1f; // Thời gian thu nhỏ

        [SerializeField] private Ease scaleUpEase = Ease.OutQuad; // Hiệu ứng phóng to
        [SerializeField] private Ease scaleDownEase = Ease.InQuad; // Hiệu ứng thu nhỏ
        public override void DoEnable()
        {
            base.DoEnable();

            transform.localScale = Vector3.one * startScale;
            StartZoomEffect(maxScale);
        }

        public void StartZoomEffect(float MaxScale)
        {
            transform.DOScale(MaxScale, scaleUpDuration)
                .SetEase(scaleUpEase)
                .OnComplete(() =>
                {
                    transform.DOScale(endScale, scaleDownDuration)
                        .SetEase(scaleDownEase);
                });
        }
    }
    
}
