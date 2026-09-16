using UnityEngine;

namespace Dacodelaac.UI.Tabs
{
    public class BaseTab : MonoBehaviour
    {
        public int Index { get; set; }

        public virtual void OnInstantiate()
        {

        }

        public virtual void OnBeginShow()
        {

        }

        public virtual void OnShown()
        {

        }

        public virtual void OnResume()
        {

        }

        public virtual void OnBeginDismiss()
        {

        }

        public virtual void OnDismissed()
        {

        }
    }
}
