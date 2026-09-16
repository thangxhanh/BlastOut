using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Dacodelaac.Common
{
    public class KeepFollow : MonoBehaviour
    {
        [SerializeField] Transform target;
        [SerializeField] Vector3 offset;

        void Update()
        {
            transform.position = target.position + offset;
        }
    }
}