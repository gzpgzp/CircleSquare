using Tools.ResourcesTool;
using Tools.Singletons;
using UnityEngine;

namespace Sounds
{
    public class SoundManager : MMSingleton<SoundManager>
    {
        [SerializeField] private AudioSource bgmSource;
        [SerializeField] private AudioSource currAudio;

        public void SetBGMVolume(float volume)
        {
            bgmSource.volume = volume;
        }

        public void SetAudioVolume(float volume)
        {
            currAudio.volume = volume;
        }

        public void PlayBGM()
        {
            bgmSource.Play();
            bgmSource.loop = true;
        }

        public void PlaySound(string soundName)
        {
            
        }

        public void PlaySound(int soundId)
        {
            
        }

        public void PlaySound(AudioClip clip)
        {
            currAudio.clip = clip;
            currAudio.Play();
        }
    }
}