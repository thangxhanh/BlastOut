using System;
using Dacodelaac.DataStorage;
using Dacodelaac.Events;
using UnityEngine;

namespace Dacodelaac.Variables
{
    public class BaseVariable<TType> : BaseEvent<TType>, IVariable<TType>, ISerializationCallbackReceiver
    {
        [SerializeField] TType initializeValue;
        [SerializeField] bool isSavable;
        [SerializeField] bool isRaiseEvent;
        [NonSerialized] TType runtimeValue;
        
        private Action<TType> onValueChanged;

        public TType Value
        {
            get => isSavable ? GameData.Get(Id, initializeValue) : runtimeValue;
            set
            {
                if (isSavable)
                {
                    GameData.Set(Id, value);
                }
                else
                {
                    runtimeValue = value;
                }
                if (isRaiseEvent)
                {
                    Raise(value);
                    onValueChanged?.Invoke(Value);
                }
            }
        }

        public void OnBeforeSerialize()
        {
            if (name != Id)
            {
                Debug.LogError("Variable error: " + name);
            }
        }

        public void OnAfterDeserialize()
        {
            runtimeValue = initializeValue;
        }

        public override string ToString()
        {
            return Value.ToString();
        }
        
        public void AddValueListener(Action<TType> listener)
        {
            onValueChanged += listener;
            onValueChanged.Invoke(Value);
        }

        public void RemoveValueListener(Action<TType> listener)
        {
            onValueChanged -= listener;
        }
    }
}