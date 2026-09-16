using System.IO;
using UnityEngine;

namespace Dacodelaac.Utils
{
    public class ScreenShooterSingle : MonoBehaviour
    {
        [SerializeField] Resolution resolution;
        [SerializeField] KeyCode keyCode;
        
#if UNITY_EDITOR
        void Update()
        {
            if (Input.GetKeyDown(keyCode))
            {
                Shoot();
            }
        }
        
        [ContextMenu("Shoot")]
        public void Shoot()
        {
            if (!Directory.Exists(Application.dataPath + $"/../ScreenShots"))
            {
                Directory.CreateDirectory(Application.dataPath + $"/../ScreenShots");
            }
            TextureUtils.CaptureToFile(GetComponent<Camera>(), Application.dataPath + $"/../ScreenShots/{TimeUtils.CurrentTicks}", resolution.width, resolution.height, refresh: false);
        }
#endif
    }
}