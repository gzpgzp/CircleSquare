using Battle3D.Combat;
using UnityEngine;

namespace Battle3D.Character.Ability.Abilities
{
    /// <summary>
    /// 技能基类 - 支持蓄力、多段释放
    /// 具体技能继承此类，通过 AbilityId 属性注册
    /// 示例：SkillAbility_Skill1, SkillAbility_Skill2
    /// </summary>
    public abstract class SkillAbility : BaseAbility
    {
        [Header("技能通用配置")]
        protected float skillRange = 5f;
        protected float hitRadius = 2f;
        protected bool canCharge = false;
        protected float maxChargeTime = 2f;

        // 蓄力状态
        protected bool isCharging;
        protected float chargeTimer;
        protected float chargeLevel; // 0-1 蓄力程度

        public bool IsCharging => isCharging;
        public float ChargeLevel => chargeLevel;

        #region 蓄力系统

        /// <summary>
        /// 开始蓄力（输入按住时调用）
        /// </summary>
        public virtual void StartCharge()
        {
            if (!canCharge) return;
            if (CurrentPhase != AbilityPhase.Ready) return;

            isCharging = true;
            chargeTimer = 0f;
            chargeLevel = 0f;

            OnChargeStart();
        }

        /// <summary>
        /// 蓄力更新
        /// </summary>
        protected virtual void UpdateCharge(float dt)
        {
            if (!isCharging) return;

            chargeTimer += dt;
            chargeLevel = Mathf.Clamp01(chargeTimer / maxChargeTime);

            OnChargeUpdate(dt, chargeLevel);

            // 满蓄自动释放
            if (chargeLevel >= 1f)
            {
                ReleaseCharge();
            }
        }

        /// <summary>
        /// 释放蓄力（输入松开时调用）
        /// </summary>
        public virtual void ReleaseCharge()
        {
            if (!isCharging) return;

            isCharging = false;
            OnChargeEnd(chargeLevel);

            // 触发技能
            TryActivate();
        }

        /// <summary>取消蓄力</summary>
        public virtual void CancelCharge()
        {
            if (!isCharging) return;
            isCharging = false;
            chargeLevel = 0f;
            OnChargeCancelled();
        }

        protected virtual void OnChargeStart() { }
        protected virtual void OnChargeUpdate(float dt, float level) { }
        protected virtual void OnChargeEnd(float finalLevel) { }
        protected virtual void OnChargeCancelled() { }

        #endregion

        #region 伤害处理

        /// <summary>
        /// 执行范围伤害检测
        /// </summary>
        protected virtual void PerformAreaAttack(Vector3 center, float radius, float multiplier)
        {
            Collider[] hits = Physics.OverlapSphere(center, radius);

            foreach (var hit in hits)
            {
                var targetChar = hit.GetComponent<BaseCharacter>();
                if (targetChar != null && targetChar != owner)
                {
                    DealDamage(targetChar, multiplier);
                }
            }
        }

        /// <summary>
        /// 执行射线伤害检测
        /// </summary>
        protected virtual void PerformRayAttack(Vector3 origin, Vector3 direction, float maxDistance, float multiplier)
        {
            if (Physics.Raycast(origin, direction, out RaycastHit hit, maxDistance))
            {
                var targetChar = hit.collider.GetComponent<BaseCharacter>();
                if (targetChar != null && targetChar != owner)
                {
                    DealDamage(targetChar, multiplier);
                }
            }
        }

        /// <summary>
        /// 对目标造成伤害
        /// </summary>
        protected virtual void DealDamage(BaseCharacter target, float multiplier)
        {
            var damageInfo = new DamageInfo
            {
                attacker = owner,
                baseDamage = owner.AttackPower,
                damageMultiplier = multiplier * (ctx?.damageMultiplier ?? 1f),
                knockbackForce = ctx?.knockbackForce ?? 3f,
                hitPoint = target.transform.position,
                isCritical = false,
            };

            target.TakeDamage(damageInfo);
        }

