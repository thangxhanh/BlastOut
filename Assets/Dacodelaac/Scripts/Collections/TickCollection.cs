using UnityEngine;

namespace Dacodelaac.Collections
{
    [CreateAssetMenu(menuName = "Collections/Tick")]
    public class TickCollection : BaseCollection<ITick>
    {
    }

    public interface ITick
    {
        void EarlyTick();
        void Tick();
        void LateTick();
        void FixedTick();
    }
}