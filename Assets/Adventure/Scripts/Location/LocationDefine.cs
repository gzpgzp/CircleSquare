namespace Adventure.Location
{
    /// <summary>
    /// 地点类型 - 每种地点对应不同的功能
    /// </summary>
    public enum LocationType
    {
        /// <summary>冒险者公会 - 查看角色列表、编队</summary>
        AdventurerGuild,
        /// <summary>战斗区域 - 显示敌人、开始挂机战斗</summary>
        BattleArea,
        /// <summary>任务公告板 - 打开任务列表</summary>
        QuestBoard,
        /// <summary>商店 - 买卖物品</summary>
        Shop,
        /// <summary>铁匠铺 - 装备强化</summary>
        Blacksmith,
        /// <summary>酒馆 - 招募角色</summary>
        Tavern
    }

    /// <summary>
    /// 地点ID枚举 - 与Luban表的 LocationId 字符串一一对应
    /// 在Inspector中选择枚举值，运行时自动转换为字符串ID查表
    /// </summary>
    public enum ELocationId
    {
        Guild,
        Forest,
        QuestBoard,
        Shop,
        Blacksmith,
        Tavern,
        Cave,
        Volcano
    }

    public static class LocationIdMapper
    {
        /// <summary>
        /// 枚举转字符串ID（对应Luban表的LocationId字段）
        /// </summary>
        public static string ToStringId(this ELocationId id)
        {
            switch (id)
            {
                case ELocationId.Guild: return "guild";
                case ELocationId.Forest: return "forest";
                case ELocationId.QuestBoard: return "quest_board";
                case ELocationId.Shop: return "shop";
                case ELocationId.Blacksmith: return "blacksmith";
                case ELocationId.Tavern: return "tavern";
                case ELocationId.Cave: return "cave";
                case ELocationId.Volcano: return "volcano";
                default: return id.ToString().ToLower();
            }
        }
    }
}
