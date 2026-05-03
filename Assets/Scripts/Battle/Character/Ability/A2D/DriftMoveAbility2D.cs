using System.Collections;
using Tools.GameObjectPools;
using UnityEngine;

namespace Battle.Character.Ability.A2D
{
    /// <summary>
    /// Car 角色的醉酒漂移移动能力（2D 俯视）。
    ///
    /// 整合了旧系统 Car.cs 中所有移动相关逻辑：
    ///   - 持续朝目标匀速前进，同时叠加随机侧向漂移
    ///   - 碰撞墙壁/敌人后：后退 → 等待 → 重新索敌出发
    ///   - 碰撞后延迟投出一枚投射物（可选）
    ///   - 撞到敌人时对其施加强力弹飞
    ///
    /// 依赖：
    ///   - character2D.motor（CharacterMotor2D）控制速度
    ///   - Blackboard 提供目标方向（由 AIBrain 写入）
    ///   - 挂载此 Ability 的 GameObject 上需有 Rigidbody2D + Collider2D
    ///   - 碰撞事件由 DriftMoveCollisionBridge（MonoBehaviour 组件）转发过来
    /// </summary>
    [Ability(AbilityEnum.DriftMove2D)]
    public class DriftMoveAbility2D : AIAbility2D
    {
        // ──────────────────────── 参数 ────────────────────────

        [Header("漂移")]
        public float minDriftAcceleration = 0.8f;
        public float maxDriftAcceleration = 2.5f;
        public float maxDriftSpeed = 4f;
        public float moveSpeed = 5f;

        [Header("碰撞后退")]
        public float recoilSpeed = 4f;
        public float recoilDuration = 0.2f;
        public float waitAfterBounce = 1f;

        [Header("碰撞投射物")]
        /// <summary>碰撞后要投出的预制体（需挂 IProjectile2D 实现，如 SquareBattleProjectileAdapter）</summary>
        public GameObject projectilePrefab;
        public float throwDelayAfterBounce = 0.5f;
        public int projectileDamage = 20;
        public float projectileSpeed = 10f;

        [Header("撞击弹飞")]
        /// <summary>撞到敌方角色时，对其施加的弹飞速度倍率（需目标实现 IBounceTarget）</summary>
        public float hitSpeedMultiplier = 3f;

        // ──────────────────────── 运行时状态 ────────────────────────

        private Vector2 _moveDir;
        private float _driftDirection;
        private float _driftAcceleration;
        private float _currentDriftSpeed;

        private float _recoilTimer;
        private Vector2 _recoilDir;

        private float _waitTimer;

        private float _throwTimer;
        private bool _hasThrown = true;

        public DriftMoveAbility2D()
        {
            OwnTag = AbilityTag.Movement;
            isEnabled = true;
        }

        public override void BindCharacter(BaseCharacter owner)
        {
            base.BindCharacter(owner);
            RandomizeDrift();

            // 自动查找或创建碰撞桥接组件，并将自身注入
            var bridge = owner.GetComponent<DriftMoveCollisionBridge>();
            if (bridge == null)
                bridge = owner.gameObject.AddComponent<DriftMoveCollisionBridge>();
            bridge.Bind(this);
        }

        // ──────────────────────── Tick ────────────────────────

        protected override void OnTickUpdate(float deltaTime)
        {
            // 投掷倒计时
            if (_throwTimer > 0f)
            {
                _throwTimer -= deltaTime;
                if (_throwTimer <= 0f && !_hasThrown)
                {
                    _throwTimer = 0f;
                    TryThrowProjectile();
                    _hasThrown = true;
                }
            }

            // 后退阶段
            if (_recoilTimer > 0f)
            {
                _recoilTimer -= deltaTime;
                character2D.motor.SetVelocity(_recoilDir * recoilSpeed);
                return;
            }

            // 等待阶段
            if (_waitTimer > 0f)
            {
                _waitTimer -= deltaTime;
                character2D.motor.SetVelocity(Vector2.zero);

                if (_waitTimer <= 0f)
                {
                    AcquireTarget();
                    _currentDriftSpeed = 0f;
                }
                return;
            }

            // 正常漂移移动
            _currentDriftSpeed += _driftAcceleration * deltaTime;
            _currentDriftSpeed = Mathf.Min(_currentDriftSpeed, maxDriftSpeed);

            Vector2 lateral = new Vector2(-_moveDir.y, _moveDir.x);
            character2D.motor.SetVelocity(
                _moveDir * moveSpeed + lateral * (_driftDirection * _currentDriftSpeed)
            );
        }

