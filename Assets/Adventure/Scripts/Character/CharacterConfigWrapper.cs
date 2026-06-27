using GameFramework;
using UnityEngine;

namespace Adventure.Character
{
    /// <summary>
    /// 角色配置包装器 - 从Luban表读取配置，提供计算方法
    /// </summary>
    public class CharacterConfigWrapper
    {
        private cfg.Adventure.CharacterInfo data;

        public string CharacterId => data.CharacterId;
        public string CharacterName => data.Name;
        public int CharacterClass => data.CharacterClass;
        public float BaseHp => data.BaseHp;
        public float BaseAtk => data.BaseAtk;
        public float BaseDef => data.BaseDef;
        public float BaseSpd => data.BaseSpd;
        public float BaseCritRate => data.BaseCritRate;
        public float BaseCritDmg => data.BaseCritDmg;
        public float HpPerLevel => data.HpPerLevel;
        public float AtkPerLevel => data.AtkPerLevel;
        public float DefPerLevel => data.DefPerLevel;
        public float SpdPerLevel => data.SpdPerLevel;
        public int BaseExpToLevel => data.BaseExpToLevel;
        public float ExpGrowthFactor => data.ExpGrowthFactor;
        public int MaxLevel => data.MaxLevel;

        public CharacterConfigWrapper(cfg.Adventure.CharacterInfo characterInfo)
        {
            data = characterInfo;
        }

        public CharacterConfigWrapper(string characterId)
        {
            data = ConfigManager.Instance.GetCharacterInfo(characterId);
        }

        /// <summary>
        /// 获取指定等级的属性值
        /// </summary>
        public float GetStatAtLevel(StatType stat, int level)
        {
            int growth = level - 1;
            switch (stat)
            {
                case StatType.MaxHp: return data.BaseHp + data.HpPerLevel * growth;
                case StatType.Atk: return data.BaseAtk + data.AtkPerLevel * growth;
                case StatType.Def: return data.BaseDef + data.DefPerLevel * growth;
                case StatType.Spd: return data.BaseSpd + data.SpdPerLevel * growth;
                case StatType.CritRate: return data.BaseCritRate;
                case StatType.CritDmg: return data.BaseCritDmg;
                default: return 0;
            }
        }

        /// <summary>
        /// 获取升到指定等级所需的累计经验
        /// </summary>
        public int GetExpForLevel(int level)
        {
            if (level <= 1) return 0;
            float total = 0;
            float current = data.BaseExpToLevel;
            for (int i = 2; i <= level; i++)
            {
                total += current;
                current *= data.ExpGrowthFactor;
            }
            return Mathf.RoundToInt(total);
        }

        /// <summary>
        /// 获取从当前等级升到下一级所需经验
        /// </summary>
        public int GetExpToNextLevel(int currentLevel)
        {
            if (currentLevel >= data.MaxLevel) return int.MaxValue;
            float exp = data.BaseExpToLevel * Mathf.Pow(data.ExpGrowthFactor, currentLevel - 1);
            return Mathf.RoundToInt(exp);
        }
    }
}
