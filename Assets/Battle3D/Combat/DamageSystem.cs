using Battle3D.Character;
using Battle3D.Core;
using UnityEngine;

namespace Battle3D.Combat
{
    /// <summary>
    /// 伤害信息结构 - 传递伤害计算所需的所有数据
    /// </summary>
    public struct DamageInfo
    {
        public BaseCharacter attacker;
        public float baseDamage;
        public float damageMultiplier;
        public float knockbackForce;
        public Vector3 hitPoint;
        public bool isCritical;
        public string damageType; // 物理、元素等（预留扩展）
    }

    /// <summary>
    /// 受击数据 - 事件传递用
    /// </summary>
    public struct HitData
    {
        public BaseCharacter target;
        public BaseCharacter attacker;
        public float damage;
        public Vector3 hitPoint;
    }

    /// <summary>
    /// 死亡数据 - 事件传递用
    /// </summary>
    public struct DeathData
    {
        public BaseCharacter character;
        public BaseCharacter killer;
    }

    /// <summary>
    /// 伤害系统 - 统一处理伤害计算
    /// 支持：攻击力、防御力、暴击、伤害减免
    /// 后续可扩展元素反应、伤害类型等
    /// </summary>
    public static class DamageSystem
    {
        // 暴击配置
        private const float BaseCritRate = 0.05f;
        private const float BaseCritDamage = 1.5f;

        /// <summary>
        /// 计算最终伤害
        /// </summary>
        public static float CalculateDamage(DamageInfo info, BaseCharacter target)
        {
            float damage = info.baseDamage * info.damageMultiplier;

            // 防御力减伤（简单公式：damage * (100 / (100 + defense))）
            float defense = target.Defense;
            float defenseMultiplier = 100f / (100f + defense);
            damage *= defenseMultiplier;

            // 暴击判定
            if (!info.isCritical)
            {
                info.isCritical = Random.value < BaseCritRate;
            }
            if (info.isCritical)
            {
                damage *= BaseCritDamage;
            }

            // Buff加成（预留）
            if (info.attacker?.Buffs != null)
            {
                damage *= info.attacker.Buffs.GetDamageMultiplier();
            }

            // 目标减伤Buff
            if (target?.Buffs != null)
            {
                damage *= target.Buffs.GetDamageReduction();
            }

            // 最小伤害保底
            damage = Mathf.Max(1f, damage);

            // 发布伤害事件（用于UI显示等）
            GameEventSystem.Publish(GameEvents.OnDamageDealt, new DamageResult
            {
                attacker = info.attacker,
                target = target,
                rawDamage = info.baseDamage * info.damageMultiplier,
                finalDamage = damage,
                isCritical = info.isCritical,
                hitPoint = info.hitPoint,
            });

            return damage;
        }
    }

    /// <summary>
    /// 伤害结果 - 用于UI显示和日志
    /// </summary>
    public struct DamageResult
    {
        public BaseCharacter attacker;
        public BaseCharacter target;
        public float rawDamage;
        public float finalDamage;
        public bool isCritical;
        public Vector3 hitPoint;
    }
}
