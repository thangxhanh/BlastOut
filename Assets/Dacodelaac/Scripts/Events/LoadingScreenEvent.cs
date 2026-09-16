using UnityEngine;

namespace Dacodelaac.Events
{
    [CreateAssetMenu(menuName = "Event/Loading Screen Event")]
    public class LoadingScreenEvent : BaseEvent<LoadingScreenData>
    {
    }

    public class LoadingScreenData
    {
        public bool IsLaunching { get; set; }
        public string Scene { get; set; }
        public float MinLoadTime { get; set; }
        public System.Func<bool> LaunchCondition { get; set; }
    }
}