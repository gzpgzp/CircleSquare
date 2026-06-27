using Adventure.Location;
using TMPro;
using Tools.Dialogs;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.UI
{
    /// <summary>
    /// 战斗区域弹窗 - 显示区域信息，提供开始战斗/挂机按钮
    /// 预制体路径: Resources/Prefabs/UI/Dialogs/BattleAreaDialog
    /// </summary>
    public class BattleAreaDialog : BaseUIDialog<BattleAreaDialogContext>
    {
        [Header("UI引用")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text enemyInfoText;
        [SerializeField] private Button startBattleBtn;
        [SerializeField] private TMP_Text startBattleBtnText;
        [SerializeField] private Button closeBtn;

        protected override void OnShowTyped(BattleAreaDialogContext context)
        {
            if (titleText != null)
                titleText.text = context.areaName ?? "战斗区域";

            if (descriptionText != null)
                descriptionText.text = context.areaDescription ?? "";

            if (enemyInfoText != null)
                enemyInfoText.text = "敌人信息加载中...";

            if (startBattleBtnText != null)
                startBattleBtnText.text = "开始战斗";

            closeBtn.onClick.AddListener(OnCloseClick);
            startBattleBtn.onClick.AddListener(OnStartBattle);

            RefreshEnemyInfo();
        }

        protected override void OnClose()
        {
            closeBtn.onClick.RemoveAllListeners();
            startBattleBtn.onClick.RemoveAllListeners();
            base.OnClose();
        }

        private void RefreshEnemyInfo()
        {
            // 从配置获取该区域的敌人信息
            var allChars = GameFramework.ConfigManager.Instance.GetAllCharacterInfos();
            var sb = new System.Text.StringBuilder();
            sb.AppendLine("<b>出没敌人:</b>");

            foreach (var charInfo in allChars)
            {
                if (!charInfo.CharacterId.StartsWith("enemy_")) continue;
                sb.AppendLine($"  · {charInfo.Name}  HP:{charInfo.BaseHp:F0} ATK:{charInfo.BaseAtk:F0}");
            }

            if (enemyInfoText != null)
                enemyInfoText.text = sb.ToString();
        }

        private void OnStartBattle()
        {
            Debug.Log("[BattleAreaDialog] Start battle!");
            // TODO: 通知战斗系统开始/继续挂机
            Close();
        }

        private void OnCloseClick()
        {
            Close();
        }
    }
}
