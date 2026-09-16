using System;
using System.Collections;
using UnityEngine;
using UnityEngine.Networking;
using UnityEngine.UI;

namespace Dacodelaac.Common
{
    [RequireComponent(typeof(RawImage))]
    public class RandomImage : MonoBehaviour
    {
        const string Url = "https://picsum.photos/seed/{0}/{1}/{2}";
        RectTransform rt;
        RawImage ra;

        void OnEnable()
        {
            rt = GetComponent<RectTransform>();
            ra = GetComponent<RawImage>();
            ra.color = Color.black;
            StartCoroutine(Fetch());
        }

        IEnumerator Fetch()
        {
            using (var request = UnityWebRequestTexture.GetTexture(string.Format(Url, Time.time, rt.rect.width, rt.rect.height)))
            {
                yield return request.SendWebRequest();

                if (request.result == UnityWebRequest.Result.Success)
                {
                    ra.texture = DownloadHandlerTexture.GetContent(request);
                }
                ra.color = Color.white;
            }
        }
    }
}