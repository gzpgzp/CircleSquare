using Battle.Character.Ability.A2D;
using SquareBattle.BattleCore;
using UnityEngine;

namespace SquareBattle.BattleCore.Characters
{
    /// <summary>
    /// SquareBattle Archer 角色 - 远程弓箭手（新系统版本）
    /// 复用 Battle 系统的 MultiRangedAttackAbility2D 能力（支持多子弹扇形射击）
    /// </summary>
    public class SquareBattleArcher : CharacterBase
    {
        // ── Inspector 参数 ──────────────────────────────────────────────────────────
        [Header("Attack")]
        [Tooltip("每次发射的子弹数量（1 = 单发）")]
        [SerializeField] private int bulletCount = 1;

        [Tooltip("相邻两发子弹的间隔角度（度），bulletCount=1 时无效")]
        [SerializeField] private float angleStep = 15f;

        [Tooltip("攻击间隔（秒）")]
        [SerializeField] private float attackInterval = 1.5f;

        [Tooltip("最远攻击距离，超出不攻击")]
        [SerializeField] private float attackRange = 10f;

        [Tooltip("最近攻击距离，太近切换近战或后退（可为 0 表示不限）")]
        [SerializeField] private float minRange = 1.5f;

        [Tooltip("投射物预制体（需挂 IProjectile2D）")]
        [SerializeField] private GameObject projectilePrefab;

        [Tooltip("投射物飞行速度")]
        [SerializeField] private float projectileSpeed = 12f;

        [Tooltip("投射物基础伤害")]
        [SerializeField] private int projectileDamage = 15;

        // ── 运行时状态 ──────────────────────────────────────────────────────────
        private MultiRangedAttackAbility2D rangedAttackAbility;

        protected override void Awake()
        {
            base.Awake();
            
            // 创建并添加 MultiRangedAttackAbility2D 能力
            rangedAttackAbility = new MultiRangedAttackAbility2D();
            
            // 配置能力参数
            rangedAttackAbility.bulletCount = bulletCount;
            rangedAttackAbility.angleStep = angleStep;
            rangedAttackAbility.attackInterval = attackInterval;
            rangedAttackAbility.attackRange = attackRange;
            rangedAttackAbility.minRange = minRange;
            rangedAttackAbility.projectilePrefab = projectilePrefab;
            rangedAttackAbility.projectileSpeed = projectileSpeed;
            rangedAttackAbility.projectileDamage = projectileDamage;
            
            // 添加能力到角色
            // 需要适配 CharacterBase 来支持能力系统
        }

        protected override void Start()
        {
            base.Start();
            
            // 初始化能力系统
            // 需要根据实际情况实现
        }

        // 重写 Update 方法来集成能力系统
        protected override void Update()
        {
            base.Update();
            
            // 在这里调用能力系统的 Update 逻辑
        }
    }
}