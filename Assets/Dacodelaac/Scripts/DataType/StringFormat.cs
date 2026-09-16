using Dacodelaac.Core;
using UnityEngine;

namespace Dacodelaac.Scripts.DataType
{
    [CreateAssetMenu]
    public class StringFormat : BaseSO
    {
        [SerializeField] string format;

        public string Format(object value) => string.Format(format, value);
    }
}