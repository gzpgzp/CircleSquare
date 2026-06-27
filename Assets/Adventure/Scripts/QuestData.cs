using System;
using System.Collections.Generic;
using UnityEngine;

namespace Adventure
{
    /// <summary>
    /// 单个任务条件配置
    /// </summary>
    [Serializable]
    public class QuestCondition
    {
        public QuestConditionType conditionType;
        /// <summary>条件目标标识（如敌人ID、物品ID、区域ID）</summary>
        public string targetId;
        /// <summary>需要达成的数量</summary>
        public int requiredAmount = 1;
        /// <summary>条件描述</summary>
        public string description;
    }

    /// <summary>
    /// 任务奖励配置
    /// </summary>
    [Serializable]
    public class QuestReward
    {
        public RewardType rewardType;
        /// <summary>奖励物品ID（Gold/Exp时为空）</summary>
        public string itemId;
        /// <summary>奖励数量</summary>
        public int amount;
    }

    /// <summary>
    /// 单个条件的运行时进度
    /// </summary>
    [Serializable]
    public class QuestConditionProgress
    {
        public QuestConditionType conditionType;
        public string targetId;
        public int currentAmount;
        public int requiredAmount;

        public bool IsCompleted => currentAmount >= requiredAmount;
    }

    /// <summary>
    /// 任务运行时数据（可存档）
    /// </summary>
    [Serializable]
    public class QuestProgress
    {
        public string questId;
        public QuestStatus status;
        public List<QuestConditionProgress> conditionProgresses = new List<QuestConditionProgress>();

        /// <summary>
        /// 检查所有条件是否达成
        /// </summary>
        public bool AreAllConditionsMet()
        {
            foreach (var cp in conditionProgresses)
            {
                if (!cp.IsCompleted) return false;
            }
            return true;
        }
    }

    /// <summary>
    /// 任务存档数据集合
    /// </summary>
    [Serializable]
    public class QuestSaveData
    {
        public List<QuestProgress> questProgresses = new List<QuestProgress>();
    }
}
