using Dacodelaac.Events;
using UnityEngine;
using UnityEngine.Events;

namespace Dacodelaac.Variables
{
    public class BaseVariableListener<TType, TEvent, TResponse> : BaseEventListener<TType, TEvent, TResponse>
        where TEvent : BaseVariable<TType>
        where TResponse : UnityEvent<TType>
    {
        [SerializeField] bool setOnEnable;

        protected override void OnEnable()
        {
            base.OnEnable();
            if (setOnEnable)
            {
                OnEventRaised(@event.Value);
            }
        }
    }
}