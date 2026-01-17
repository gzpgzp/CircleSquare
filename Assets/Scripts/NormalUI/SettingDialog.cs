using GameFramework;
using Sounds;
using Tools.Dialogs;
using UnityEngine;
using UnityEngine.UI;

namespace NormalUI
{
    public class SettingDialogContext : BaseUIDialogContext
    {
        
    }

    public class SettingDialog : BaseUIDialog<SettingDialogContext>
    {
        [SerializeField] private Button closeBtn;
        [SerializeField] private Button menuBtn;
        [SerializeField] private SoundItem bgmVolumeSlider;
        [SerializeField] private SoundItem musicVolumeSlider;

        private void Start()
        {
            closeBtn.onClick.AddListener(OnCloseButtonClick);
            menuBtn.onClick.AddListener(OnMenuButtonClick);
        }

        private void OnDestroy()
        {
            closeBtn.onClick.RemoveAllListeners();
            menuBtn.onClick.RemoveAllListeners();
        }

        private void OnCloseButtonClick()
        {
            Close();
        }

        private void OnMenuButtonClick()
        {
            Close();
            GameStopEvent.Trigger();
        }

        protected override void OnShowTyped(SettingDialogContext context)
        {
            
        }
    }
}