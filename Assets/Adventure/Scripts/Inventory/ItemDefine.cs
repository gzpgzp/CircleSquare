using System;

namespace Adventure.Inventory
{
    /// <summary>
    /// 道具类型
    /// </summary>
    public enum ItemType
    {
        /// <summary>消耗品（药水、食物等）</summary>
        Consumable,
        /// <summary>装备（武器、防具等）</summary>
        Equipment,
        /// <summary>材料（合成素材）</summary>
        Material,
        /// <summary>任务道具</summary>
        Quest,
        /// <summary>杂物（可出售）</summary>
        Misc
    }

    /// <summary>
    /// 道具品质/稀有度
    /// </summary>
    public enum ItemRarity
    {
        Common,     // 白色
        Uncommon,   // 绿色
        Rare,       // 蓝色
        Epic,       // 紫色
        Legendary   // 橙色
    }

    /// <summary>
    /// 背包中的道具实例（运行时数据）
    /// </summary>
    [Serializable]
    public class InventoryItem
    {
        public string itemId;
        public int count;

        public InventoryItem(string itemId, int count = 1)
        {
            this.itemId = itemId;
            this.count = count;
        }
    }
}
