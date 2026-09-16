using UnityEngine;

namespace Dacodelaac.UI.SwipeUI
{
    public class BasePage : MonoBehaviour
    {
        public Vector2 Position { get; set; }

        public virtual void OnBeginShow()
        {
            
        }

        public virtual void OnBeginDismiss()
        {
            
        }
    }
}