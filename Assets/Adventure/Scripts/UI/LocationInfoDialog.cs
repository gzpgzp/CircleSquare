using Adventure.Location;
using TMPro;
using Tools.Dialogs;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.UI
{
    /// <summary>
    /// 地点信息弹窗 - 右键地点建筑时打开，展示地点基本信息
    /// 预制体路径: Resources/Prefabs/UI/Dialogs/LocationInfoDialog
    /// </summary>
    public class LocationInfoDialog : BaseUIDialog<LocationInfoDialogContext>
    {
        [Header("UI引用")]
        [SerializeField] private TMP_Text titleText;
        [SerializeField] private TMP_Text descriptionText;
        [SerializeField] private TMP_Text typeText;
        [SerializeField] private Button closeBtn;

        protected override void OnShowTyped(LocationInfoDialogContext context)
        {
            if (titleText != null)
                titleText.text = context.locationName ?? "未知地点";

            if (descriptionText != null)
                descriptionText.text = context.description ?? "";

            if (typeText != null)
                typeText.text = $"类型: {GetTypeName((LocationType)context.locationType)}";

            closeBtn.onClick.AddListener(OnCloseClick);
        }

        protected override void OnClose()
        {
            closeBtn.onClick.RemoveAllListeners();
            base.OnClose();
        }

        private string GetTypeName(LocationType type)
        {
            switch (type)
            {
                case LocationType.AdventurerGuild: return "冒险者公会";
                case LocationType.BattleArea: return "战斗区域";
                case LocationType.QuestBoard: return "任务公告板";
                case LocationType.Shop: return "商店";
                case LocationType.Blacksmith: return "铁匠铺";
                case LocationType.Tavern: return "酒馆";
                default: return "未知";
            }
        }

        private void OnCloseClick()
        {
            Close();
        }
    }
}
