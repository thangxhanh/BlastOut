using Dacodelaac.Common;
using UnityEngine;
using Event = Dacodelaac.Events.Event;

namespace Dacodelaac.DebugUtils
{
    public class InterstitialMockUI : MonoBehaviour
    {
        [SerializeField] RandomImage image;
        [SerializeField] Event adsCloseInterstitialMockEvent;
        
        void OnEnable()
        {
            image.gameObject.SetActive(false);
        }

        public void OnShow()
        {
            image.gameObject.SetActive(true);
        }

        public void OnClose()
        {
            image.gameObject.SetActive(false);
            adsCloseInterstitialMockEvent.Raise();
        }
    }
}