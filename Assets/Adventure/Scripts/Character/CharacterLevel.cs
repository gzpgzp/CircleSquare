using System;
using UnityEngine;

namespace Adventure.Character
{
    /// <summary>
    /// 角色等级系统 - 管理经验获取和升级
    /// </summary>
    [Serializable]
    public class CharacterLevel
    {
        public int level = 1;
        public int currentExp;

        private CharacterConfigWrapper config;
        private CharacterStats stats;
        private string characterId;

        public int ExpToNextLevel => config.GetExpToNextLevel(level);
        public bool IsMaxLevel => level >= config.MaxLevel;
        public float ExpProgress => IsMaxLevel ? 1f : (float)currentExp / ExpToNextLevel;

        public void Init(CharacterConfigWrapper config, CharacterStats stats, string characterId)
        {
            this.config = config;
            this.stats = stats;
            this.characterId = characterId;
            ApplyLevelStats();
        }

        /// <summary>
        /// 增加经验，自动处理升级
        /// </summary>
        public void AddExp(int amount)
        {
            if (IsMaxLevel) return;

            currentExp += amount;
            GainExpEvent.Trigger(characterId, amount);

            // 循环升级
            while (!IsMaxLevel && currentExp >= ExpToNextLevel)
            {
                currentExp -= ExpToNextLevel;
                LevelUp();
            }

            if (IsMaxLevel)
            {
                currentExp = 0;
            }
        }

        private void LevelUp()
        {
            level++;
            ApplyLevelStats();
            CharacterLevelUpEvent.Trigger(characterId, level);
            Debug.Log($"[CharacterLevel] {characterId} leveled up to {level}!");
        }

        /// <summary>
        /// 将当前等级的属性应用到Stats
        /// </summary>
        private void ApplyLevelStats()
        {
            stats.SetBase(StatType.MaxHp, config.GetStatAtLevel(StatType.MaxHp, level));
            stats.SetBase(StatType.Atk, config.GetStatAtLevel(StatType.Atk, level));
            stats.SetBase(StatType.Def, config.GetStatAtLevel(StatType.Def, level));
            stats.SetBase(StatType.Spd, config.GetStatAtLevel(StatType.Spd, level));
            stats.SetBase(StatType.CritRate, config.GetStatAtLevel(StatType.CritRate, level));
            stats.SetBase(StatType.CritDmg, config.GetStatAtLevel(StatType.CritDmg, level));
        }

        /// <summary>
        /// 从存档恢复
        /// </summary>
        public void LoadFromSave(int savedLevel, int savedExp)
        {
            level = savedLevel;
            currentExp = savedExp;
            ApplyLevelStats();
        }
    }
}
