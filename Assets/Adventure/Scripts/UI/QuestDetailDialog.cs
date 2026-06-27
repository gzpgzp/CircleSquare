using System.Collections.Generic;
using System.Text;
using GameFramework;
using TMPro;
using Tools.Dialogs;
using Tools.EventTool;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.UI
{
    public class QuestDetailDialogContext : BaseUIDialogContext
    {
        public string questId;
    }

    /// <summary>
    /// 任务详情弹窗 - 展示任务信息、条件进度、奖励，以及接受/提交按钮
    /// 预制体路径: Resources/Prefabs/UI/Dialogs/QuestDetailDialog
    /// </summary>
    public class QuestDetailDialog : BaseUIDialog<QuestDetailDialogContext>,
        MMEventListener<QuestProgressUpdateEvent>
    {
        [Header("基本信息")]
        [SerializeField] private TMP_Text questNameText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text levelText;

        [Header("条件区域")]
        [SerializeField] private TMP_Text conditionsText;

        [Header("奖励区域")]
        [SerializeField] private TMP_Text rewardsText;

        [Header("按钮")]
        [SerializeField] private Button acceptBtn;
        [SerializeField] private TMP_Text acceptBtnText;
        [SerializeField] private Button closeBtn;

        private string questId;
        private cfg.Adventure.QuestInfo config;
        private List<cfg.Adventure.QuestConditionCfg> conditions;
        private List<cfg.Adventure.QuestRewardCfg> rewards;

        protected override void OnShowTyped(QuestDetailDialogContext context)
        {
            questId = context.questId;
            config = ConfigManager.Instance.GetQuestInfo(questId);

            if (config == null)
            {
                Debug.LogWarning($"[QuestDetailDialog] Config not found for: {questId}");
                Close();
                return;
            }

            conditions = ConfigManager.Instance.GetQuestConditions(questId);
            rewards = ConfigManager.Instance.GetQuestRewards(questId);

            closeBtn.onClick.AddListener(OnCloseClick);
            acceptBtn.onClick.AddListener(OnAcceptClick);

            this.MMEventStartListening<QuestProgressUpdateEvent>();

            RefreshUI();
        }

        protected override void OnClose()
        {
            closeBtn.onClick.RemoveAllListeners();
            acceptBtn.onClick.RemoveAllListeners();
            this.MMEventStopListening<QuestProgressUpdateEvent>();
            base.OnClose();
        }

        private void RefreshUI()
        {
            var status = QuestManager.Instance.GetQuestStatus(questId);

            // 基本信息
            questNameText.text = config.Name;
            descriptionText.text = config.Desc;
            levelText.text = $"推荐等级: Lv.{config.RecommendLevel}";

            // 条件
            RefreshConditions(status);

            // 奖励
            RefreshRewards();

            // 按钮状态
            RefreshButton(status);
        }

        private void RefreshConditions(QuestStatus status)
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>任务目标:</b>");

            if (status == QuestStatus.Available)
            {
                // 未接取，显示配置条件
                foreach (var condition in conditions)
                {
                    string desc = GetConditionDescription(condition);
                    sb.AppendLine($"  · {desc} (0/{condition.RequiredAmount})");
                }
            }
            else
            {
                // 已接取，显示实时进度
                var progress = QuestManager.Instance.GetQuestProgress(questId);
                if (progress != null)
                {
                    for (int i = 0; i < progress.conditionProgresses.Count; i++)
                    {
                        var cp = progress.conditionProgresses[i];
                        string desc = i < conditions.Count
                            ? GetConditionDescription(conditions[i])
                            : cp.conditionType.ToString();
                        string checkMark = cp.IsCompleted ? "✓" : "○";
                        sb.AppendLine($"  {checkMark} {desc} ({cp.currentAmount}/{cp.requiredAmount})");
                    }
                }
            }

            conditionsText.text = sb.ToString();
        }

        private void RefreshRewards()
        {
            var sb = new StringBuilder();
            sb.AppendLine("<b>任务奖励:</b>");

            foreach (var reward in rewards)
            {
                string rewardDesc = GetRewardDescription(reward);
                sb.AppendLine($"  · {rewardDesc}");
            }

            rewardsText.text = sb.ToString();
        }

        private void RefreshButton(QuestStatus status)
        {
            switch (status)
            {
                case QuestStatus.Available:
                    acceptBtn.gameObject.SetActive(true);
                    acceptBtn.interactable = true;
                    acceptBtnText.text = "接受任务";
                    break;
                case QuestStatus.InProgress:
                    acceptBtn.gameObject.SetActive(true);
                    acceptBtn.interactable = false;
                    acceptBtnText.text = "进行中...";
                    break;
                case QuestStatus.Completed:
                    acceptBtn.gameObject.SetActive(true);
                    acceptBtn.interactable = true;
                    acceptBtnText.text = "提交任务";
                    break;
                case QuestStatus.Rewarded:
                    acceptBtn.gameObject.SetActive(false);
                    break;
            }
        }

        private void OnAcceptClick()
        {
            var status = QuestManager.Instance.GetQuestStatus(questId);

            if (status == QuestStatus.Available)
            {
                QuestManager.Instance.AcceptQuest(questId);
                RefreshUI();
            }
            else if (status == QuestStatus.Completed)
            {
                QuestManager.Instance.SubmitQuest(questId);
                RefreshUI();
            }
        }

        private void OnCloseClick()
        {
            Close();
        }

        private string GetConditionDescription(cfg.Adventure.QuestConditionCfg condition)
        {
            var type = (QuestConditionType)condition.ConditionType;
            switch (type)
            {
                case QuestConditionType.KillEnemy:
                    return $"击败 {condition.TargetId} x{condition.RequiredAmount}";
                case QuestConditionType.CollectItem:
                    return $"收集 {condition.TargetId} x{condition.RequiredAmount}";
                case QuestConditionType.ReachArea:
                    return $"前往 {condition.TargetId}";
                case QuestConditionType.TalkToNPC:
                    return $"与 {condition.TargetId} 对话";
                case QuestConditionType.ClearLevel:
                    return $"通关 {condition.TargetId}";
                default:
                    return $"{type}: {condition.TargetId} x{condition.RequiredAmount}";
            }
        }

        private string GetRewardDescription(cfg.Adventure.QuestRewardCfg reward)
        {
            switch ((RewardType)reward.RewardType)
            {
                case RewardType.Gold:
                    return $"金币 x{reward.Amount}";
                case RewardType.Exp:
                    return $"经验 x{reward.Amount}";
                case RewardType.Item:
                    return $"物品[{reward.RewardId}] x{reward.Amount}";
                case RewardType.Equipment:
                    return $"装备[{reward.RewardId}]";
                default:
                    return "未知奖励";
            }
        }

        #region 进度事件

        public void OnMMEvent(QuestProgressUpdateEvent eventType)
        {
            if (eventType.questId == questId)
            {
                RefreshUI();
            }
        }

        #endregion
    }
}
