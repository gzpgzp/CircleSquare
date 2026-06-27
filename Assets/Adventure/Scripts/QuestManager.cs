using System.Collections.Generic;
using System.Linq;
using GameFramework;
using Tools.EventTool;
using Tools.Singletons;
using UnityEngine;

namespace Adventure
{
    /// <summary>
    /// 任务管理器 - 管理任务的完整生命周期
    /// 职责：加载任务配置、接受任务、追踪进度、完成任务、发放奖励
    /// </summary>
    public class QuestManager : Singleton<QuestManager>, MMEventListener<QuestConditionReportEvent>
    {
        /// <summary>所有任务运行时进度（questId -> progress）</summary>
        private Dictionary<string, QuestProgress> questProgresses = new Dictionary<string, QuestProgress>();

        private bool initialized;

        public void Init()
        {
            if (initialized) return;
            initialized = true;

            LoadQuestProgress();

            // 开始监听条件报告事件
            this.MMEventStartListening<QuestConditionReportEvent>();
        }

        public void Destroy()
        {
            this.MMEventStopListening<QuestConditionReportEvent>();
        }

        #region 存档

        private void LoadQuestProgress()
        {
            var saveData = GetSaveData();
            if (saveData != null)
            {
                foreach (var progress in saveData.questProgresses)
                {
                    questProgresses[progress.questId] = progress;
                }
            }
        }

        private void SaveQuestProgress()
        {
            var saveData = new QuestSaveData();
            saveData.questProgresses = questProgresses.Values.ToList();
            SetSaveData(saveData);
        }

        private QuestSaveData GetSaveData()
        {
            string json = PlayerPrefs.GetString("QuestSaveData", "");
            if (string.IsNullOrEmpty(json)) return null;
            return JsonUtility.FromJson<QuestSaveData>(json);
        }

        private void SetSaveData(QuestSaveData data)
        {
            string json = JsonUtility.ToJson(data);
            PlayerPrefs.SetString("QuestSaveData", json);
            PlayerPrefs.Save();
        }

        #endregion

        #region 公开接口

        /// <summary>
        /// 获取所有可见的任务（可接取 + 进行中 + 已完成未领奖）
        /// </summary>
        public List<cfg.Adventure.QuestInfo> GetAvailableQuests()
        {
            var result = new List<cfg.Adventure.QuestInfo>();
            var allQuests = ConfigManager.Instance.GetAllQuestInfos();

            foreach (var quest in allQuests)
            {
                var status = GetQuestStatus(quest.QuestId);
                if (status == QuestStatus.Rewarded) continue;

                // 检查前置任务
                if (!string.IsNullOrEmpty(quest.PrerequisiteQuestId))
                {
                    var preStatus = GetQuestStatus(quest.PrerequisiteQuestId);
                    if (preStatus != QuestStatus.Rewarded) continue;
                }

                result.Add(quest);
            }
            return result;
        }

        /// <summary>
        /// 获取任务当前状态
        /// </summary>
        public QuestStatus GetQuestStatus(string questId)
        {
            if (questProgresses.TryGetValue(questId, out var progress))
            {
                return progress.status;
            }
            return QuestStatus.Available;
        }

        /// <summary>
        /// 获取任务进度
        /// </summary>
        public QuestProgress GetQuestProgress(string questId)
        {
            questProgresses.TryGetValue(questId, out var progress);
            return progress;
        }

        /// <summary>
        /// 获取任务配置（从Luban表）
        /// </summary>
        public cfg.Adventure.QuestInfo GetQuestConfig(string questId)
        {
            return ConfigManager.Instance.GetQuestInfo(questId);
        }

        /// <summary>
        /// 获取任务条件列表（从Luban表）
        /// </summary>
        public List<cfg.Adventure.QuestConditionCfg> GetQuestConditions(string questId)
        {
            return ConfigManager.Instance.GetQuestConditions(questId);
        }

        /// <summary>
        /// 获取任务奖励列表（从Luban表）
        /// </summary>
        public List<cfg.Adventure.QuestRewardCfg> GetQuestRewards(string questId)
        {
            return ConfigManager.Instance.GetQuestRewards(questId);
        }

