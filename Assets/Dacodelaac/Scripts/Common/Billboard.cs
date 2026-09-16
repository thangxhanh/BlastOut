using Dacodelaac.Variables;
using UnityEngine;

namespace Dacodelaac.Common
{
    public class Billboard : MonoBehaviour
    {
        [SerializeField] CameraVariable mainCamera;
        
        void LateUpdate()
        {
            transform.rotation = mainCamera.Value.transform.rotation;
        }
    }
}