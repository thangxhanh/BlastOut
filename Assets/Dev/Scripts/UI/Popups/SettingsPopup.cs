using Dacodelaac.DataStorage;
using Dacodelaac.UI.Popups;
using Dev.Scripts.Sound;
using UnityEngine;
using UnityEngine.Events;

namespace Dev.Scripts.UI.Popups
{
    /// <summary>
    /// Bật/tắt nhạc, âm thanh, rung. Nguồn thật là <see cref="GameData"/>; popup chỉ đọc ra lúc mở
    /// và gọi <see cref="SoundManager"/> khi người chơi gạt — không với tay thẳng vào
    /// SFXController/SoundBG, để mọi đường đổi âm thanh đi qua đúng một đầu mối.
    /// </summary>
    public class SettingsPopup : BasePopup
    {
        [SerializeField] private SettingToggle musicToggle;
        [SerializeField] private SettingToggle soundToggle;
        [SerializeField] private SettingToggle vibrationToggle;

        protected override void AfterInstantiate()
        {
            /* Nối ở đây chứ không kéo onClick trong Inspector: SettingToggle là nút bấm tự lật
               trạng thái, kéo tay thì mỗi ô phải nhớ nối đúng một hàm. */
            Bind(musicToggle, OnMusicButton);
            Bind(soundToggle, OnSoundButton);
            Bind(vibrationToggle, OnVibrationButton);
        }

        void Bind(SettingToggle toggle, UnityAction onClick)
        {
            if (!toggle) return;

            toggle.Init();
            if (toggle.Button) toggle.Button.onClick.AddListener(onClick);
        }

        protected override void BeforeShow(object data = null)
        {
            if (musicToggle) musicToggle.SetState(GameData.Music);
            if (soundToggle) soundToggle.SetState(GameData.Sound);
            if (vibrationToggle) vibrationToggle.SetState(GameData.Vibration);
        }

        public void OnMusicButton()
        {
            var isOn = !GameData.Music;

            SoundManager.Instance.SetMusic(isOn);

            if (musicToggle) musicToggle.SetState(isOn);
        }

        public void OnSoundButton()
        {
            var isOn = !GameData.Sound;

            SoundManager.Instance.SetSound(isOn);

            if (soundToggle) soundToggle.SetState(isOn);
        }

        /// <summary>Rung một nhịp khi bật để người chơi biết nó có tác dụng.</summary>
        public void OnVibrationButton()
        {
            var isOn = !GameData.Vibration;

            SoundManager.Instance.SetVibration(isOn);
            if (isOn) SoundManager.Instance.Vibrate();

            if (vibrationToggle) vibrationToggle.SetState(isOn);
        }

        protected override void BeforeDismiss()
        {
            /* Storage.Set chỉ ghi RAM — không Save() thì tắt app là mất thiết lập vừa đổi. */
            GameData.Save();
        }

        public void OnCloseButton() => Close();
    }
}
