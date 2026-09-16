using System;
using UnityEngine;

namespace Dacodelaac.Variables
{
    [Serializable]
    public class BaseReference : IReference
    {
    }

    [Serializable]
    public class BaseReference<TType, TVariable> : BaseReference, IReference<TType, TVariable>
        where TVariable : BaseVariable<TType>
    {
        [SerializeField] bool useVariable;
        [SerializeField] TType constantValue;
        [SerializeField] TVariable variable;

        public TType Value
        {
            get => useVariable ? variable.Value : constantValue;
            set
            {
                if (useVariable)
                {
                    variable.Value = value;
                }
                else
                {
                    constantValue = value;
                }
            }
        }

        public override string ToString()
        {
            return Value.ToString();
        }
    }
}
