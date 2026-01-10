using System;
using Tools.Dialogs;
using UnityEngine;
using UnityEngine.UI;

namespace NormalUI
{
    public class TopPanel : MonoBehaviour
    {
        [SerializeField] private Button settingButton;

        private void Start()
        {
            settingButton.onClick.AddListener(OnSettingButtonClick);
        }

        private void OnSettingButtonClick()
        {
            OpenDialogEvent.Trigger("SettingDialog",new SettingDialogContext()
            {
                dialogName =  "SettingDialog",
            });
        }
    }
}