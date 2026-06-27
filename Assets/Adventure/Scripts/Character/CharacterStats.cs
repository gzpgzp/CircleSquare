using System;
using System.Collections.Generic;

namespace Adventure.Character
{
    /// <summary>
    /// 角色属性系统 - 支持基础值 + 多层加成（装备/Buff/等级）
    /// 最终值 = (基础值 + 固定加成) * (1 + 百分比加成)
    /// </summary>
    [Serializable]
    public class CharacterStats
    {
        private Dictionary<StatType, float> baseValues = new Dictionary<StatType, float>();
        private Dictionary<StatType, float> flatBonus = new Dictionary<StatType, float>();
        private Dictionary<StatType, float> percentBonus = new Dictionary<StatType, float>();

        /// <summary>属性变化回调</summary>
        public Action onStatsChanged;

        public CharacterStats()
        {
            // 初始化所有属性为0
            foreach (StatType stat in Enum.GetValues(typeof(StatType)))
            {
                baseValues[stat] = 0f;
                flatBonus[stat] = 0f;
                percentBonus[stat] = 0f;
            }
        }

        /// <summary>
        /// 设置基础属性值（来自配置/等级成长）
        /// </summary>
        public void SetBase(StatType stat, float value)
        {
            baseValues[stat] = value;
            onStatsChanged?.Invoke();
        }

        /// <summary>
        /// 添加固定加成（装备、Buff固定值）
        /// </summary>
        public void AddFlatBonus(StatType stat, float value)
        {
            flatBonus[stat] += value;
            onStatsChanged?.Invoke();
        }

        /// <summary>
        /// 移除固定加成
        /// </summary>
        public void RemoveFlatBonus(StatType stat, float value)
        {
            flatBonus[stat] -= value;
            onStatsChanged?.Invoke();
        }

        /// <summary>
        /// 添加百分比加成（如 0.2 = +20%）
        /// </summary>
        public void AddPercentBonus(StatType stat, float value)
        {
            percentBonus[stat] += value;
            onStatsChanged?.Invoke();
        }

        /// <summary>
        /// 移除百分比加成
        /// </summary>
        public void RemovePercentBonus(StatType stat, float value)
        {
            percentBonus[stat] -= value;
            onStatsChanged?.Invoke();
        }

        /// <summary>
        /// 获取最终属性值
        /// </summary>
        public float GetValue(StatType stat)
        {
            float baseVal = baseValues.ContainsKey(stat) ? baseValues[stat] : 0f;
            float flat = flatBonus.ContainsKey(stat) ? flatBonus[stat] : 0f;
            float percent = percentBonus.ContainsKey(stat) ? percentBonus[stat] : 0f;
            return (baseVal + flat) * (1f + percent);
        }

        /// <summary>
        /// 获取基础值
        /// </summary>
        public float GetBase(StatType stat)
        {
            return baseValues.ContainsKey(stat) ? baseValues[stat] : 0f;
        }

        /// <summary>
        /// 从配置初始化所有基础属性
        /// </summary>
        public void InitFromConfig(float hp, float atk, float def, float spd, float critRate, float critDmg)
        {
            baseValues[StatType.MaxHp] = hp;
            baseValues[StatType.Atk] = atk;
            baseValues[StatType.Def] = def;
            baseValues[StatType.Spd] = spd;
            baseValues[StatType.CritRate] = critRate;
            baseValues[StatType.CritDmg] = critDmg;
            onStatsChanged?.Invoke();
        }
    }
}
