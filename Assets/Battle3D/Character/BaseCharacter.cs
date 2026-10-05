using Battle3D.Character.Ability;
using Battle3D.Character.Brain;
using Battle3D.Character.Motor;
using Battle3D.Combat;
using Battle3D.Core;
using UnityEngine;

namespace Battle3D.Character
{
    /// <summary>
    /// 角色基类 - 所有角色的抽象基类
    /// 组合模式：AbilitySystem + CharacterBrain + ICharacterMotor
    /// 参考现有项目 BaseCharacter.cs 并增强为3D版本
    /// </summary>
    public abstract class BaseCharacter : MonoBehaviour
    {
        // 核心系统
        protected CharacterBrain brain;
        protected AbilitySystem abilitySystem;
        protected ICharacterMotor motor;
        protected CharacterContext ctx;

        // 战斗属性
        [SerializeField] protected float maxHealth = 100f;
        [SerializeField] protected float currentHealth;
        [SerializeField] protected float moveSpeed = 6f;
        [SerializeField] protected float attackPower = 10f;
        [SerializeField] protected float defense = 5f;

        // Buff系统
        protected BuffComponent buffComponent;

        // 状态
        public bool IsAlive => currentHealth > 0f;
        public bool IsPlayer => ctx?.isPlayer ?? false;
        public string CharacterId => ctx?.characterId ?? "";

        // 属性访问器
        public float MaxHealth => maxHealth;
        public float CurrentHealth => currentHealth;
        public float HealthNormalized => maxHealth > 0f ? currentHealth / maxHealth : 0f;
        public float MoveSpeed => moveSpeed;
        public float AttackPower => attackPower;
        public float Defense => defense;

        public AbilitySystem AbilitySystemComponent => abilitySystem;
        public ICharacterMotor Motor => motor;
        public BuffComponent Buffs => buffComponent;

        #region 初始化

        public virtual void Init(CharacterContext context)
        {
            ctx = context;

            // 设置属性
            maxHealth = context.maxHealth;
            currentHealth = maxHealth;
            moveSpeed = context.moveSpeed;

            // 初始化能力系统
            abilitySystem = new AbilitySystem();
            abilitySystem.Init(this, context.maxEnergy);

            // 初始化Buff组件
            buffComponent = new BuffComponent(this);

            // 初始化运动系统（由子类实现）
            InitMotor();

            // 初始化能力
            InitAbilities(context.abilityIds);

            Debug.Log($"[BaseCharacter] {context.characterName} initialized.");
        }

        /// <summary>初始化运动系统（子类实现）</summary>
        protected abstract void InitMotor();

        /// <summary>初始化能力列表</summary>
        protected virtual void InitAbilities(System.Collections.Generic.List<string> abilityIds)
        {
            foreach (var id in abilityIds)
            {
                var ability = AbilityInitializer.CreateAbility(id);
                if (ability != null)
                {
                    AddAbility(ability);
                }
            }
        }

        #endregion

        #region 能力管理

        public void AddAbility(BaseAbility ability)
        {
            ability.BindCharacter(this);
            abilitySystem.AddAbility(ability);
        }

        public void SetBrain(CharacterBrain newBrain)
        {
            brain = newBrain;
        }

        #endregion

        #region 更新循环

        /// <summary>Update中调用 - 驱动脑系统</summary>
        public virtual void UpdateTick(float dt)
        {
            if (!IsAlive) return;

            brain?.Tick(dt);
            buffComponent?.Tick(dt);
        }

        /// <summary>FixedUpdate中调用 - 驱动能力系统和运动系统</summary>
        public virtual void Tick(float dt)
        {
            if (!IsAlive) return;

            abilitySystem?.Tick(dt);
            motor?.FixedTick(dt);
        }

        #endregion

        #region 战斗相关

        /// <summary>受到伤害</summary>
        public virtual void TakeDamage(DamageInfo damageInfo)
        {
            if (!IsAlive) return;

            // 检查无敌状态
            if (abilitySystem != null && abilitySystem.IsTagBlocked(AbilityTag.Invincible))
            {
                return; // 无敌帧中，忽略伤害
            }

            // 伤害计算
            float actualDamage = DamageSystem.CalculateDamage(damageInfo, this);
            currentHealth = Mathf.Max(0f, currentHealth - actualDamage);

            // 发布受击事件
            GameEventSystem.Publish(GameEvents.OnCharacterHit, new HitData
            {
                target = this,
                attacker = damageInfo.attacker,
                damage = actualDamage,
                hitPoint = damageInfo.hitPoint,
            });

            // 受击反馈
            OnHit(damageInfo, actualDamage);

            // 死亡检查
            if (currentHealth <= 0f)
            {
                OnDeath(damageInfo.attacker);
            }
        }

        /// <summary>受击反馈（子类可重写）</summary>
        protected virtual void OnHit(DamageInfo damageInfo, float actualDamage)
        {
            // 打断非无敌能力
            abilitySystem?.InterruptAll();

            // 施加击退
            if (damageInfo.knockbackForce > 0f && motor != null)
            {
                Vector3 knockDir = (transform.position - damageInfo.attacker.transform.position).normalized;
                knockDir.y = 0.2f;
                motor.ApplyKnockback(knockDir * damageInfo.knockbackForce);
            }
        }

        /// <summary>死亡处理</summary>
        protected virtual void OnDeath(BaseCharacter killer)
        {
            currentHealth = 0f;

            // 强制结束所有能力
            abilitySystem?.ForceEndAll();

            // 禁用运动
            if (motor != null)
                motor.CanMove = false;

            GameEventSystem.Publish(GameEvents.OnCharacterDeath, new DeathData
            {
                character = this,
                killer = killer,
            });

            Debug.Log($"[BaseCharacter] {ctx.characterName} has been killed by {killer?.ctx?.characterName}");
        }

        /// <summary>治疗</summary>
        public virtual void Heal(float amount)
        {
            if (!IsAlive) return;
            currentHealth = Mathf.Min(maxHealth, currentHealth + amount);
        }

        #endregion

        #region 辅助方法

        public void SetPosition(Vector3 position)
        {
            transform.position = position;
        }

        public void SetRotation(Quaternion rotation)
        {
            transform.rotation = rotation;
        }

        /// <summary>触发能力的动画事件</summary>
        public void TriggerAbilityAnimationEvent(string eventName)
        {
            abilitySystem?.TriggerAnimationEvent(eventName);
        }

        #endregion
    }
}
