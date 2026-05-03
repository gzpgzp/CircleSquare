using UnityEngine;

namespace Battle.Character.Ability.A2D
{
    /// <summary>
    /// AI 自动多子弹远程攻击能力（2D）。
    /// 
    /// 行为逻辑：
    ///   - 从 Blackboard 读取目标，进入 attackRange 且超过 minRange 时发射多发子弹
    ///   - 支持扇形分布：以目标方向为中心，按 angleStep 角度间隔发射 bulletCount 发子弹
    ///   - 间隔 attackInterval 射出一次
    ///   - 投射物需要挂载 <see cref="IProjectile2D"/> 实现具体碰撞/伤害逻辑
    /// 
    /// 与旧系统解耦：
    ///   不直接依赖 SquareBattle 的 Projectile，而是通过 IProjectile2D 接口注入伤害。
    ///   若要复用 SquareBattle.Projectile，给它加上 IProjectile2D 适配即可。
    /// </summary>
    [Ability(AbilityEnum.MultiRangedAttack2D)]
    public class MultiRangedAttackAbility2D : RangedAttackAbility2D
    {
        // ──────────────────────── 参数 ────────────────────────

        /// <summary>每次发射的子弹数量（1 = 单发）</summary>
        public int bulletCount = 1;

        /// <summary>相邻两发子弹的间隔角度（度），bulletCount=1 时无效</summary>
        public float angleStep = 15f;

        // ──────────────────────── 运行时状态 ────────────────────────

        private float attackTimer;

        public MultiRangedAttackAbility2D()
        {
            OwnTag = AbilityTag.Action;
            isEnabled = true;
        }

        protected override void OnTickUpdate(float deltaTime)
        {
            attackTimer -= deltaTime;

            if (!blackboard.HasTarget) return;

            float dist = blackboard.DistanceToTarget;

            // 在有效射程内才攻击
            if (dist > attackRange) return;
            if (minRange > 0f && dist < minRange) return;

            // 朝向目标
            FaceTarget(blackboard.DirectionToTarget);

            if (attackTimer <= 0f)
            {
                Shoot(blackboard.DirectionToTarget);
                attackTimer = attackInterval;
            }
        }

        // ──────────────────────── 发射 ────────────────────────

        private void Shoot(Vector2 direction)
        {
            if (projectilePrefab == null)
            {
                Debug.LogWarning($"[MultiRangedAttackAbility2D] projectilePrefab 未赋值，无法发射！");
                return;
            }

            // 总展开角 = (bulletCount - 1) * angleStep
            // 左起始角偏移 = -总展开角 / 2
            float totalSpread = (bulletCount - 1) * angleStep;
            float startAngle = -totalSpread * 0.5f;

            for (int i = 0; i < bulletCount; i++)
            {
                float angle = startAngle + i * angleStep;
                Vector2 rotDir = RotateVector(direction, angle);
                
                var go = Object.Instantiate(
                    projectilePrefab,
                    character2D.transform.position,
                    Quaternion.identity
                );

                // 优先通过接口初始化（解耦方式）
                var proj = go.GetComponent<IProjectile2D>();
                if (proj != null)
                {
                    proj.Init(rotDir, projectileSpeed, projectileDamage, owner);
                    return;
                }

                // 兜底：如果挂的是旧系统的 SquareBattle.Projectile，给出提示
                Debug.LogWarning(
                    $"[MultiRangedAttackAbility2D] 投射物 {go.name} 未实现 IProjectile2D 接口，" +
                    "请为预制体添加实现该接口的组件，或使用 SquareBattleProjectileAdapter 适配。"
                );
            }
        }

        // ──────────────────────── 辅助方法 ────────────────────────

        /// <summary>将向量按指定角度（度）旋转</summary>
        private static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
