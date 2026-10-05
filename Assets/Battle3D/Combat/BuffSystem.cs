using System;
using System.Collections.Generic;
using Battle3D.Character;
using UnityEngine;

namespace Battle3D.Combat
{
    /// <summary>
    /// Buff类型枚举
    /// </summary>
    public enum BuffType
    {
        AttackUp,       // 攻击力提升
        DefenseUp,      // 防御力提升
        SpeedUp,        // 移速提升
        DamageBoost,    // 伤害加成
        DamageReduction,// 伤害减免
        RegenHealth,    // 持续回血
        Stun,           // 眩晕
        Slow,           // 减速
    }

    /// <summary>
    /// Buff数据
    /// </summary>
    [Serializable]
    public class BuffData
    {
        public string buffId;
        public string buffName;
        public BuffType type;
        public float value;           // 数值（绝对值或百分比）
        public float duration;        // 持续时间（0=永久）
        public float tickInterval;    // 效果触发间隔（0=一次性）
        public bool isPercent;        // 是否为百分比加成
        public int maxStacks = 1;     // 最大叠加层数
    }

    /// <summary>
    /// Buff实例 - 运行时的Buff状态
    /// </summary>
    public class BuffInstance
    {
        public BuffData data;
        public BaseCharacter source;    // 施加者
        public BaseCharacter target;    // 目标
        public float remainingTime;
        public float tickTimer;
        public int stacks;

        public bool IsExpired => data.duration > 0f && remainingTime <= 0f;
    }

    /// <summary>
    /// Buff组件 - 管理角色身上的所有Buff
    /// 处理Buff的添加、移除、叠加和效果计算
    /// </summary>
    public class BuffComponent
    {
        private BaseCharacter owner;
        private List<BuffInstance> activeBuffs = new List<BuffInstance>();

        public BuffComponent(BaseCharacter character)
        {
            owner = character;
        }

        /// <summary>
        /// 添加Buff
        /// </summary>
        public void AddBuff(BuffData data, BaseCharacter source = null)
        {
            // 检查是否已存在同ID的Buff
            var existing = activeBuffs.Find(b => b.data.buffId == data.buffId);
            if (existing != null)
            {
                // 叠加
                if (existing.stacks < data.maxStacks)
                {
                    existing.stacks++;
                }
                // 刷新持续时间
                existing.remainingTime = data.duration;
                return;
            }

            // 新Buff
            var instance = new BuffInstance
            {
                data = data,
                source = source,
                target = owner,
                remainingTime = data.duration,
                tickTimer = data.tickInterval,
                stacks = 1,
            };
            activeBuffs.Add(instance);

            OnBuffAdded(instance);
        }

        /// <summary>
        /// 移除Buff
        /// </summary>
        public void RemoveBuff(string buffId)
        {
            var buff = activeBuffs.Find(b => b.data.buffId == buffId);
            if (buff != null)
            {
                OnBuffRemoved(buff);
                activeBuffs.Remove(buff);
            }
        }

        /// <summary>
        /// 移除所有Buff
        /// </summary>
        public void RemoveAllBuffs()
        {
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                OnBuffRemoved(activeBuffs[i]);
            }
            activeBuffs.Clear();
        }

        /// <summary>
        /// 移除指定类型的所有Buff
        /// </summary>
        public void RemoveBuffsByType(BuffType type)
        {
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                if (activeBuffs[i].data.type == type)
                {
                    OnBuffRemoved(activeBuffs[i]);
                    activeBuffs.RemoveAt(i);
                }
            }
        }

        /// <summary>
        /// 每帧更新
        /// </summary>
        public void Tick(float dt)
        {
            for (int i = activeBuffs.Count - 1; i >= 0; i--)
            {
                var buff = activeBuffs[i];

                // 持续时间更新
                if (buff.data.duration > 0f)
                {
                    buff.remainingTime -= dt;
                    if (buff.IsExpired)
                    {
                        OnBuffRemoved(buff);
                        activeBuffs.RemoveAt(i);
                        continue;
                    }
                }

                // Tick效果（如持续回血）
                if (buff.data.tickInterval > 0f)
                {
                    buff.tickTimer -= dt;
                    if (buff.tickTimer <= 0f)
                    {
                        buff.tickTimer = buff.data.tickInterval;
                        ApplyTickEffect(buff);
                    }
                }
            }
        }

        #region 效果查询（供伤害计算使用）

        /// <summary>获取伤害加成倍率</summary>
        public float GetDamageMultiplier()
        {
            float multiplier = 1f;
            foreach (var buff in activeBuffs)
            {
                if (buff.data.type == BuffType.DamageBoost)
                {
                    if (buff.data.isPercent)
                        multiplier += buff.data.value * buff.stacks;
                    else
                        multiplier += buff.data.value * buff.stacks / 100f;
                }
            }
            return multiplier;
        }

        /// <summary>获取伤害减免倍率</summary>
        public float GetDamageReduction()
        {
            float reduction = 0f;
            foreach (var buff in activeBuffs)
            {
                if (buff.data.type == BuffType.DamageReduction)
                {
                    if (buff.data.isPercent)
                        reduction += buff.data.value * buff.stacks;
                    else
                        reduction += buff.data.value * buff.stacks / 100f;
                }
            }
            return Mathf.Max(0f, 1f - reduction);
        }

        /// <summary>获取移速加成</summary>
        public float GetSpeedMultiplier()
        {
            float multiplier = 1f;
            foreach (var buff in activeBuffs)
            {
                if (buff.data.type == BuffType.SpeedUp)
                {
                    multiplier += buff.data.isPercent
                        ? buff.data.value * buff.stacks
                        : buff.data.value * buff.stacks / 100f;
                }
                else if (buff.data.type == BuffType.Slow)
                {
                    multiplier -= buff.data.isPercent
                        ? buff.data.value * buff.stacks
                        : buff.data.value * buff.stacks / 100f;
                }
            }
            return Mathf.Max(0.1f, multiplier); // 最低10%移速
        }

        /// <summary>是否被眩晕</summary>
        public bool IsStunned()
        {
            foreach (var buff in activeBuffs)
            {
                if (buff.data.type == BuffType.Stun)
                    return true;
            }
            return false;
        }

        /// <summary>获取当前所有Buff</summary>
        public IReadOnlyList<BuffInstance> GetActiveBuffs()
        {
            return activeBuffs;
        }

        #endregion

        #region 回调

        private void ApplyTickEffect(BuffInstance buff)
        {
            switch (buff.data.type)
            {
                case BuffType.RegenHealth:
                    owner.Heal(buff.data.value * buff.stacks);
                    break;
            }
        }

        protected virtual void OnBuffAdded(BuffInstance buff)
        {
            Debug.Log($"[Buff] {owner.CharacterId} gained {buff.data.buffName} x{buff.stacks}");
        }

        protected virtual void OnBuffRemoved(BuffInstance buff)
        {
            Debug.Log($"[Buff] {owner.CharacterId} lost {buff.data.buffName}");
        }

        #endregion
    }
}
