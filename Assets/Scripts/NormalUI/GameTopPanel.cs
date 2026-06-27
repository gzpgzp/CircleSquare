using Adventure.Inventory;
using Tools.Dialogs;
using UnityEngine;
using UnityEngine.UI;

namespace NormalUI
{
    public class GameTopPanel : MonoBehaviour
    {
        [SerializeField] private Button settingButton;
        [SerializeField] private Button inventoryButton;

        private void Start()
        {
            settingButton.onClick.AddListener(OnSettingButtonClick);
            if (inventoryButton != null)
                inventoryButton.onClick.AddListener(OnInventoryButtonClick);
        }

        private void OnDestroy()
        {
            settingButton.onClick.RemoveListener(OnSettingButtonClick);
            if (inventoryButton != null)
                inventoryButton.onClick.RemoveListener(OnInventoryButtonClick);
        }

        private void OnSettingButtonClick()
        {
            OpenDialogEvent.Trigger("SettingDialog", new SettingDialogContext()
            {
                dialogName = "SettingDialog",
            });
        }

        private void OnInventoryButtonClick()
        {
            OpenDialogEvent.Trigger("InventoryDialog", new InventoryDialogContext());
        }
    }
}