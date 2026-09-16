using Dacodelaac.Core;
using UnityEngine;
using UnityEngine.UI;
using Event = Dacodelaac.Events.Event;

namespace Dacodelaac.UI.Buttons
{
    public class CustomToggle : BaseMono
    {
        [SerializeField] private GameObject goOn;
        [SerializeField] private GameObject goOff;
        
        [SerializeField] private Button button;
        [SerializeField] private Button.ButtonClickedEvent m_OnClick;
        [SerializeField] private Event buttonClickEvent;

        private bool isClick = false;
        public override void DoEnable()
        {
            base.DoEnable();
            if (button != null) button.onClick.AddListener(OnClickToggle);
        }

        public override void DoDisable()
        {
            base.DoDisable();
            if (button != null) button.onClick.RemoveListener(OnClickToggle);
        }

        private void OnValidate()
        {
            if (button != null) button.onClick.RemoveListener(OnClickToggle);
            button = gameObject.GetComponent<Button>();
        }
        
        private void OnClickToggle()
        {
            m_OnClick?.Invoke();
            buttonClickEvent.Raise();
            SetOnOff(!IsOn);
        }

        
        public bool IsOn { get; set; } = true;

        private void Start()
        {
            SetOnOff(IsOn);
        }

        public void SetOnOff(bool on)
        {
            IsOn = on;
            goOn.SetActive(on);
            goOff.SetActive(!on);
        }
    }
}