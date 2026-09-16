using System.Collections;
using UnityEngine;

namespace Dacodelaac.ObjectPooling
{
    public class PooledParticleCallback : MonoBehaviour
    {
        [SerializeField] Pools pools;

        void OnParticleSystemStopped()
        {
            StartCoroutine(IEDespawn());
        }

        IEnumerator IEDespawn()
        {
            yield return null;
            pools.DeSpawn(gameObject);
        }
    }
}