        // ──────────────────────── 碰撞响应（由 Bridge 调用）────────────────────────

        /// <summary>
        /// 由 DriftMoveCollisionBridge 在 OnCollisionEnter2D 时调用。
        /// </summary>
        public void OnCollision(Collision2D collision)
        {
            if (owner == null || owner.IsDead()) return;

            // 检查是否撞到了 BaseCharacter
            var other = collision.collider.GetComponent<BaseCharacter>();
            if (other != null)
            {
                // 撞到角色：弹飞对方，自己后退等待
                ContactPoint2D contact = collision.contacts[0];
                Vector2 normal = contact.normal;

                // 对方弹飞（若实现了 IBounceTarget）
                var bounceTarget = collision.collider.GetComponent<IBounceTarget>();
                bounceTarget?.OnHitByCharacter(-normal, hitSpeedMultiplier);

                OnBounce(normal);
                return;
            }

            // 撞到墙
            if (collision.gameObject.CompareTag("Wall"))
            {
                ContactPoint2D contact = collision.contacts[0];
                OnBounce(contact.normal);
            }
        }

        private void OnBounce(Vector2 normal)
        {
            // 反射移动方向
            _moveDir = Vector2.Reflect(_moveDir, normal).normalized;

            RandomizeDrift();
            _currentDriftSpeed = 0f;

            _recoilDir = normal.normalized;
            _recoilTimer = recoilDuration;

            if (_waitTimer <= 0f)
                _waitTimer = waitAfterBounce;

            // 启动投掷倒计时
            _throwTimer = throwDelayAfterBounce;
            _hasThrown = false;
        }

        // ──────────────────────── 辅助 ────────────────────────

        /// <summary>重新索敌：把 _moveDir 对准 Blackboard 中的目标方向并翻转朝向</summary>
        private void AcquireTarget()
        {
            if (blackboard == null || !blackboard.HasTarget) return;

            _moveDir = blackboard.DirectionToTarget;

            // 翻转 Sprite
            Vector3 scale = character2D.transform.localScale;
            scale.x = _moveDir.x > 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            character2D.transform.localScale = scale;
        }

        private void RandomizeDrift()
        {
            _driftDirection = Random.value > 0.5f ? 1f : -1f;
            _driftAcceleration = Random.Range(minDriftAcceleration, maxDriftAcceleration);
        }

        private void TryThrowProjectile()
        {
            if (projectilePrefab == null) return;
            if (blackboard == null || !blackboard.HasTarget) return;

            var go = Object.Instantiate(
                projectilePrefab,
                character2D.transform.position,
                Quaternion.identity
            );

            var proj = go.GetComponent<IProjectile2D>();
            if (proj != null)
            {
                proj.Init(blackboard.DirectionToTarget, projectileSpeed, projectileDamage, owner);
            }
            else
            {
                Debug.LogWarning("[DriftMoveAbility2D] 投射物未实现 IProjectile2D，请挂载适配器组件。");
            }
        }
    }

    // ──────────────────────── 弹飞接口（供旧系统角色实现）────────────────────────

    /// <summary>
    /// 任何希望被 Car 撞飞的角色，需在 MonoBehaviour 上实现此接口。
    /// 旧系统 CharacterBase 可加一个 CharacterBaseBounceAdapter 实现它。
    /// </summary>
    public interface IBounceTarget
    {
        void OnHitByCharacter(Vector2 normal, float speedMultiplier = 1f);
    }
}
