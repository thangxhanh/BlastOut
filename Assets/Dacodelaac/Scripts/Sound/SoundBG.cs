using Dacodelaac.Core;
using Dacodelaac.Sound;
using UnityEngine;

namespace Dacodelaac.Scripts.Sound{

    public class SoundBG : MonoSingleton<SoundBG>
    {
        [SerializeField] private AudioSource audioSource;

        protected override void Awake()
        {
            base.Awake();

            /* base.Awake() đã huỷ bản trùng — chỉ bản thắng mới cần sống qua scene. */
            if (Instance == this) DontDestroyOnLoad(gameObject);
        }

        public void PlayAudio(AudioSfx audioClip)
        {
            audioSource.clip = audioClip.Audio;
            audioSource.volume = audioClip.Volume;
            audioSource.Play();
        }
    }

}
