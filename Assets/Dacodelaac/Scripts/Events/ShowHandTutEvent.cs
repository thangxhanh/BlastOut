using UnityEngine;

namespace Dacodelaac.Events
{
    [CreateAssetMenu(menuName = "Event/Show Hand Tut Event")]
    public class ShowHandTutEvent : BaseEvent<ShowHandTutData>
    {
    }

    public class ShowHandTutData
    {
        public Vector3[] LineVetexPositions { get; set; }
    }
}