using System;
using System.Collections.Generic;
using System.Text;
using GameFramework;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.UI
{
    /// <summary>
    /// 任务列表中的单个任务项（支持直接接受/提交操作）
    /// </summary>
    public class QuestItemUI : MonoBehaviour
    {
        [Header("基本信息")]
        [SerializeField] private TMP_Text questNameText;
        [SerializeField] private TMP_Text statusText;
        [SerializeField] private TMP_Text conditionText;
        [SerializeField] private TMP_Text rewardText;

        [Header("按钮")]
        [SerializeField] private Button actionBtn;
        [SerializeField] private TMP_Text actionBtnText;
        [SerializeField] private Button detailBtn;

        private string questId;
        private Action<string> onDetailCallback;

        public void Setup(cfg.Adventure.QuestInfo config, QuestStatus status, Action<string> onDetail)
        {
            questId = config.QuestId;
            onDetailCallback = onDetail;

            // 基本信息
            questNameText.text = config.Name;
            statusText.text = GetStatusText(status);

            // 条件摘要
            conditionText.text = GetConditionSummary(config.QuestId, status);

            // 奖励摘要
            rewardText.text = GetRewardSummary(config.QuestId);

            // 操作按钮
            RefreshActionButton(status);

            // 详情按钮（打开DetailDialog查看完整信息）
            if (detailBtn != null)
            {
                detailBtn.onClick.RemoveAllListeners();
                detailBtn.onClick.AddListener(OnDetailClick);
            }
        }

        private void RefreshActionButton(QuestStatus status)
        {
            actionBtn.onClick.RemoveAllListeners();

            switch (status)
            {
                case QuestStatus.Available:
                    actionBtn.gameObject.SetActive(true);
                    actionBtn.interactable = true;
                    actionBtnText.text = "接受";
                    actionBtn.onClick.AddListener(OnAcceptClick);
                    break;
                case QuestStatus.InProgress:
                    actionBtn.gameObject.SetActive(true);
                    actionBtn.interactable = false;
                    actionBtnText.text = "进行中";
                    break;
                case QuestStatus.Completed:
                    actionBtn.gameObject.SetActive(true);
                    actionBtn.interactable = true;
                    actionBtnText.text = "提交";
                    actionBtn.onClick.AddListener(OnSubmitClick);
                    break;
                case QuestStatus.Rewarded:
                    actionBtn.gameObject.SetActive(false);
                    break;
            }
        }

        private void OnAcceptClick()
        {
            QuestManager.Instance.AcceptQuest(questId);
        }

        private void OnSubmitClick()
        {
            QuestManager.Instance.SubmitQuest(questId);
        }

        private void OnDetailClick()
        {
            onDetailCallback?.Invoke(questId);
        }

        private string GetConditionSummary(string questId, QuestStatus status)
        {
            var sb = new StringBuilder();
            if (status == QuestStatus.InProgress || status == QuestStatus.Completed)
            {
                var progress = QuestManager.Instance.GetQuestProgress(questId);
                if (progress != null)
                {
                    foreach (var cp in progress.conditionProgresses)
                    {
                        string mark = cp.IsCompleted ? "\u2713" : "\u25cb";
                        sb.Append($"{mark} {cp.currentAmount}/{cp.requiredAmount}  ");
                    }
                    return sb.ToString().TrimEnd();
                }
            }

            // Available状态：显示条件概要
            var conditions = ConfigManager.Instance.GetQuestConditions(questId);
            foreach (var cond in conditions)
            {
                var type = (QuestConditionType)cond.ConditionType;
                sb.Append($"\u25cb {type}:{cond.TargetId}  ");
            }
            return sb.ToString().TrimEnd();
        }

        private string GetRewardSummary(string questId)
        {
            var rewards = ConfigManager.Instance.GetQuestRewards(questId);
            if (rewards.Count == 0) return "";

            var sb = new StringBuilder();
            foreach (var reward in rewards)
            {
                switch ((RewardType)reward.RewardType)
                {
                    case RewardType.Gold:
                        sb.Append($"金币x{reward.Amount} ");
                        break;
                    case RewardType.Exp:
                        sb.Append($"经验x{reward.Amount} ");
                        break;
                    case RewardType.Item:
                        sb.Append($"{reward.RewardId}x{reward.Amount} ");
                        break;
                    case RewardType.Equipment:
                        sb.Append($"{reward.RewardId} ");
                        break;
                }
            }
            return sb.ToString().TrimEnd();
        }

        private string GetStatusText(QuestStatus status)
        {
            switch (status)
            {
                case QuestStatus.Available: return "可接取";
                case QuestStatus.InProgress: return "进行中";
                case QuestStatus.Completed: return "可提交";
                case QuestStatus.Rewarded: return "已完成";
                default: return "";
            }
        }
    }
}
