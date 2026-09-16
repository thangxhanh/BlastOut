using UnityEngine;

namespace Dacodelaac.ObjectPooling
{
    public class PooledObjectId : MonoBehaviour
    {
        [SerializeField] private GameObject prefab;

        public GameObject Prefab => prefab;

        /* Chỉ Pools được gán — internal nên không lọt ra ngoài assembly. */
        internal void SetPrefab(GameObject value) => prefab = value;
    }
}