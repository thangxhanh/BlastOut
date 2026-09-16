using System;
using Dacodelaac.Core;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Dacodelaac.Common
{
    [RequireComponent(typeof(AudioSource))]
    public class PlayAudioRandom : BaseMono
    {
        [SerializeField] AudioSource audioSource;
        [SerializeField] AudioClip[] audioClips;
        [SerializeField] bool playOneTime;
        [SerializeField] bool playOnEnable;

        bool played;

        public void PlayOneShot()
        {
            if (playOneTime && played) return;
            played = true;
            audioSource.PlayOneShot(audioClips[Random.Range(0, audioClips.Length)]); 
        }

        protected override void OnEnable()
        {
            base.OnEnable();

            if (playOnEnable)
            {
                PlayOneShot();
            }
        }
    }
}