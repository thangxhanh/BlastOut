using System.Collections;
using System.Collections.Generic;
using Dacodelaac.Core;
using Dacodelaac.Events;

using DG.Tweening;
using UnityEngine;
using UnityEngine.UI;

namespace Dacodelaac.Scripts.Common
{
    public class FlyCoinTarget : BaseMono
    {
        [SerializeField] private RectTransform rect;
        [SerializeField] private GameObject flyCoinPrefab;


        private List<GameObject> flyCoins = new List<GameObject>();
        private Coroutine _coroutine;

        public void FlyCoin(FlyCoinData data)
        {
            PlayCoinSound();
            if (_coroutine != null) StopCoroutine(_coroutine);
            _coroutine = StartCoroutine(IEFlyCoin(data));
        }

        public override void DoDisable()
        {
            base.DoDisable();
            if (_coroutine != null) StopCoroutine(_coroutine);
            DOTween.Kill(this);
            foreach (var coin in flyCoins)
            {
                Destroy(coin);
            }
        
            flyCoins.Clear();
        }

        public void PlayCoinSound()
        {
            // SoundManager giờ ở Assets/Dev — framework không tham chiếu được. Bắn AudioSFXEvent thay thế.
        }

        private IEnumerator IEFlyCoin(FlyCoinData data)
        {
            yield return new WaitForEndOfFrame();

            var container = rect.GetComponentInParent<CanvasScaler>().transform;

            var corners = new Vector3[4];
            var position = container.InverseTransformPoint(data.Position);
            position.z = 0;

            var count = 10;//Random.Range(5, 10);
            var completedCount = 0;
            for (var i = 0; i < count; i++)
            {
                var coin = Instantiate(flyCoinPrefab, container);
                var canvas = coin.GetComponent<Canvas>();
                if (canvas != null)
                {
                    canvas.sortingLayerName = "UI";
                }

                flyCoins.Add(coin);
                coin.transform.localScale = Vector3.one;

                var r = coin.GetComponent<RectTransform>();
                r.anchoredPosition3D = position;

                var offset = (Vector3) Random.insideUnitCircle * 70f;
                var delay = Random.Range(0.2f, 0.2f);
                var duration = Random.Range(0.5f, 1f);
                r.DOAnchorPos(position + offset, 0.3f).SetEase(Ease.OutQuad).SetTarget(this).OnComplete(() =>
                {
                    var startPos = r.anchoredPosition3D;
                    DOTween.To(() => 0f,
                            x =>
                            {
                                rect.GetWorldCorners(corners);
                                var targetPos = container.InverseTransformPoint((corners[0] + corners[2]) / 2);
                                targetPos.z = 0;
                                r.anchoredPosition3D = Vector3.Lerp(startPos, targetPos, x);
                            }, 1f, duration).SetEase(Ease.InQuad).SetDelay(delay).SetTarget(this)
                        .OnComplete(
                            () =>
                            {
                                flyCoins.Remove(coin.gameObject);
                                Destroy(coin.gameObject);
                                completedCount++;
                            }).Play();
                }).Play();
            }

            yield return new WaitUntil(() => completedCount >= count);

            data.OnCompleted?.Invoke();
        }
    }
}