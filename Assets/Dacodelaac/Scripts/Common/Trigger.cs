using System;
using UnityEngine;

namespace Dacodelaac.Common
{
    public class Trigger : MonoBehaviour
    {
        public event System.Action<Collider> OnTriggerEnterEvent;
        public event System.Action<Collider> OnTriggerStayEvent;
        public event System.Action<Collider> OnTriggerExitEvent;
        
        public event System.Action<Collision> OnCollisionEnterEvent;
        public event System.Action<Collision> OnCollisionStayEvent;
        public event System.Action<Collision> OnCollisionExitEvent;
        
        void OnTriggerEnter(Collider other)
        {
            OnTriggerEnterEvent?.Invoke(other);
        }

        void OnTriggerStay(Collider other)
        {
            OnTriggerStayEvent?.Invoke(other);
        }

        void OnTriggerExit(Collider other)
        {
            OnTriggerExitEvent?.Invoke(other);
        }

        void OnCollisionEnter(Collision other)
        {
            OnCollisionEnterEvent?.Invoke(other);
        }

        void OnCollisionStay(Collision other)
        {
            OnCollisionStayEvent?.Invoke(other);
        }

        void OnCollisionExit(Collision other)
        {
            OnCollisionExitEvent?.Invoke(other);
        }
    }
}