using UnityEngine.Events;

namespace Dacodelaac.Events
{
    public class BaseEventResponse : UnityEvent, IEventResponse
    {
    }

    public class BaseEventResponse<TType> : UnityEvent<TType>, IEventResponse
    {
    }
}