        #endregion

        #region 覆写

        protected override void OnTickUpdate(float dt)
        {
            base.OnTickUpdate(dt);

            if (isCharging)
            {
                UpdateCharge(dt);
            }
        }

        public override void OnInterrupted()
        {
            if (isCharging)
            {
                CancelCharge();
            }
            base.OnInterrupted();
        }

        public override void ForceEnd()
        {
            if (isCharging)
            {
                CancelCharge();
            }
            base.ForceEnd();
        }

        #endregion
    }

    /// <summary>
    /// 示例技能1 - 前方扇形斩击
    /// </summary>
    [AbilityId("skill_slash")]
    public class SlashSkill : SkillAbility
    {
        public override void Init(AbilityContext context)
        {
            base.Init(context);
            Priority = 15;
            skillRange = 4f;
            hitRadius = 2.5f;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            // 技能动画后续再加，当前用攻击动画替代
        }

        protected override void OnActiveStart()
        {
            base.OnActiveStart();

            // 在生效帧执行前方扇形攻击
            Vector3 attackCenter = owner.transform.position + owner.transform.forward * (skillRange * 0.5f);
            float multiplier = 1f + chargeLevel * 0.5f; // 蓄力加成
            PerformAreaAttack(attackCenter, hitRadius, multiplier);
        }

        public override void OnAnimationEvent(string eventName)
        {
            base.OnAnimationEvent(eventName);

            // 支持动画事件触发攻击判定
            if (eventName == "SlashHit")
            {
                Vector3 attackCenter = owner.transform.position + owner.transform.forward * (skillRange * 0.5f);
                float multiplier = 1f + chargeLevel * 0.5f;
                PerformAreaAttack(attackCenter, hitRadius, multiplier);
            }
        }
    }

    /// <summary>
    /// 示例技能2 - 远程投射物
    /// </summary>
    [AbilityId("skill_projectile")]
    public class ProjectileSkill : SkillAbility
    {
        [Header("投射物配置")]
        private float projectileSpeed = 20f;
        private float projectileRange = 15f;

        public override void Init(AbilityContext context)
        {
            base.Init(context);
            Priority = 15;
            canCharge = true;
            maxChargeTime = 1.5f;
        }

        protected override void OnActivate()
        {
            base.OnActivate();
            // 技能动画后续再加
        }

        protected override void OnActiveStart()
        {
            base.OnActiveStart();

            // 发射投射物（简化版：使用射线检测）
            Vector3 origin = owner.transform.position + Vector3.up * 1.5f;
            Vector3 direction = owner.transform.forward;

            // 蓄力增加射程
            float range = projectileRange * (0.5f + chargeLevel * 0.5f);
            float multiplier = 1f + chargeLevel;

            PerformRayAttack(origin, direction, range, multiplier);
        }

        protected override void OnChargeUpdate(float dt, float level)
        {
            // 蓄力动画后续再加
        }

        protected override void OnChargeEnd(float finalLevel)
        {
            // 蓄力动画后续再加
        }
    }

    /// <summary>
    /// 终极技能 - 需要充满能量槽
    /// </summary>
    [AbilityId("ultimate")]
    public class UltimateAbility : SkillAbility
    {
        private float requiredEnergy = 100f; // 需要满能量

        public override void Init(AbilityContext context)
        {
            base.Init(context);
            Priority = 25; // 最高优先级
        }

        public override bool TryActivate()
        {
            // 终极技能需要满能量
            if (system.CurrentEnergy < requiredEnergy)
                return false;

            return base.TryActivate();
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            // 消耗全部能量
            system.SetEnergy(0f);

            // 终极技能动画后续再加
            // system.SetEnergy(0f); 保持能量消耗
        }

        protected override void OnActiveStart()
        {
            base.OnActiveStart();

            // 大范围AOE攻击
            Vector3 center = owner.transform.position;
            float radius = 8f;
            float multiplier = 3f;

            PerformAreaAttack(center, radius, multiplier);
        }
    }
}