        /// <summary>
        /// 接受任务
        /// </summary>
        public bool AcceptQuest(string questId)
        {
            var config = ConfigManager.Instance.GetQuestInfo(questId);
            if (config == null)
            {
                Debug.LogWarning($"[QuestManager] Quest not found: {questId}");
                return false;
            }

            if (GetQuestStatus(questId) != QuestStatus.Available)
            {
                Debug.LogWarning($"[QuestManager] Quest not available: {questId}");
                return false;
            }

            // 创建运行时进度
            var progress = new QuestProgress
            {
                questId = questId,
                status = QuestStatus.InProgress
            };

            var conditions = ConfigManager.Instance.GetQuestConditions(questId);
            foreach (var condition in conditions)
            {
                progress.conditionProgresses.Add(new QuestConditionProgress
                {
                    conditionType = (QuestConditionType)condition.ConditionType,
                    targetId = condition.TargetId,
                    currentAmount = 0,
                    requiredAmount = condition.RequiredAmount
                });
            }

            questProgresses[questId] = progress;
            SaveQuestProgress();

            QuestAcceptedEvent.Trigger(questId);
            Debug.Log($"[QuestManager] Quest accepted: {config.Name}");
            return true;
        }

        /// <summary>
        /// 提交任务并领取奖励
        /// </summary>
        public bool SubmitQuest(string questId)
        {
            if (GetQuestStatus(questId) != QuestStatus.Completed)
            {
                Debug.LogWarning($"[QuestManager] Quest not completed: {questId}");
                return false;
            }

            var config = ConfigManager.Instance.GetQuestInfo(questId);
            if (config == null) return false;

            // 发放奖励
            var rewards = ConfigManager.Instance.GetQuestRewards(questId);
            foreach (var reward in rewards)
            {
                GrantReward(reward);
            }

            // 更新状态
            questProgresses[questId].status = QuestStatus.Rewarded;
            SaveQuestProgress();

            QuestRewardedEvent.Trigger(questId);
            Debug.Log($"[QuestManager] Quest rewarded: {config.Name}");
            return true;
        }

        #endregion

        #region 内部逻辑

        /// <summary>
        /// 处理外部系统报告的条件进度
        /// </summary>
        private void ReportProgress(QuestConditionType conditionType, string targetId, int amount)
        {
            foreach (var kvp in questProgresses)
            {
                var progress = kvp.Value;
                if (progress.status != QuestStatus.InProgress) continue;

                bool changed = false;
                foreach (var cp in progress.conditionProgresses)
                {
                    if (cp.conditionType == conditionType && cp.targetId == targetId && !cp.IsCompleted)
                    {
                        cp.currentAmount = Mathf.Min(cp.currentAmount + amount, cp.requiredAmount);
                        changed = true;

                        QuestProgressUpdateEvent.Trigger(
                            progress.questId, conditionType, targetId,
                            cp.currentAmount, cp.requiredAmount);
                    }
                }

                // 检查是否全部完成
                if (changed && progress.AreAllConditionsMet())
                {
                    progress.status = QuestStatus.Completed;
                    QuestCompletedEvent.Trigger(progress.questId);
                    Debug.Log($"[QuestManager] Quest completed: {progress.questId}");
                }
            }

            SaveQuestProgress();
        }

        private void GrantReward(cfg.Adventure.QuestRewardCfg reward)
        {
            switch ((RewardType)reward.RewardType)
            {
                case RewardType.Gold:
                    Debug.Log($"[QuestManager] Grant gold: {reward.Amount}");
                    break;
                case RewardType.Exp:
                    Debug.Log($"[QuestManager] Grant exp: {reward.Amount}");
                    break;
                case RewardType.Item:
                    Debug.Log($"[QuestManager] Grant item: {reward.RewardId} x{reward.Amount}");
                    break;
                case RewardType.Equipment:
                    Debug.Log($"[QuestManager] Grant equipment: {reward.RewardId}");
                    break;
            }
        }

        #endregion

        #region 事件监听

        public void OnMMEvent(QuestConditionReportEvent eventType)
        {
            ReportProgress(eventType.conditionType, eventType.targetId, eventType.amount);
        }

        #endregion
    }
}
