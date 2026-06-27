using Tools.EventTool;

namespace Adventure.Inventory
{
    /// <summary>
    /// 道具变化事件 - 添加/移除/数量变化时触发
    /// </summary>
    public struct InventoryChangedEvent
    {
        public string itemId;
        public int newCount;
        public int delta; // 正=获得，负=消耗

        static InventoryChangedEvent e;

        public static void Trigger(string itemId, int newCount, int delta)
        {
            e.itemId = itemId;
            e.newCount = newCount;
            e.delta = delta;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 使用道具事件
    /// </summary>
    public struct ItemUsedEvent
    {
        public string itemId;
        public int itemIntId;

        static ItemUsedEvent e;

        public static void Trigger(string itemId, int itemIntId)
        {
            e.itemId = itemId;
            e.itemIntId = itemIntId;
            MMEventManager.TriggerEvent(e);
        }
    }
}
