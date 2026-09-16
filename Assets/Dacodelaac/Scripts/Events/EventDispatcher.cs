using Dacodelaac.Core;
using DG.DemiLib.Attributes;
using UnityEngine;

namespace Dacodelaac.Events
{
    [DeScriptExecutionOrder(ExecutionOrder.Default)]
    public class EventDispatcher : BaseMono
    {
        [SerializeField] Event @event;
        [SerializeField] bool dispatchOnEnable;

        void Start()
        {
            if (dispatchOnEnable)
            {
                Dispatch();
            }
        }

        public void Dispatch()
        {
            @event.Raise();
        }
    }
}