using System.Collections.Generic;
using GameFramework;
using Tools.Singletons;
using UnityEngine;

namespace Adventure.Inventory
{
    /// <summary>
    /// 背包管理器 - 管理玩家拥有的所有道具
    /// 道具配置从Luban表读取
    /// </summary>
    public class InventoryManager : Singleton<InventoryManager>
    {
        /// <summary>当前背包中的道具列表</summary>
        private List<InventoryItem> items = new List<InventoryItem>();

        /// <summary>是否已初始化</summary>
        private bool initialized;

        public void Init()
        {
            if (initialized) return;
            initialized = true;
            Debug.Log("[InventoryManager] Initialized.");
        }

        /// <summary>
        /// 获取当前背包所有道具
        /// </summary>
        public IReadOnlyList<InventoryItem> GetAllItems()
        {
            return items;
        }

        /// <summary>
        /// 获取指定道具的数量
        /// </summary>
        public int GetItemCount(string itemId)
        {
            var item = items.Find(i => i.itemId == itemId);
            return item?.count ?? 0;
        }

        /// <summary>
        /// 是否拥有指定道具
        /// </summary>
        public bool HasItem(string itemId, int requiredCount = 1)
        {
            return GetItemCount(itemId) >= requiredCount;
        }

        /// <summary>
        /// 添加道具
        /// </summary>
        public bool AddItem(string itemId, int amount = 1)
        {
            if (amount <= 0) return false;

            var itemCfg = ConfigManager.Instance.GetItemInfo(itemId);
            if (itemCfg == null)
            {
                Debug.LogWarning($"[InventoryManager] Item config not found: {itemId}");
                return false;
            }

            var existing = items.Find(i => i.itemId == itemId);
            if (existing != null)
            {
                int maxStack = itemCfg.MaxStack;
                int maxAdd = maxStack - existing.count;
                int actualAdd = Mathf.Min(amount, maxAdd);
                if (actualAdd <= 0)
                {
                    Debug.Log($"[InventoryManager] Item {itemId} stack full ({maxStack})");
                    return false;
                }
                existing.count += actualAdd;
                InventoryChangedEvent.Trigger(itemId, existing.count, actualAdd);
            }
            else
            {
                int maxStack = itemCfg.MaxStack;
                int actualAdd = Mathf.Min(amount, maxStack);
                items.Add(new InventoryItem(itemId, actualAdd));
                InventoryChangedEvent.Trigger(itemId, actualAdd, actualAdd);
            }

            Debug.Log($"[InventoryManager] Added {amount}x {itemCfg.Name}");
            return true;
        }

        /// <summary>
        /// 移除道具
        /// </summary>
        public bool RemoveItem(string itemId, int amount = 1)
        {
            if (amount <= 0) return false;

            var existing = items.Find(i => i.itemId == itemId);
            if (existing == null || existing.count < amount)
            {
                Debug.LogWarning($"[InventoryManager] Not enough item to remove: {itemId}");
                return false;
            }

            existing.count -= amount;
            if (existing.count <= 0)
            {
                items.Remove(existing);
                InventoryChangedEvent.Trigger(itemId, 0, -amount);
            }
            else
            {
                InventoryChangedEvent.Trigger(itemId, existing.count, -amount);
            }

            return true;
        }

        /// <summary>
        /// 使用消耗品
        /// </summary>
        public bool UseItem(string itemId)
        {
            var itemCfg = ConfigManager.Instance.GetItemInfo(itemId);
            if (itemCfg == null)
            {
                Debug.LogWarning($"[InventoryManager] Cannot use item: {itemId}");
                return false;
            }

            // 只有消耗品可用
            if (itemCfg.ItemType != (int)ItemType.Consumable)
            {
                Debug.LogWarning($"[InventoryManager] Item is not consumable: {itemId}");
                return false;
            }

            if (!HasItem(itemId))
            {
                Debug.LogWarning($"[InventoryManager] No item to use: {itemId}");
                return false;
            }

            RemoveItem(itemId, 1);
            ItemUsedEvent.Trigger(itemId, itemCfg.Id);
            Debug.Log($"[InventoryManager] Used item: {itemCfg.Name}");
            return true;
        }

        /// <summary>
        /// 清空背包
        /// </summary>
        public void ClearAll()
        {
            items.Clear();
        }

        /// <summary>
        /// 从存档加载背包数据
        /// </summary>
        public void LoadFromSave(List<InventoryItem> savedItems)
        {
            items = savedItems ?? new List<InventoryItem>();
        }

        /// <summary>
        /// 导出背包数据用于存档
        /// </summary>
        public List<InventoryItem> ExportForSave()
        {
            return new List<InventoryItem>(items);
        }
    }
}
