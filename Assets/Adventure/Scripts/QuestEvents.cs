using Tools.EventTool;

namespace Adventure
{
    /// <summary>
    /// 任务被接受事件
    /// </summary>
    public struct QuestAcceptedEvent
    {
        public string questId;

        static QuestAcceptedEvent e;

        public static void Trigger(string questId)
        {
            e.questId = questId;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 任务条件进度更新事件
    /// </summary>
    public struct QuestProgressUpdateEvent
    {
        public string questId;
        public QuestConditionType conditionType;
        public string targetId;
        public int currentAmount;
        public int requiredAmount;

        static QuestProgressUpdateEvent e;

        public static void Trigger(string questId, QuestConditionType conditionType, string targetId,
            int currentAmount, int requiredAmount)
        {
            e.questId = questId;
            e.conditionType = conditionType;
            e.targetId = targetId;
            e.currentAmount = currentAmount;
            e.requiredAmount = requiredAmount;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 任务完成（条件全部满足）事件
    /// </summary>
    public struct QuestCompletedEvent
    {
        public string questId;

        static QuestCompletedEvent e;

        public static void Trigger(string questId)
        {
            e.questId = questId;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 任务奖励领取事件
    /// </summary>
    public struct QuestRewardedEvent
    {
        public string questId;

        static QuestRewardedEvent e;

        public static void Trigger(string questId)
        {
            e.questId = questId;
            MMEventManager.TriggerEvent(e);
        }
    }

    /// <summary>
    /// 外部系统向任务系统报告进度（如击杀、拾取等）
    /// </summary>
    public struct QuestConditionReportEvent
    {
        public QuestConditionType conditionType;
        public string targetId;
        public int amount;

        static QuestConditionReportEvent e;

        public static void Trigger(QuestConditionType conditionType, string targetId, int amount = 1)
        {
            e.conditionType = conditionType;
            e.targetId = targetId;
            e.amount = amount;
            MMEventManager.TriggerEvent(e);
        }
    }
}
