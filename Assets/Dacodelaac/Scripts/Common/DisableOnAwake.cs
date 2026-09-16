using System;
using UnityEngine;

namespace Dacodelaac.Common
{
    public class DisableOnAwake : MonoBehaviour
    {
        void Awake()
        {
            gameObject.SetActive(false);
        }
    }
}