using System;
using Dacodelaac.Variables;
using UnityEngine;

namespace Dacodelaac.Utils
{
    public class CameraExtra : MonoBehaviour
    {
        [SerializeField] bool keepMinHorizontalFov = true;
        [SerializeField] Vector2 minReferenceAspect = new Vector2(9, 16);
        [SerializeField] bool keepMaxHorizontalFov = true;
        [SerializeField] Vector2 maxReferenceAspect = new Vector2(3, 4);
        [SerializeField] FloatVariable cameraOrthoSize;

        float originalFov;
        float originalOrthoSize;
        Camera cam;
        bool setup;

        void Awake()
        {
            cam = GetComponent<Camera>();
            Setup(cam);
            Apply();
        }

        public void Setup(Camera c)
        {
            if (setup) return;
            setup = true;
            
            cam = c;
            originalFov = cam.fieldOfView;
            originalOrthoSize = cam.orthographicSize;
        }

        public void Apply()
        {
            if (keepMinHorizontalFov)
            {
                KeepMinHorizontalFov();
            }

            if (keepMaxHorizontalFov)
            {
                KeepMaxHorizontalFov();
            }
        }

        void KeepMinHorizontalFov()
        {
            KeepHorizontalFov(minReferenceAspect.x / minReferenceAspect.y, true);
        }

        void KeepMaxHorizontalFov()
        {
            KeepHorizontalFov(maxReferenceAspect.x / maxReferenceAspect.y, false);
        }

        void KeepHorizontalFov(float refAspect, bool min)
        {
            if ((min && cam.aspect < refAspect) || (!min && cam.aspect > refAspect))
            {
                if (cam.orthographic)
                {
                    cam.orthographicSize = originalOrthoSize * refAspect / cam.aspect;
                    cameraOrthoSize.Value = cam.orthographicSize;
                }
                else
                {
                    var hfov = VFov2HFov(originalFov, refAspect);
                    cam.fieldOfView = HFov2VFov(hfov, cam.aspect);   
                }
            }
        }

        public static float VFov2HFov(float vfov, float camAspect)
        {
            vfov *= Mathf.Deg2Rad;
            var camHalfH = Mathf.Tan(vfov / 2);
            var camHalfW = camHalfH * camAspect;

            return 2 * Mathf.Atan(camHalfW) * Mathf.Rad2Deg;
        }

        public static float HFov2VFov(float hfov, float camAspect)
        {
            hfov *= Mathf.Deg2Rad;
            var camHalfW = Mathf.Tan(hfov / 2);
            var camHalfH = camHalfW / camAspect;

            return 2 * Mathf.Atan(camHalfH) * Mathf.Rad2Deg;
        }
    }
}