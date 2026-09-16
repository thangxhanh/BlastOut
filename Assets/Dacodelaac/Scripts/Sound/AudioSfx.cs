using System;
using UnityEngine;

namespace Dacodelaac.Sound
{
    /* Ở LẠI framework: AudioSFXEvent / SFXController / SoundBG đều dùng type này.
       Còn catalog clip (SoundManager) là content riêng từng game nên nằm ở Assets/Dev. */
    [Serializable]
    public class AudioSfx
    {
        [SerializeField] private AudioClip audio;
        [SerializeField, Range(0f, 2f)] private float volume = 1f;

        public AudioClip Audio => audio;
        public float Volume => volume;
    }
}
