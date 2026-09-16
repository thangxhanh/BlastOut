using Dacodelaac.Core;
using Dacodelaac.DataStorage;
using Dacodelaac.DebugUtils;
using Dacodelaac.Events;
using Dacodelaac.Scripts.Sound;
using Dacodelaac.Sound;
using UnityEngine;
using UnityEngine.Audio;

namespace Dev.Scripts.Sound
{
    /* Catalog âm thanh của game này. Nằm ở Assets/Dev vì mỗi game một bộ clip khác nhau —
       thêm clip mới = thêm một [SerializeField] AudioSfx + một method Play... ở đây,
       không đụng tới Dacodelaac. */
    [CreateAssetMenu(menuName = "Sound/SoundManager")]
    public class SoundManager : BaseSO
    {
        /* Chỉ có đúng một asset SoundManager, nên để static cho mọi nơi gọi được
           mà không phải kéo reference. GameLauncher gọi Initialize() để gán. */
        public static SoundManager Instance { get; private set; }

        [Header("SFX")]
        [SerializeField] private AudioSfx collectMoney;

        [Header("Music")]
        [SerializeField] private AudioSfx gameplayMusic;

        [Header("Event (chỉ cần khi muốn nhiều thứ cùng phản ứng)")]
        [SerializeField] private AudioSFXEvent playAudioEvent;

        [Header("Mixer")]
        [Tooltip("GameMixer — group Music và SFX phải expose param MusicVolume / SfxVolume.")]
        [SerializeField] private AudioMixer mixer;

        const string MUSIC_VOLUME_PARAM = "MusicVolume";
        const string SFX_VOLUME_PARAM = "SfxVolume";

        /* -80dB là ngưỡng im của mixer. Tắt bằng group chứ không bằng AudioSource.mute:
           mute chỉ với tới đúng một source, còn group phủ mọi source route vào nó —
           kể cả object sinh ra lúc chạy như âm của quái trên map. */
        const float MUTED_DB = -80f;
        const float FULL_DB = 0f;

        public override void Initialize()
        {
            Instance = this;
        }

        #region Gọi theo tên — thêm mỗi âm thanh một dòng ở đây

        public void PlayCollectMoney() => PlaySfx(collectMoney);
        public void PlayGameplayMusic() => PlayMusic(gameplayMusic);

        #endregion

        /* Đường THẲNG — không qua UnityEvent nên Ctrl+click lần được tới nơi phát. */
        public void PlaySfx(AudioSfx clip) => SFXController.Instance.PlayAudioOneShot(clip);

        public void PlayMusic(AudioSfx clip) => SoundBG.Instance.PlayAudio(clip);


        #region Bật/tắt — SettingsPopup gọi vào đây

        /// <summary>Áp lại cả ba thiết lập từ GameData. Gọi lúc khởi động để âm thanh khớp
        /// lựa chọn của người chơi ngay từ đầu, không chờ họ mở Settings.</summary>
        public void ApplySavedSettings()
        {
            SetMusic(GameData.Music);
            SetSound(GameData.Sound);
        }

        public void SetMusic(bool isOn)
        {
            GameData.Music = isOn;
            ApplyVolume(MUSIC_VOLUME_PARAM, isOn);
        }

        public void SetSound(bool isOn)
        {
            GameData.Sound = isOn;
            ApplyVolume(SFX_VOLUME_PARAM, isOn);
        }

        void ApplyVolume(string param, bool isOn)
        {
            if (!mixer) return;

            /* SetFloat trả false khi sai tên hoặc param chưa expose, và KHÔNG ném exception —
               không kiểm ở đây thì âm thanh hỏng hoàn toàn im lặng. */
            if (!mixer.SetFloat(param, isOn ? FULL_DB : MUTED_DB))
                Dacoder.LogError("Mixer thiếu param đã expose: ", param);
        }

        public void SetVibration(bool isOn) => GameData.Vibration = isOn;

        /// <summary>Rung một nhịp ngắn nếu người chơi chưa tắt. Mọi nơi muốn rung phải đi qua đây,
        /// đừng gọi thẳng Handheld.Vibrate — gọi thẳng là bỏ qua lựa chọn của người chơi.</summary>
        public void Vibrate()
        {
            if (!GameData.Vibration) return;

#if UNITY_ANDROID || UNITY_IOS
            Handheld.Vibrate();
#endif
        }

        #endregion
        /* Đường QUA EVENT — cho prefab dùng chung (nút bấm, popup) không nên biết
           SoundManager tồn tại. Cần AudioSFXEventListener trên prefab phát âm. */
        public void PlayAudio(AudioSfx audioClip)
        {
            playAudioEvent.Raise(audioClip);
        }
    }
}
