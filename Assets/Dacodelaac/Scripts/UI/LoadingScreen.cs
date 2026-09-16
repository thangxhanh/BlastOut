using System;
using System.Collections;
using Dacodelaac.Core;
using Dacodelaac.Events;
using DG.Tweening;
//using Spine.Unity;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;
using Dacodelaac.DataStorage;

namespace Dacodelaac.UI
{
    public class LoadingScreen : BaseMono
    {
        [SerializeField] private Canvas canvas;
        [SerializeField] private CanvasGroup canvasGroup;
        [Header("Launching")] [SerializeField] private GameObject launching;
        [SerializeField] private Image progress;
        [Header("Loading")] [SerializeField] private GameObject loading;
        [SerializeField] private Transform logo;
        [SerializeField] private TMP_Text txtLoading;

        //[SerializeField] private SkeletonGraphic hero;
        //[SerializeField] private SkeletonGraphic princess;

        private bool Loading { get; set; }
        private float _timeScale;

        private void Awake()
        {
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            GameData.GameSessionCount++;
        }

        public void Load(LoadingScreenData data)
        {
            GetPools().DespawnAll();
            GetPools().DestroyAll();
            
            if (data.IsLaunching)
            {
                Launching(data.Scene, data.MinLoadTime, data.LaunchCondition);
            }
            else
            {
                LoadScene(data.Scene, data.MinLoadTime, data.LaunchCondition);
            }
        }

        private void Launching(string sceneName, float minLoadTime, Func<bool> launchCondition = null)
        {
            StartCoroutine(LaunchingRoutine(sceneName, minLoadTime, launchCondition));
        }

        private IEnumerator LaunchingRoutine(string sceneName, float minLoadTime, Func<bool> launchCondition)
        {
            if (Loading) yield break;
            Loading = true;
            canvas.gameObject.SetActive(true);
            launching.gameObject.SetActive(true);
            loading.gameObject.SetActive(false);

            var ao = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            ao.allowSceneActivation = false;

            var t = 0f;
            var startLoadTime = DateTime.Now;

            // var pos = princess.transform.position;
            // pos.y = hero.transform.position.y;
            // pos.x -= 130;
            //
            // hero.transform.DOMove(pos, minLoadTime).OnComplete(() =>
            // {
            //     princess.AnimationState.SetAnimation(0, "Win_Scene", true);
            //     hero.AnimationState.SetAnimation(0, "Happy", true);
            // });

            while (t < minLoadTime || ao.progress < 0.9f)
            {
                t += Time.unscaledDeltaTime;
                var percent = Mathf.Min(t / minLoadTime, ao.progress / 0.9f);
                progress.fillAmount = percent;
                txtLoading.text = $"{(percent * 100):N0}%";

                yield return null;
            }

            if (launchCondition != null)
            {
                yield return new WaitUntil(launchCondition);
            }

            ao.allowSceneActivation = true;
        }

        private void LoadScene(string sceneName, float minLoadTime, Func<bool> launchCondition = null)
        {
            StartCoroutine(LoadSceneRoutine(sceneName, minLoadTime, launchCondition));
        }

        private IEnumerator LoadSceneRoutine(string sceneName, float minLoadTime, Func<bool> launchCondition)
        {
            if (Loading) yield break;
            Loading = true;

            _timeScale = Time.timeScale;
            Time.timeScale = 0;

            canvas.gameObject.SetActive(true);
            launching.gameObject.SetActive(false);
            loading.gameObject.SetActive(true);

            logo.localScale = Vector3.one * 10f;
            canvasGroup.alpha = 0f;

            var zoomIn = false;

            DOTween.Kill(this);
            DOTween.To(() => 0f, x =>
            {
                canvasGroup.alpha = x;
                logo.localScale = Mathf.Lerp(2.5f, 1f, x) * Vector3.one;
            }, 1f, 1f).SetTarget(this).SetUpdate(true).OnComplete(() =>
            {
                zoomIn = true;
                logo.DOScale(1.1f, 0.5f).SetEase(Ease.InQuad).SetLoops(-1, LoopType.Yoyo).SetTarget(this)
                    .SetUpdate(true);
            });

            yield return new WaitUntil(() => zoomIn);

            var ao = SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single);
            ao.allowSceneActivation = false;

            var t = 0f;

            while (t < minLoadTime || ao.progress < 0.9f)
            {
                t += Time.unscaledDeltaTime;

                yield return null;
            }

            if (launchCondition != null)
            {
                yield return new WaitUntil(launchCondition);
            }

            ao.allowSceneActivation = true;
            Time.timeScale = _timeScale;
        }

        public void OnLoadingFinished()
        {
            if (loading.activeSelf)
            {
                DOTween.Kill(this);

                var scale = logo.localScale.x;
                DOTween.To(() => 1f, x =>
                {
                    canvasGroup.alpha = x;
                    logo.localScale = Mathf.Lerp(2.5f, scale, x) * Vector3.one;
                }, 0f, 1f).SetTarget(this).SetUpdate(true).OnComplete(() =>
                {
                    Loading = false;
                    canvas.gameObject.SetActive(false);
                });
            }
            else
            {
                Loading = false;
                canvas.gameObject.SetActive(false);
            }
        }
    }
}