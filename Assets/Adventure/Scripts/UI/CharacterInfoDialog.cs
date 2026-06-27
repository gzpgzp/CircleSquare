using GameFramework;
using TMPro;
using Tools.Dialogs;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.UI
{
    /// <summary>
    /// 角色详细信息弹窗上下文
    /// </summary>
    public class CharacterInfoDialogContext : BaseUIDialogContext
    {
        /// <summary>角色ID</summary>
        public string characterId;
    }

    /// <summary>
    /// 角色详细信息弹窗 - 通过快捷菜单的"信息"按钮打开，显示角色详细属性
    /// 预制体路径: Resources/Prefabs/UI/Dialogs/CharacterInfoDialog
    /// </summary>
    public class CharacterInfoDialog : BaseUIDialog<CharacterInfoDialogContext>
    {
        [Header("基本信息")]
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text classText;
        [SerializeField] private TMP_Text statsText;

        [Header("按钮")]
        [SerializeField] private Button closeBtn;

        protected override void OnShowTyped(CharacterInfoDialogContext context)
        {
            // 填充数据
            var charInfo = ConfigManager.Instance.GetCharacterInfo(context.characterId);
            if (charInfo != null)
            {
                if (nameText != null) nameText.text = charInfo.Name;
                if (levelText != null) levelText.text = "Lv.1";
                if (classText != null) classText.text = GetClassName((cfg.Adventure.ECharacterClass)charInfo.CharacterClass);
                if (statsText != null) statsText.text = $"HP:{charInfo.BaseHp:F0}  ATK:{charInfo.BaseAtk:F0}  DEF:{charInfo.BaseDef:F0}";
            }

            if (closeBtn != null)
                closeBtn.onClick.AddListener(OnCloseClick);
        }

        protected override void OnClose()
        {
            if (closeBtn != null)
                closeBtn.onClick.RemoveAllListeners();
            base.OnClose();
        }

        private string GetClassName(cfg.Adventure.ECharacterClass charClass)
        {
            switch (charClass)
            {
                case cfg.Adventure.ECharacterClass.Warrior: return "战士";
                case cfg.Adventure.ECharacterClass.Mage: return "法师";
                case cfg.Adventure.ECharacterClass.Archer: return "弓手";
                case cfg.Adventure.ECharacterClass.Healer: return "治疗";
                default: return "未知";
            }
        }

        private void OnCloseClick()
        {
            Close();
        }
    }
}
