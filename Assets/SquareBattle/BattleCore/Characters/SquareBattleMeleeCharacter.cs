using Battle.Character.Ability.A2D;
using SquareBattle.BattleCore;
using UnityEngine;

namespace SquareBattle.BattleCore.Characters
{
    /// <summary>
    /// SquareBattle MeleeCharacter 角色 - 近战英雄（新系统版本）
    /// 复用 Battle 系统的 MeleeAttackAbility2D 能力
    /// </summary>
    public class SquareBattleMeleeCharacter : CharacterBase
    {
        // ── Inspector 参数 ──────────────────────────────────────────────────────────
        [Header("Melee Attack")]
        [Tooltip("触发攻击的范围（与目标的距离）")]
        [SerializeField] private float attackRange = 2.5f;

        [Tooltip("每次单段攻击造成的伤害")]
        [SerializeField] private int attackDamage = 20;

        [Tooltip("连段攻击次数（1 = 单次，>1 = 连段）")]
        [SerializeField] private int comboCount = 1;

        [Tooltip("连段中每次攻击之间的间隔（秒）")]
        [SerializeField] private float comboInterval = 0.3f;

        [Tooltip("完成一轮连段后的冷却时间（秒）")]
        [SerializeField] private float attackCooldown = 1.5f;

        [Tooltip("攻击瞬间打击判定的半径（通常略大于自身碰撞体）")]
        [SerializeField] private float hitRadius = 0.6f;

        [Header("Critical Hit")]
        [Tooltip("暴击发生的概率（0.0-1.0）")]
        [SerializeField] private float criticalChance = 0.2f;

        [Tooltip("暴击伤害倍率")]
        [SerializeField] private float criticalMultiplier = 2.0f;

        // ── 运行时状态 ──────────────────────────────────────────────────────────
        private MeleeAttackAbility2D meleeAttackAbility;

        protected override void Awake()
        {
            base.Awake();
            
            // 创建并添加 MeleeAttackAbility2D 能力
            meleeAttackAbility = new MeleeAttackAbility2D();
            
            // 配置能力参数
            meleeAttackAbility.attackRange = attackRange;
            meleeAttackAbility.attackDamage = attackDamage;
            meleeAttackAbility.comboCount = comboCount;
            meleeAttackAbility.comboInterval = comboInterval;
            meleeAttackAbility.attackCooldown = attackCooldown;
            meleeAttackAbility.hitRadius = hitRadius;
            
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
