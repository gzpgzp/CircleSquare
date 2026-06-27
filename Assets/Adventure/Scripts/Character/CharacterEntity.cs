using System;
using UnityEngine;

namespace Adventure.Character
{
    /// <summary>
    /// 角色运行时实体 - 组合了属性、等级、战斗状态
    /// 挂机游戏中每个角色（玩家/敌人）都是一个 CharacterEntity
    /// </summary>
    public class CharacterEntity
    {
        public string characterId;
        public string characterName;
        public CharacterCamp camp;
        public CombatState combatState = CombatState.Idle;

        public CharacterStats stats { get; private set; }
        public CharacterLevel levelSystem { get; private set; }
        public CharacterConfigWrapper configWrapper { get; private set; }

        /// <summary>当前血量</summary>
        public float currentHp { get; private set; }
        public bool isDead => currentHp <= 0;
        public float hpPercent => stats.GetValue(StatType.MaxHp) > 0
            ? currentHp / stats.GetValue(StatType.MaxHp)
            : 0f;

        /// <summary>攻击冷却计时器</summary>
        private float attackTimer;

        public CharacterEntity(cfg.Adventure.CharacterInfo charInfo, CharacterCamp camp, int level = 1)
        {
            this.configWrapper = new CharacterConfigWrapper(charInfo);
            this.characterId = charInfo.CharacterId;
            this.characterName = charInfo.Name;
            this.camp = camp;

            stats = new CharacterStats();
            levelSystem = new CharacterLevel();
            levelSystem.Init(configWrapper, stats, characterId);

            // 如果不是1级，加载到目标等级
            if (level > 1)
            {
                levelSystem.LoadFromSave(level, 0);
            }

            // 满血
            currentHp = stats.GetValue(StatType.MaxHp);

            // 属性变化时检查血量上限
            stats.onStatsChanged += OnStatsChanged;
        }

        /// <summary>
        /// 受到伤害
        /// </summary>
        public void TakeDamage(float rawDamage, bool isCrit = false)
        {
            if (isDead) return;

            // 伤害公式: 实际伤害 = 原始伤害 - 防御 * 0.5（至少1点）
            float def = stats.GetValue(StatType.Def);
            float finalDamage = Mathf.Max(rawDamage - def * 0.5f, 1f);

            currentHp = Mathf.Max(currentHp - finalDamage, 0f);
            CharacterDamagedEvent.Trigger(characterId, finalDamage, currentHp, isCrit);

            if (isDead)
            {
                combatState = CombatState.Dead;
                CharacterDeadEvent.Trigger(characterId, camp);
            }
        }

        /// <summary>
        /// 恢复血量
        /// </summary>
        public void Heal(float amount)
        {
            if (isDead) return;
            float maxHp = stats.GetValue(StatType.MaxHp);
            currentHp = Mathf.Min(currentHp + amount, maxHp);
        }

        /// <summary>
        /// 满血复活
        /// </summary>
        public void Revive()
        {
            currentHp = stats.GetValue(StatType.MaxHp);
            combatState = CombatState.Idle;
        }

        /// <summary>
        /// 计算对目标的一次攻击伤害
        /// </summary>
        public float CalculateAttackDamage(out bool isCrit)
        {
            float atk = stats.GetValue(StatType.Atk);
            float critRate = stats.GetValue(StatType.CritRate);
            float critDmg = stats.GetValue(StatType.CritDmg);

            isCrit = UnityEngine.Random.value < critRate;
            float damage = isCrit ? atk * critDmg : atk;
            return damage;
        }

        /// <summary>
        /// 每帧更新攻击冷却，返回是否可以攻击
        /// </summary>
        public bool UpdateAttackTimer(float deltaTime)
        {
            if (isDead) return false;

            float spd = stats.GetValue(StatType.Spd);
            if (spd <= 0) return false;

            float attackInterval = 1f / spd;
            attackTimer += deltaTime;

            if (attackTimer >= attackInterval)
            {
                attackTimer -= attackInterval;
                return true;
            }
            return false;
        }

        /// <summary>
        /// 重置攻击计时器
        /// </summary>
        public void ResetAttackTimer()
        {
            attackTimer = 0f;
        }

        private void OnStatsChanged()
        {
            float maxHp = stats.GetValue(StatType.MaxHp);
            if (currentHp > maxHp)
            {
                currentHp = maxHp;
            }
        }
    }
}
