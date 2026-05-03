using Battle.Character.Ability.A2D;
using SquareBattle.BattleCore;
using UnityEngine;

namespace SquareBattle.BattleCore.Characters
{
    /// <summary>
    /// SquareBattle Car 角色 - 醉酒漂移车辆（新系统版本）
    /// 复用 Battle 系统的 DriftMoveAbility2D 能力
    /// </summary>
    public class SquareBattleCar : CharacterBase
    {
        // ── Inspector 参数 ──────────────────────────────────────────────────────────
        [Header("Car Drift")]
        [Tooltip("横向漂移加速度最小值（每秒增加的侧向速度）")]
        [SerializeField] private float minDriftAcceleration = 0.8f;

        [Tooltip("横向漂移加速度最大值（每秒增加的侧向速度）")]
        [SerializeField] private float maxDriftAcceleration = 2.5f;

        [Tooltip("最大横向漂移速度")]
        [SerializeField] private float maxDriftSpeed = 4f;

        [Header("Car Throw")]
        [Tooltip("碰撞后投掷的投射物预制体（需挂 IProjectile2D）")]
        [SerializeField] private GameObject projectilePrefab;

        [Tooltip("碰撞后延迟多少秒后投掷（应小于 waitAfterBounce）")]
        [SerializeField] private float throwDelayAfterBounce = 0.5f;

        [Header("Car Hit")]
        [Tooltip("撞到敌方角色时给对方施加的弹飞速度倍率")]
        [SerializeField] private float hitSpeedMultiplier = 3f;

        // ── 运行时状态 ──────────────────────────────────────────────────────────
        private DriftMoveAbility2D driftMoveAbility;

        protected override void Awake()
        {
            base.Awake();
            
            // 创建并添加 DriftMoveAbility2D 能力
            driftMoveAbility = new DriftMoveAbility2D();
            
            // 配置能力参数
            driftMoveAbility.minDriftAcceleration = minDriftAcceleration;
            driftMoveAbility.maxDriftAcceleration = maxDriftAcceleration;
            driftMoveAbility.maxDriftSpeed = maxDriftSpeed;
            driftMoveAbility.projectilePrefab = projectilePrefab;
            driftMoveAbility.throwDelayAfterBounce = throwDelayAfterBounce;
            driftMoveAbility.hitSpeedMultiplier = hitSpeedMultiplier;
            
            // 添加能力到角色
            // 注意：这里需要适配器或修改 CharacterBase 来支持能力系统
            // 由于 CharacterBase 是旧系统，我们需要创建一个适配层
        }

        protected override void Start()
        {
            base.Start();
            
            // 初始化能力系统
            // 这里需要根据实际情况实现能力系统的集成
        }

        // 重写 Update 方法来集成能力系统
        protected override void Update()
        {
            base.Update();
            
            // 在这里调用能力系统的 Update 逻辑
            // 由于 CharacterBase 没有直接支持能力系统，我们需要适配
        }
    }
}
