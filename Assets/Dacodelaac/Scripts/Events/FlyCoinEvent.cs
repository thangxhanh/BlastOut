using UnityEngine;

namespace Dacodelaac.Events
{
    [CreateAssetMenu(menuName = "Event/Fly Coin Event")]
    public class FlyCoinEvent : BaseEvent<FlyCoinData>
    {
    }

    public class FlyCoinData
    {
        public Vector2 Position { get; set; }
        public System.Action OnCompleted { get; set; }
    }
}