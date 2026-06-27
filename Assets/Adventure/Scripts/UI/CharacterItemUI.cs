using TMPro;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.UI
{
    /// <summary>
    /// 角色列表项UI - 展示单个角色的摘要信息
    /// 挂在 CharacterItem 预制体上
    /// </summary>
    public class CharacterItemUI : MonoBehaviour
    {
        [SerializeField] private TMP_Text nameText;
        [SerializeField] private TMP_Text classText;
        [SerializeField] private TMP_Text levelText;
        [SerializeField] private TMP_Text statsText;
        [SerializeField] private Image iconImage;

        private cfg.Adventure.CharacterInfo charInfo;

        public void Setup(cfg.Adventure.CharacterInfo info)
        {
            charInfo = info;

            if (nameText != null)
                nameText.text = info.Name;

            if (classText != null)
                classText.text = GetClassName((cfg.Adventure.ECharacterClass)info.CharacterClass);

            if (levelText != null)
                levelText.text = "Lv.1"; // TODO: 从存档获取实际等级

            if (statsText != null)
                statsText.text = $"HP:{info.BaseHp:F0}  ATK:{info.BaseAtk:F0}  DEF:{info.BaseDef:F0}";
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
    }
}
