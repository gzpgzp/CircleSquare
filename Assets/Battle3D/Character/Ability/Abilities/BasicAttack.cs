using Battle3D.Character.Ability;
using Battle3D.Combat;
using UnityEngine;

namespace Battle3D.Character.Ability.Abilities
{
    /// <summary>
    /// 普攻连段能力 - 支持多段连击
    /// 每段有不同的伤害倍率和动画
    /// 通过连招窗口期实现连招衔接
    /// </summary>
    [AbilityId("basic_attack")]
    public class BasicAttack : BaseAbility
    {
        // 连招状态
        private int currentComboIndex;
        private float comboWindowTimer;
        private bool waitingForNextInput;

        // 连招配置
        private int maxComboCount = 4;
        private float comboWindow = 0.6f;
        private float[] comboDamageMultipliers = { 1.0f, 1.0f, 1.2f, 1.5f };
        private float[] comboActiveTimes = { 0.2f, 0.2f, 0.25f, 0.35f };
        private float[] comboRecoveryTimes = { 0.15f, 0.15f, 0.2f, 0.3f };

        // 判定
        private bool hasHitThisCombo; // 每段攻击只命中一次
        private HitDetector hitDetector;

        public int CurrentComboIndex => currentComboIndex;
        public int MaxComboCount => maxComboCount;

        public override void Init(AbilityContext context)
        {
            base.Init(context);
            Priority = 10;
            currentComboIndex = 0;

            // 从上下文读取连招数
            if (context.comboCount > 0)
                maxComboCount = context.comboCount;
            if (context.comboWindow > 0f)
                comboWindow = context.comboWindow;
        }

        public override bool TryActivate()
        {
            // 普攻可以在连招窗口期再次激活
            if (waitingForNextInput)
            {
                // 推进连招
                currentComboIndex++;
                if (currentComboIndex >= maxComboCount)
                {
                    // 连招结束，重置
                    currentComboIndex = 0;
                    waitingForNextInput = false;
                    EndAbility();
                    return false;
                }

                // 直接进入下一段的前摇
                waitingForNextInput = false;
                hasHitThisCombo = false;
                CurrentPhase = AbilityPhase.Windup;
                phaseTimer = ctx.windupTime;
                OnActivate();
                return true;
            }

            // 正常的首次激活
            if (CurrentPhase == AbilityPhase.Ready || CurrentPhase == AbilityPhase.Cooldown)
            {
                currentComboIndex = 0;
                hasHitThisCombo = false;
                return base.TryActivate();
            }

            return false;
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            // 设置当前段的时间参数
            if (currentComboIndex < comboActiveTimes.Length)
            {
                ctx.activeTime = comboActiveTimes[currentComboIndex];
            }
            if (currentComboIndex < comboRecoveryTimes.Length)
            {
                ctx.recoveryTime = comboRecoveryTimes[currentComboIndex];
            }

            hasHitThisCombo = false;

            // 触发动画
            TriggerAttackAnimation();
        }

        protected override void OnActiveStart()
        {
            base.OnActiveStart();
            hasHitThisCombo = false;
        }

        protected override void OnActiveUpdate(float dt)
        {
            base.OnActiveUpdate(dt);

            // 在生效阶段进行攻击判定
            if (!hasHitThisCombo)
            {
                PerformHitDetection();
            }
        }

        protected override void OnActiveEnd()
        {
            base.OnActiveEnd();

            // 进入连招窗口期
            waitingForNextInput = true;
            comboWindowTimer = comboWindow;
        }

        protected override void OnTickUpdate(float dt)
        {
            base.OnTickUpdate(dt);

            // 连招窗口计时
            if (waitingForNextInput)
            {
                comboWindowTimer -= dt;
                if (comboWindowTimer <= 0f)
                {
                    // 窗口期过期，连招重置
                    waitingForNextInput = false;
                    currentComboIndex = 0;
                    EndAbility();
                }
            }
        }

        protected override void OnDeactivate()
        {
            base.OnDeactivate();
            waitingForNextInput = false;
        }

        private void PerformHitDetection()
        {
            if (owner == null) return;

            // 前方扇形检测
            Vector3 origin = owner.transform.position + owner.transform.forward * 1f;
            float radius = 1.5f;
            Collider[] hits = Physics.OverlapSphere(origin, radius);

            foreach (var hit in hits)
            {
                var targetChar = hit.GetComponent<BaseCharacter>();
                if (targetChar != null && targetChar != owner)
                {
                    float damageMultiplier = GetComboDamageMultiplier();
                    DealDamage(targetChar, damageMultiplier);
                    hasHitThisCombo = true;
                    break; // 每段只命中一个目标（可扩展为多个）
                }
            }
        }

        private float GetComboDamageMultiplier()
        {
            if (currentComboIndex < comboDamageMultipliers.Length)
                return comboDamageMultipliers[currentComboIndex];
            return 1f;
        }

        private void DealDamage(BaseCharacter target, float multiplier)
        {
            var damageInfo = new DamageInfo
            {
                attacker = owner,
                baseDamage = owner.AttackPower,
                damageMultiplier = multiplier * (ctx?.damageMultiplier ?? 1f),
                knockbackForce = ctx?.knockbackForce ?? 2f,
                hitPoint = target.transform.position,
                isCritical = false, // 后续可扩展暴击系统
            };

            target.TakeDamage(damageInfo);
        }

        private void TriggerAttackAnimation()
        {
            // 触发单一攻击动画
            var char3D = owner as Character3D;
            if (char3D?.AnimatorComponent != null)
            {
                char3D.AnimatorComponent.SetTrigger("Attack");
            }
        }

        public override void OnAnimationEvent(string eventName)
        {
            base.OnAnimationEvent(eventName);

            if (eventName == "HitDetect" && !hasHitThisCombo)
            {
                PerformHitDetection();
            }
        }

        public override void OnInterrupted()
        {
            currentComboIndex = 0;
            waitingForNextInput = false;
            base.OnInterrupted();
        }
    }
}
