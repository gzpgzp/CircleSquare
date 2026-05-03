using System.Collections;
using UnityEngine;

namespace Battle.Character.Ability.A2D
{
    /// <summary>
    /// AI 自动近战连段攻击能力（2D）。
    /// 
    /// 行为逻辑：
    ///   - 每帧从 Blackboard 读取目标信息，无需任何输入绑定
    ///   - 目标进入 attackRange → 发起一轮 comboCount 次连段
    ///   - 连段之间间隔 comboInterval，完成后冷却 attackCooldown
    ///   - 打击时以 hitRadius 做圆形 OverlapCircle 范围伤害（可命中多个目标）
    /// 
    /// 使用方式：
    ///   直接 new MeleeAttackAbility2D() 或通过 AbilityInitializer.CreateAbility(AbilityEnum.MeleeAttack2D)
    ///   由 CharacterManager.CreateEnemy() 挂载到 AIBrain
    /// </summary>
    [Ability(AbilityEnum.MeleeAttack2D)]
    public class MeleeAttackAbility2D : AIAbility2D
    {
        // ──────────────────────── 参数（可在构造函数或外部赋值）────────────────────────

        /// <summary>触发攻击的最大距离</summary>
        public float attackRange = 2.5f;

        /// <summary>每段攻击的基础伤害</summary>
        public int attackDamage = 20;

        /// <summary>连段次数（1 = 单次普攻，>1 = 连段）</summary>
        public int comboCount = 1;

        /// <summary>连段内每次攻击的间隔（秒）</summary>
        public float comboInterval = 0.3f;

        /// <summary>完成一轮连段后的冷却（秒）</summary>
        public float attackCooldown = 1.5f;

        /// <summary>打击判定圆半径</summary>
        public float hitRadius = 0.6f;

        // ──────────────────────── 运行时状态 ────────────────────────

        private float cooldownTimer;
        private bool isAttacking;

        public MeleeAttackAbility2D()
        {
            OwnTag = AbilityTag.Action;
            isEnabled = true;
        }

        protected override void OnTickUpdate(float deltaTime)
        {
            // 冷却倒计时
            if (cooldownTimer > 0f)
            {
                cooldownTimer -= deltaTime;
                return;
            }

            // 连段进行中不重复触发
            if (isAttacking) return;

            // 无有效目标则等待
            if (blackboard == null || !blackboard.HasTarget) return;

            // 目标超出攻击范围则等待
            if (blackboard.DistanceToTarget > attackRange) return;

            // 朝向目标翻转 Sprite
            FaceTarget(blackboard.DirectionToTarget);

            // 发起连段（协程）
            character2D.StartCoroutine(ComboRoutine());
        }

        // ──────────────────────── 连段逻辑 ────────────────────────

        private IEnumerator ComboRoutine()
        {
            isAttacking = true;

            for (int i = 0; i < comboCount; i++)
            {
                // 目标失效或超范围则中断
                if (blackboard == null || !blackboard.HasTarget) break;
                if (blackboard.DistanceToTarget > attackRange) break;

                PerformHit(i);

                if (i < comboCount - 1)
                    yield return new WaitForSeconds(comboInterval);
            }

            isAttacking = false;
            cooldownTimer = attackCooldown;
        }

        /// <summary>
        /// 单次打击：以角色位置为圆心，hitRadius 为半径，对所有不同阵营角色造成伤害。
        /// 子类可 override 以实现击退、特效等附加效果。
        /// </summary>
        protected virtual void PerformHit(int comboIndex)
        {
            if (character2D == null) return;

            Collider2D[] hits = Physics2D.OverlapCircleAll(
                character2D.transform.position, hitRadius
            );

            foreach (var col in hits)
            {
                var target = col.GetComponent<BaseCharacter>();
                if (target == null) continue;
                if (target == owner) continue;
                if (target.IsDead()) continue;

                OnMeleeHit(target, comboIndex);
            }
        }

        /// <summary>
        /// 命中回调，默认直接扣血。子类可 override 扩展。
        /// </summary>
        protected virtual void OnMeleeHit(BaseCharacter target, int comboIndex)
        {
            // 获取暴击修饰器
            var criticalModifier = GetCriticalModifier();
            
            if (criticalModifier != null && criticalModifier.ShouldCrit())
            {
                // 暴击伤害
                int criticalDamage = (int)(attackDamage * criticalModifier.CriticalMultiplier);
                target.TakeDamage(criticalDamage);
                
                // 播放暴击特效
                PlayCriticalEffect(target.transform.position);
            }
            else
            {
                // 普通伤害
                target.TakeDamage(attackDamage);
            }
        }

        /// <summary>
        /// 获取暴击修饰器
        /// </summary>
        private CriticalHitModifier GetCriticalModifier()
        {
            if (owner == null) return null;
            
            // 查找已激活的暴击修饰器
            foreach (var ability in owner.GetComponents<BaseAbility>())
            {
                if (ability is CriticalHitModifier modifier && modifier.IsEnabled)
                {
                    return modifier;
                }
            }
            
            return null;
        }

        /// <summary>
        /// 播放暴击特效
        /// </summary>
        private void PlayCriticalEffect(Vector3 position)
        {
            // 这里可以集成特效播放逻辑
        }

        // ──────────────────────── 朝向 ────────────────────────

        /// <summary>
        /// 按目标方向翻转 Sprite（Sprite 默认面左规则）
        /// </summary>
        private void FaceTarget(Vector2 dir)
        {
            if (character2D == null) return;
            Vector3 scale = character2D.transform.localScale;
            scale.x = dir.x > 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            character2D.transform.localScale = scale;
        }

        // ──────────────────────── Gizmos ────────────────────────

#if UNITY_EDITOR
        // 注意：Ability 是纯 C# 类，无法直接写 OnDrawGizmosSelected
        // 如需可视化，在角色 MonoBehaviour 上添加 GizmoDrawer 组件
#endif
    }
}
