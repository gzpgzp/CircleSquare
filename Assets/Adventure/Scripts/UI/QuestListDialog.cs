using System.Collections.Generic;
using Tools.Dialogs;
using Tools.EventTool;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.UI
{
    public class QuestListDialogContext : BaseUIDialogContext
    {
    }

    /// <summary>
    /// 任务列表弹窗 - 展示所有可接取和进行中的任务
    /// 预制体路径: Resources/Prefabs/UI/Dialogs/QuestListDialog
    /// </summary>
    public class QuestListDialog : BaseUIDialog<QuestListDialogContext>,
        MMEventListener<QuestAcceptedEvent>,
        MMEventListener<QuestCompletedEvent>,
        MMEventListener<QuestRewardedEvent>
    {
        [SerializeField] private Transform questListContent;
        [SerializeField] private GameObject questItemPrefab;
        [SerializeField] private Button closeBtn;

        private List<QuestItemUI> questItems = new List<QuestItemUI>();

        protected override void OnShowTyped(QuestListDialogContext context)
        {
            closeBtn.onClick.AddListener(OnCloseClick);
            RefreshQuestList();

            this.MMEventStartListening<QuestAcceptedEvent>();
            this.MMEventStartListening<QuestCompletedEvent>();
            this.MMEventStartListening<QuestRewardedEvent>();
        }

        protected override void OnClose()
        {
            closeBtn.onClick.RemoveAllListeners();
            this.MMEventStopListening<QuestAcceptedEvent>();
            this.MMEventStopListening<QuestCompletedEvent>();
            this.MMEventStopListening<QuestRewardedEvent>();
            base.OnClose();
        }

        private void RefreshQuestList()
        {
            // 清理旧列表
            foreach (var item in questItems)
            {
                if (item != null) Destroy(item.gameObject);
            }
            questItems.Clear();

            // 获取可见任务
            var quests = QuestManager.Instance.GetAvailableQuests();

            foreach (var config in quests)
            {
                var go = Instantiate(questItemPrefab, questListContent);
                var itemUI = go.GetComponent<QuestItemUI>();
                if (itemUI != null)
                {
                    var status = QuestManager.Instance.GetQuestStatus(config.QuestId);
                    itemUI.Setup(config, status, OnQuestDetailClicked);
                    questItems.Add(itemUI);
                }
            }
        }

        private void OnQuestDetailClicked(string questId)
        {
            // 点击详情按钮时打开任务详情弹窗
            OpenDialogEvent.Trigger("QuestDetailDialog", new QuestDetailDialogContext
            {
                questId = questId
            });
        }

        private void OnCloseClick()
        {
            Close();
        }

        #region 事件刷新

        public void OnMMEvent(QuestAcceptedEvent eventType)
        {
            RefreshQuestList();
        }

        public void OnMMEvent(QuestCompletedEvent eventType)
        {
            RefreshQuestList();
        }

        public void OnMMEvent(QuestRewardedEvent eventType)
        {
            RefreshQuestList();
        }

        #endregion
    }
}
