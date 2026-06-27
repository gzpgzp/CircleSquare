using System.Collections.Generic;
using GameFramework;
using TMPro;
using Tools.Dialogs;
using Tools.EventTool;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.Inventory
{
    public class InventoryDialogContext : BaseUIDialogContext
    {
    }

    /// <summary>
    /// 背包弹窗 - 展示玩家拥有的所有道具
    /// 预制体路径: Resources/Prefabs/UI/Dialogs/InventoryDialog
    /// </summary>
    public class InventoryDialog : BaseUIDialog<InventoryDialogContext>,
        MMEventListener<InventoryChangedEvent>
    {
        [Header("UI引用")]
        [SerializeField] private Transform itemListContent;
        [SerializeField] private GameObject itemPrefab;
        [SerializeField] private Button closeBtn;
        [SerializeField] private TMP_Text titleText;

        [Header("筛选")]
        [SerializeField] private Button filterAllBtn;
        [SerializeField] private Button filterConsumableBtn;
        [SerializeField] private Button filterEquipmentBtn;
        [SerializeField] private Button filterMaterialBtn;

        private List<InventoryItemUI> itemUIList = new List<InventoryItemUI>();
        private ItemType? currentFilter = null;

        protected override void OnShowTyped(InventoryDialogContext context)
        {
            if (titleText != null)
                titleText.text = "背包";

            closeBtn.onClick.AddListener(OnCloseClick);
            SetupFilters();
            RefreshList();

            this.MMEventStartListening<InventoryChangedEvent>();
        }

        protected override void OnClose()
        {
            closeBtn.onClick.RemoveAllListeners();
            this.MMEventStopListening<InventoryChangedEvent>();
            base.OnClose();
        }

        private void SetupFilters()
        {
            if (filterAllBtn != null)
                filterAllBtn.onClick.AddListener(() => SetFilter(null));
            if (filterConsumableBtn != null)
                filterConsumableBtn.onClick.AddListener(() => SetFilter(ItemType.Consumable));
            if (filterEquipmentBtn != null)
                filterEquipmentBtn.onClick.AddListener(() => SetFilter(ItemType.Equipment));
            if (filterMaterialBtn != null)
                filterMaterialBtn.onClick.AddListener(() => SetFilter(ItemType.Material));
        }

        private void SetFilter(ItemType? type)
        {
            currentFilter = type;
            RefreshList();
        }

        private void RefreshList()
        {
            // 清除旧UI
            foreach (var ui in itemUIList)
            {
                if (ui != null)
                    Destroy(ui.gameObject);
            }
            itemUIList.Clear();

            // 获取背包数据
            var items = InventoryManager.Instance.GetAllItems();

            foreach (var item in items)
            {
                var cfg = ConfigManager.Instance.GetItemInfo(item.itemId);
                if (cfg == null) continue;

                // 筛选
                if (currentFilter.HasValue && cfg.ItemType != (int)currentFilter.Value)
                    continue;

                // 创建UI
                var go = Instantiate(itemPrefab, itemListContent);
                var itemUI = go.GetComponent<InventoryItemUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(item);
                    itemUIList.Add(itemUI);
                }
            }
        }

        private void OnCloseClick()
        {
            Close();
        }

        public void OnMMEvent(InventoryChangedEvent evt)
        {
            RefreshList();
        }
    }
}
