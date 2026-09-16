using UnityEngine;

namespace Dacodelaac.UI.Tabs
{
    public class BaseTabController : MonoBehaviour
    {
        BaseTabButton[] tabButtons;
        BaseTab[] tabs;
        protected int currentTab;
        
        public BaseTab CurrentTab => tabs[currentTab];

        public void Initialize()
        {
            tabButtons = GetComponentsInChildren<BaseTabButton>();
            tabs = GetComponentsInChildren<BaseTab>();
            
            for (var i = 0; i < tabButtons.Length; i++)
            {
                tabButtons[i].Index = i;
                tabs[i].Index = i;
                tabs[i].OnInstantiate();
            }
            OnChangeTab(-1);
        }

        public void Show(int tabIndex)
        {
            for (var i = 0; i < tabButtons.Length; i++)
            {
                tabButtons[i].OnClickedEvent += OnChangeTab;
            }

            currentTab = tabIndex;
            OnChangeTab(currentTab);
        }

        public void Resume()
        {
            foreach (var tab in tabs)
            {
                tab.OnResume();
            }
        }

        public void Dismiss()
        {
            for (int i = 0; i < tabButtons.Length; i++)
            {
                tabButtons[i].OnClickedEvent -= OnChangeTab;
            }
            OnChangeTab(-1);
        }

        void OnChangeTab(int tabIndex)
        {
            if (tabIndex != -1)
            {
                currentTab = tabIndex;
            }

            foreach (var button in tabButtons)
            {
                if (button.Index == tabIndex)
                {
                    button.OnSelected();
                }
                else
                {
                    button.OnDeselected();
                }
            }

            foreach (var tab in tabs)
            {
                if (tab.Index != tabIndex && tab.gameObject.activeSelf)
                {
                    tab.OnBeginDismiss();
                    tab.gameObject.SetActive(false);
                    tab.OnDismissed();
                }
                else if (tab.Index == tabIndex && !tab.gameObject.activeSelf)
                {
                    tab.OnBeginShow();
                    tab.gameObject.SetActive(true);
                    tab.OnShown();
                }
            }
        }
    }
}
