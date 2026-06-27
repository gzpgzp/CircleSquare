using GameFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.Inventory
{
    /// <summary>
    /// 背包中单个道具的UI显示
    /// </summary>
    public class InventoryItemUI : MonoBehaviour
    {
        [SerializeField] private Image iconImage;
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text countText;
        [SerializeField] private TMP_Text rarityText;
        [SerializeField] private Image rarityBg;
        [SerializeField] private Button useBtn;

        private InventoryItem itemData;

        public void Setup(InventoryItem data)
        {
            itemData = data;
            var cfg = ConfigManager.Instance.GetItemInfo(data.itemId);
            if (cfg == null) return;

            // 名称
            if (nameText != null)
                nameText.text = cfg.Name;

            // 数量
            if (countText != null)
                countText.text = data.count > 1 ? $"x{data.count}" : "";

            // 稀有度
            if (rarityText != null)
                rarityText.text = GetRarityName((ItemRarity)cfg.Rarity);

            if (rarityBg != null)
                rarityBg.color = GetRarityColor((ItemRarity)cfg.Rarity);

            // 使用按钮（仅消耗品显示）
            if (useBtn != null)
            {
                bool canUse = cfg.ItemType == (int)ItemType.Consumable;
                useBtn.gameObject.SetActive(canUse);
                if (canUse)
                {
                    useBtn.onClick.RemoveAllListeners();
                    useBtn.onClick.AddListener(OnUseClick);
                }
            }
        }

        private void OnUseClick()
        {
            if (itemData != null)
            {
                InventoryManager.Instance.UseItem(itemData.itemId);
            }
        }

        private string GetRarityName(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return "普通";
                case ItemRarity.Uncommon: return "优秀";
                case ItemRarity.Rare: return "稀有";
                case ItemRarity.Epic: return "史诗";
                case ItemRarity.Legendary: return "传说";
                default: return "";
            }
        }

        private Color GetRarityColor(ItemRarity rarity)
        {
            switch (rarity)
            {
                case ItemRarity.Common: return new Color(0.8f, 0.8f, 0.8f);
                case ItemRarity.Uncommon: return new Color(0.2f, 0.8f, 0.2f);
                case ItemRarity.Rare: return new Color(0.2f, 0.4f, 1f);
                case ItemRarity.Epic: return new Color(0.6f, 0.2f, 0.9f);
                case ItemRarity.Legendary: return new Color(1f, 0.6f, 0f);
                default: return Color.white;
            }
        }
    }
}
