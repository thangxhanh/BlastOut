using Dacodelaac.Core;
using DG.DemiLib.Attributes;
using UnityEngine;
using UnityEngine.Events;

namespace Dacodelaac.Events
{
    [DeScriptExecutionOrder(ExecutionOrder.First)]
    public class BaseEventListener<TEvent, TResponse> : BaseMono, IEventListener
        where TEvent : BaseEvent
        where TResponse : UnityEvent
    {
        [SerializeField] TEvent @event;
        [SerializeField] TResponse response;

        public void OnEventRaised()
        {
            response?.Invoke();
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            @event.AddListener(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            @event.RemoveListener(this);
        }
    }
    
    [DeScriptExecutionOrder(ExecutionOrder.First)]
    public class BaseEventListener<TType, TEvent, TResponse> : BaseMono, IEventListener<TType>
        where TEvent : BaseEvent<TType>
        where TResponse : UnityEvent<TType>
    {
        [SerializeField] protected TEvent @event;
        [SerializeField] TResponse response;

        public void OnEventRaised(TType value)
        {
            response?.Invoke(value);
        }

        protected override void OnEnable()
        {
            base.OnEnable();
            @event.AddListener(this);
        }

        protected override void OnDisable()
        {
            base.OnDisable();
            @event.RemoveListener(this);
        }
    }
    
    [DeScriptExecutionOrder(ExecutionOrder.First)]
    public class BaseCombinedEventListener<TEvent, TResponse> : BaseMono, IEventListener
        where TEvent : BaseEvent
        where TResponse : UnityEvent
    {
        [SerializeField] TEvent[] events;
        [SerializeField] TResponse response;

        public void OnEventRaised()
        {
            response?.Invoke();
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            foreach (var @event in events)
            {
                @event.AddListener(this);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            foreach (var @event in events)
            {
                @event.RemoveListener(this);
            }
        }
    }
    
    [DeScriptExecutionOrder(ExecutionOrder.First)]
    public class BaseCombinedEventListener<TType, TEvent, TResponse> : BaseMono, IEventListener<TType>
        where TEvent : BaseEvent<TType>
        where TResponse : UnityEvent<TType>
    {
        [SerializeField] TEvent[] events;
        [SerializeField] TResponse response;

        public void OnEventRaised(TType value)
        {
            response?.Invoke(value);
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            foreach (var @event in events)
            {
                @event.AddListener(this);
            }
        }

        protected override void OnDisable()
        {
            base.OnDisable();

            foreach (var @event in events)
            {
                @event.RemoveListener(this);
            }
        }
    }
}