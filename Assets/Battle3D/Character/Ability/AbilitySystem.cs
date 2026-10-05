using System.Collections.Generic;
using UnityEngine;
using Battle3D.Core;

namespace Battle3D.Character.Ability
{
    /// <summary>
    /// 能力系统 - 管理角色的所有能力
    /// 维护能力列表、标签阻塞机制、能量系统
    /// 参考现有项目 AbilitySystem 并增强能量/连招系统
    /// </summary>
    public class AbilitySystem
    {
        private List<BaseAbility> abilities = new List<BaseAbility>();
        private Dictionary<AbilitySlot, BaseAbility> slotMap = new Dictionary<AbilitySlot, BaseAbility>();
        private AbilityTag blockedTags = AbilityTag.None;

        // 能量系统
        private float currentEnergy;
        private float maxEnergy = 100f;
        private float energyRegenRate = 5f; // 每秒自然恢复

        public float CurrentEnergy => currentEnergy;
        public float MaxEnergy => maxEnergy;
        public float EnergyNormalized => maxEnergy > 0f ? currentEnergy / maxEnergy : 0f;

        private BaseCharacter owner;

        public void Init(BaseCharacter character, float maxEnergy = 100f)
        {
            owner = character;
            this.maxEnergy = maxEnergy;
            currentEnergy = maxEnergy;
        }

        #region 能力管理

        public void AddAbility(BaseAbility ability)
        {
            ability.BindSystem(this);
            ability.BindCharacter(owner);
            abilities.Add(ability);

            // 按槽位注册
            if (!slotMap.ContainsKey(ability.Slot))
            {
                slotMap[ability.Slot] = ability;
            }

            // 按优先级排序（高优先级先处理）
            abilities.Sort((a, b) => b.Priority.CompareTo(a.Priority));
        }

        public void RemoveAbility(BaseAbility ability)
        {
            abilities.Remove(ability);
            if (slotMap.TryGetValue(ability.Slot, out var slotAbility) && slotAbility == ability)
            {
                slotMap.Remove(ability.Slot);
            }
        }

        public BaseAbility GetAbility(AbilitySlot slot)
        {
            slotMap.TryGetValue(slot, out var ability);
            return ability;
        }

        public T GetAbility<T>() where T : BaseAbility
        {
            foreach (var ability in abilities)
            {
                if (ability is T typed)
                    return typed;
            }
            return null;
        }

        public IReadOnlyList<BaseAbility> GetAllAbilities()
        {
            return abilities;
        }

        #endregion

        #region 标签阻塞

        public bool IsTagBlocked(AbilityTag tag)
        {
            if (tag == AbilityTag.None) return false;
            return (blockedTags & tag) != 0;
        }

        public void AddBlock(AbilityTag tag)
        {
            blockedTags |= tag;
        }

        public void RemoveBlock(AbilityTag tag)
        {
            blockedTags &= ~tag;
        }

        public void ClearBlocks()
        {
            blockedTags = AbilityTag.None;
        }

        #endregion

        #region 能量系统

        public bool HasEnoughEnergy(float cost)
        {
            return currentEnergy >= cost;
        }

        public void ConsumeEnergy(float amount)
        {
            currentEnergy = Mathf.Max(0f, currentEnergy - amount);
            NotifyEnergyChanged();
        }

        public void AddEnergy(float amount)
        {
            currentEnergy = Mathf.Min(maxEnergy, currentEnergy + amount);
            NotifyEnergyChanged();
        }

        public void SetEnergy(float amount)
        {
            currentEnergy = Mathf.Clamp(amount, 0f, maxEnergy);
            NotifyEnergyChanged();
        }

        public void SetMaxEnergy(float max)
        {
            maxEnergy = max;
            currentEnergy = Mathf.Min(currentEnergy, maxEnergy);
            NotifyEnergyChanged();
        }

        private void NotifyEnergyChanged()
        {
            GameEventSystem.Publish(GameEvents.OnEnergyChanged, new EnergyChangedData
            {
                character = owner,
                current = currentEnergy,
                max = maxEnergy,
            });
        }

        #endregion

        #region 更新循环

        public void Tick(float dt)
        {
            // 自然恢复能量
            if (currentEnergy < maxEnergy)
            {
                AddEnergy(energyRegenRate * dt);
            }

            // 更新所有能力
            for (int i = 0; i < abilities.Count; i++)
            {
                abilities[i].Tick(dt);
            }
        }

        #endregion

        #region 辅助方法

        /// <summary>
        /// 激活指定槽位的能力
        /// </summary>
        public bool ActivateSlot(AbilitySlot slot)
        {
            if (slotMap.TryGetValue(slot, out var ability))
            {
                return ability.TryActivate();
            }
            return false;
        }

        /// <summary>
        /// 强制结束所有能力
        /// </summary>
        public void ForceEndAll()
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                abilities[i].ForceEnd();
            }
            ClearBlocks();
        }

        /// <summary>
        /// 打断所有非无敌能力
        /// </summary>
        public void InterruptAll()
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                var ability = abilities[i];
                if ((ability.OwnTag & AbilityTag.Invincible) == 0)
                {
                    ability.OnInterrupted();
                }
            }
        }

        /// <summary>
        /// 触发所有能力的动画事件
        /// </summary>
        public void TriggerAnimationEvent(string eventName)
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                abilities[i].OnAnimationEvent(eventName);
            }
        }

        /// <summary>
        /// 重置所有冷却
        /// </summary>
        public void ResetAllCooldowns()
        {
            for (int i = 0; i < abilities.Count; i++)
            {
                abilities[i].ResetCooldown();
            }
        }

        #endregion
    }

    /// <summary>
    /// 能量变化事件数据
    /// </summary>
    public struct EnergyChangedData
    {
        public BaseCharacter character;
        public float current;
        public float max;
    }
}
