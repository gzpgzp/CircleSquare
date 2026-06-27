namespace Adventure
{
    /// <summary>
    /// 任务状态
    /// </summary>
    public enum QuestStatus
    {
        /// <summary>未接取，可见</summary>
        Available,
        /// <summary>已接取，进行中</summary>
        InProgress,
        /// <summary>条件已满足，可提交</summary>
        Completed,
        /// <summary>已提交并领奖</summary>
        Rewarded
    }

    /// <summary>
    /// 任务条件类型
    /// </summary>
    public enum QuestConditionType
    {
        /// <summary>击杀指定数量的敌人</summary>
        KillEnemy,
        /// <summary>收集指定物品</summary>
        CollectItem,
        /// <summary>到达指定区域</summary>
        ReachArea,
        /// <summary>与NPC对话</summary>
        TalkToNPC,
        /// <summary>通关指定关卡</summary>
        ClearLevel
    }

    /// <summary>
    /// 奖励类型
    /// </summary>
    public enum RewardType
    {
        Gold,
        Exp,
        Item,
        Equipment
    }
}
