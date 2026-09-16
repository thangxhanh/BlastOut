using UnityEngine;

namespace Dacodelaac.Events
{
    [CreateAssetMenu(menuName = "Event/Ads Request Show Rewarded Event")]
    public class AdsShowRewardedEvent : BaseEvent<AdsShowRewardedData>
    {
    }

    public class AdsShowRewardedData
    {
        public System.Action OnAvailable { get; set; }
        public System.Action OnNotAvailable { get; set; }
        public System.Action OnCompleted { get; set; }
        public System.Action OnClosed { get; set; }
    }
}