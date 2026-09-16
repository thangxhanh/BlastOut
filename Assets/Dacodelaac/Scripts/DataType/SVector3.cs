using UnityEngine;
using Newtonsoft.Json;

namespace Dacodelaac.DataType
{
    [System.Serializable, JsonObject(MemberSerialization.OptIn)]
    public struct SVector3
    {
        [JsonProperty] public float x;
        [JsonProperty] public float y;
        [JsonProperty] public float z;

        public SVector3(Vector3 v)
        {
            x = v.x;
            y = v.y;
            z = v.z;
        }

        public static implicit operator Vector3(SVector3 v) => new Vector3(v.x, v.y, v.z);
        public static explicit operator SVector3(Vector3 v) => new SVector3(v);
    }
}