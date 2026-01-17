using Sounds;
using UnityEngine;
using UnityEngine.UI;

namespace NormalUI
{
    public enum SoundEnum
    {
        None,
        Sound,
        BGM,
    }

    public class SoundItem : MonoBehaviour
    {
        [SerializeField] private SoundEnum soundEnum;
        [SerializeField] private Slider slider;
        [SerializeField] private Text soundNum;

        private void Start()
        {
            slider.onValueChanged.AddListener(OnSoundValueChanged); 
        }

        private void OnSoundValueChanged(float value)
        {
            switch (soundEnum)
            {
                // 除一百是为了转为0-1的值
                case SoundEnum.Sound:
                    SoundManager.Instance.SetAudioVolume(value / 100f);
                    break;
                case SoundEnum.BGM:
                    SoundManager.Instance.SetBGMVolume(value / 100f);
                    break;
            }
            soundNum.text = ((int)value).ToString();
        }
    }
}