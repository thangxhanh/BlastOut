using System;
using UnityEngine;

namespace Dacodelaac.Attributes
{
    public class GUIDAttribute : PropertyAttribute
    {
        /* GuidAttributeDrawer nằm ở assembly khác nên cần property public để đọc. */
        public string Prefix { get; }

        public GUIDAttribute()
        {
            Prefix = string.Empty;
        }

        public GUIDAttribute(string prefix)
        {
            Prefix = prefix;
        }
    }
}