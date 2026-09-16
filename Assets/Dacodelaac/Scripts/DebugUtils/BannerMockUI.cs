using Dacodelaac.Common;
using UnityEngine;

namespace Dacodelaac.DebugUtils
{
    public class BannerMockUI : MonoBehaviour
    {
        [SerializeField] RandomImage image;

        void OnEnable()
        {
            image.gameObject.SetActive(false);
        }

        public void OnShow()
        {
            image.gameObject.SetActive(true);
        }

        public void OnHide()
        {
            image.gameObject.SetActive(false);
        }
    }
}