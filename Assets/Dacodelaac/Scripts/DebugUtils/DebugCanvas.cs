using System;
using UnityEngine;

namespace Dacodelaac.DebugUtils
{
    public class DebugCanvas : MonoBehaviour
    {
        void Start()
        {
#if UNITY_EDITOR
            DontDestroyOnLoad(gameObject);
#else
            Destroy(gameObject);
#endif
        }
    }
}