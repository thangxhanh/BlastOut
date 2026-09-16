using UnityEngine;

namespace Dacodelaac.UI.Tabs
{
    public class BaseTabButton : MonoBehaviour
    {
        public event System.Action<int> OnClickedEvent = null;
        public int Index { get; set; }

        public void OnClicked()
        {
            OnClickedEvent?.Invoke(Index);
        }

        public virtual void OnSelected()
        {

        }

        public virtual void OnDeselected()
        {

        }
    }
}
