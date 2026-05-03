using UnityEngine;

namespace Battle.Character.Ability.A2D
{
    /// <summary>
    /// AI 自动远程攻击能力（2D）。
    ///
    /// 行为逻辑：
    ///   - 从 Blackboard 读取目标，进入 attackRange 且超过 minRange 时发射投射物
    ///   - 间隔 attackInterval 射出一次
    ///   - 投射物需要挂载 <see cref="IProjectile2D"/> 实现具体碰撞/伤害逻辑
    ///
    /// 与旧系统解耦：
    ///   不直接依赖 SquareBattle 的 Projectile，而是通过 IProjectile2D 接口注入伤害。
    ///   若要复用 SquareBattle.Projectile，给它加上 IProjectile2D 适配即可。
    /// </summary>
    [Ability(AbilityEnum.RangedAttack2D)]
    public class RangedAttackAbility2D : AIAbility2D
    {
        // ──────────────────────── 参数 ────────────────────────

        /// <summary>最远攻击距离，超出不攻击</summary>
        public float attackRange = 10f;

        /// <summary>最近攻击距离，太近切换近战或后退（可为 0 表示不限）</summary>
        public float minRange = 1.5f;

        /// <summary>攻击间隔（秒）</summary>
        public float attackInterval = 1.5f;

        /// <summary>
        /// 投射物预制体。需要在运行时通过代码赋值，
        /// 或在具体角色的 Awake/Init 中注入到 Ability。
        /// </summary>
        public GameObject projectilePrefab;

        /// <summary>
        /// 投射物飞行速度（会写入 IProjectile2D.Init）
        /// </summary>
        public float projectileSpeed = 12f;

        /// <summary>
        /// 投射物基础伤害（会写入 IProjectile2D.Init）
        /// </summary>
        public int projectileDamage = 15;

        // ──────────────────────── 运行时状态 ────────────────────────

        private float attackTimer;

        public RangedAttackAbility2D()
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
                Debug.LogWarning($"[RangedAttackAbility2D] projectilePrefab 未赋值，无法发射！");
                return;
            }

            var go = Object.Instantiate(
                projectilePrefab,
                character2D.transform.position,
                Quaternion.identity
            );

            // 优先通过接口初始化（解耦方式）
            var proj = go.GetComponent<IProjectile2D>();
            if (proj != null)
            {
                proj.Init(direction, projectileSpeed, projectileDamage, owner);
                return;
            }

            // 兜底：如果挂的是旧系统的 SquareBattle.Projectile，给出提示
            Debug.LogWarning(
                $"[RangedAttackAbility2D] 投射物 {go.name} 未实现 IProjectile2D 接口，" +
                "请为预制体添加实现该接口的组件，或使用 SquareBattleProjectileAdapter 适配。"
            );
        }

        // ──────────────────────── 朝向 ────────────────────────

        protected void FaceTarget(Vector2 dir)
        {
            if (character2D == null) return;
            Vector3 scale = character2D.transform.localScale;
            scale.x = dir.x > 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            character2D.transform.localScale = scale;
        }
    }
}
