using Dacodelaac.Core;
using DG.Tweening;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dacodelaac.Common
{
    public class PulseAnimation : BaseMono
    {
        [SerializeField] private float scale = 1.2f;
        [SerializeField] private float duration = 0.2f;
        [SerializeField] private bool randomStart = false;
        [SerializeField] Ease ease = Ease.Linear;

        private Vector3 originalScale;
        private bool setScale = false;

        public override void DoEnable()
        {
            base.DoEnable();

            if (randomStart) duration = Random.Range(0.15f, 1f);
            if (!setScale)
            {
                originalScale = transform.localScale;
                setScale = true;
            }

            DoScale();
        }

        private void DoScale()
        {
            DOTween.Kill(this);

            transform.localScale = originalScale;
            transform.DOScale(scale * originalScale, duration).SetEase(ease).SetLoops(-1, LoopType.Yoyo)
                .SetTarget(this);
        }

        public override void DoDisable()
        {
            base.DoDisable();
            DOTween.Kill(this);
        }

        public override void CleanUp()
        {
            base.CleanUp();
            DOTween.Kill(this);
        }
    }
}