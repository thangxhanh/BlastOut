using Dacodelaac.Core;
using UnityEngine;

namespace Dacodelaac.Sound
{
    public class SFXController : MonoSingleton<SFXController>
    {
        [SerializeField] private AudioSource mainAudio;

        protected override void Awake()
        {
            base.Awake();

            /* base.Awake() đã huỷ bản trùng — chỉ bản thắng mới cần sống qua scene. */
            if (Instance == this) DontDestroyOnLoad(gameObject);
        }

        public void PlayAudioOneShot(AudioSfx audio)
        {
            mainAudio.PlayOneShot(audio.Audio, audio.Volume);
        }
    }
}
