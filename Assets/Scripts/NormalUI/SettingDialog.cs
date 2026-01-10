using System;
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
        [SerializeField] private Button quitButton;
        [SerializeField] private Button cancelButton;

        private void Start()
        {
            quitButton.onClick.AddListener(OnQuitButtonClick);
            cancelButton.onClick.AddListener(OnCancelButtonClick);
        }

        private void OnQuitButtonClick()
        {
            CloseDialog();   
        }

        private void OnCancelButtonClick()
        {
            CloseDialog();
        }
    }
}