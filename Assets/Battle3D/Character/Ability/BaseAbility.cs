using Battle3D.Core;
using UnityEngine;

namespace Battle3D.Character.Ability
{
    /// <summary>
    /// 能力基类 - 所有技能/动作的抽象基类
    /// 支持：优先级、标签阻塞、冷却时间、能量消耗、阶段管理、动画事件
    /// 参考现有项目 AbilitySystem 架构并增强
    /// </summary>
    public abstract class BaseAbility
    {
        protected BaseCharacter owner;
        protected AbilitySystem system;
        protected AbilityContext ctx;

        // 运行时状态
        public int Priority { get; protected set; }
        public AbilitySlot Slot => ctx?.slot ?? AbilitySlot.Passive;
        public AbilityTag OwnTag => ctx?.ownTag ?? AbilityTag.None;
        public AbilityTag BlockTags => ctx?.blockTags ?? AbilityTag.None;

        public bool IsEnabled { get; protected set; }
        public AbilityPhase CurrentPhase { get; protected set; }

        // 冷却
        protected float cooldownTimer;
        public float CooldownRemaining => Mathf.Max(0f, cooldownTimer);
        public float CooldownTotal => ctx?.cooldown ?? 0f;
        public bool IsOnCooldown => cooldownTimer > 0f;
        public float CooldownNormalized => CooldownTotal > 0f ? cooldownTimer / CooldownTotal : 0f;

        // 阶段计时
        protected float phaseTimer;

        // 能量引用（由 AbilitySystem 注入）
        public float EnergyCost => ctx?.energyCost ?? 0f;

        #region 初始化

        public virtual void Init(AbilityContext context)
        {
            ctx = context;
            CurrentPhase = AbilityPhase.Ready;
            IsEnabled = true;
        }

        public virtual void BindCharacter(BaseCharacter character)
        {
            owner = character;
        }

        public virtual void BindSystem(AbilitySystem abilitySystem)
        {
            system = abilitySystem;
        }

        #endregion

        #region 激活/释放

        /// <summary>
        /// 尝试激活能力（由输入系统或AI调用）
        /// </summary>
        public virtual bool TryActivate()
        {
            if (!IsEnabled) return false;
            if (CurrentPhase != AbilityPhase.Ready) return false;
            if (IsOnCooldown) return false;
            if (system.IsTagBlocked(OwnTag)) return false;
            if (!CheckEnergy()) return false;
            if (ctx.requiresGround && !owner.Motor.IsGrounded) return false;

            ConsumeEnergy();
            Activate();
            return true;
        }

        protected virtual void Activate()
        {
            // 阻塞自身标签和指定标签
            system.AddBlock(OwnTag);
            system.AddBlock(BlockTags);

            CurrentPhase = AbilityPhase.Windup;
            phaseTimer = ctx.windupTime;

            // 如果不能移动，禁用运动
            if (!ctx.canMoveWhileActive && owner.Motor != null)
            {
                owner.Motor.CanMove = false;
            }

            OnActivate();
            GameEventSystem.Publish(GameEvents.OnAbilityActivated, this);
        }

        /// <summary>
        /// 强制结束能力（被打断、取消等）
        /// </summary>
        public virtual void ForceEnd()
        {
            EndAbility();
        }

        protected virtual void EndAbility()
        {
            // 解除标签阻塞
            system.RemoveBlock(OwnTag);
            system.RemoveBlock(BlockTags);

            // 恢复移动
            if (!ctx.canMoveWhileActive && owner.Motor != null)
            {
                owner.Motor.CanMove = true;
            }

            // 进入冷却
            if (ctx.cooldown > 0f)
            {
                cooldownTimer = ctx.cooldown;
                CurrentPhase = AbilityPhase.Cooldown;
            }
            else
            {
                CurrentPhase = AbilityPhase.Ready;
            }

            OnDeactivate();
            GameEventSystem.Publish(GameEvents.OnAbilityDeactivated, this);
        }

        #endregion

        #region 更新循环

        public virtual void Tick(float dt)
        {
            if (!IsEnabled) return;

            // 冷却计时
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= dt;
                if (cooldownTimer <= 0f)
                {
                    cooldownTimer = 0f;
                    CurrentPhase = AbilityPhase.Ready;
                    GameEventSystem.Publish(GameEvents.OnAbilityCooldownEnd, this);
                }
            }

            // 阶段状态机
            switch (CurrentPhase)
            {
                case AbilityPhase.Windup:
                    UpdateWindup(dt);
                    break;
                case AbilityPhase.Active:
                    UpdateActive(dt);
                    break;
                case AbilityPhase.Recovery:
                    UpdateRecovery(dt);
                    break;
            }

            OnTickUpdate(dt);
        }

        private void UpdateWindup(float dt)
        {
            phaseTimer -= dt;
            if (phaseTimer <= 0f)
            {
                CurrentPhase = AbilityPhase.Active;
                phaseTimer = ctx.activeTime;
                OnActiveStart();
            }
        }

        private void UpdateActive(float dt)
        {
            phaseTimer -= dt;
            OnActiveUpdate(dt);
            if (phaseTimer <= 0f)
            {
                CurrentPhase = AbilityPhase.Recovery;
                phaseTimer = ctx.recoveryTime;
                OnActiveEnd();
            }
        }

        private void UpdateRecovery(float dt)
        {
            phaseTimer -= dt;
            if (phaseTimer <= 0f)
            {
                EndAbility();
            }
        }

        #endregion

        #region 虚方法（子类重写）

        protected virtual void OnActivate() { }
        protected virtual void OnDeactivate() { }
        protected virtual void OnTickUpdate(float dt) { }

        /// <summary>前摇结束，进入生效阶段</summary>
        protected virtual void OnActiveStart() { }

        /// <summary>生效阶段每帧更新</summary>
        protected virtual void OnActiveUpdate(float dt) { }

        /// <summary>生效阶段结束</summary>
        protected virtual void OnActiveEnd() { }

        /// <summary>动画事件回调（由 Animation Event 触发）</summary>
        public virtual void OnAnimationEvent(string eventName) { }

        /// <summary>被打断时调用</summary>
        public virtual void OnInterrupted()
        {
            ForceEnd();
        }

        #endregion

        #region 能量系统

        protected virtual bool CheckEnergy()
        {
            if (ctx.energyCost <= 0f) return true;
            return system.HasEnoughEnergy(ctx.energyCost);
        }

        protected virtual void ConsumeEnergy()
        {
            if (ctx.energyCost > 0f)
            {
                system.ConsumeEnergy(ctx.energyCost);
            }
        }

        #endregion

        #region 辅助方法

        public void SetEnabled(bool enabled)
        {
            if (IsEnabled == enabled) return;
            IsEnabled = enabled;

            if (!enabled && CurrentPhase != AbilityPhase.Ready && CurrentPhase != AbilityPhase.Cooldown)
            {
                ForceEnd();
            }
        }

        public void ResetCooldown()
        {
            cooldownTimer = 0f;
            if (CurrentPhase == AbilityPhase.Cooldown)
                CurrentPhase = AbilityPhase.Ready;
        }

        /// <summary>获取面向方向（用于技能释放方向）</summary>
        protected Vector3 GetForwardDirection()
        {
            return owner ? owner.transform.forward : Vector3.forward;
        }

        /// <summary>获取面向位置（用于朝向目标）</summary>
        protected Vector3 GetTargetDirection(Vector3 targetPos)
        {
            if (owner == null) return Vector3.forward;
            Vector3 dir = targetPos - owner.transform.position;
            dir.y = 0f;
            return dir.normalized;
        }

        #endregion
    }
}
