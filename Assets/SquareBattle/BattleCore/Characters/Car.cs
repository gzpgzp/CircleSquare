using Tools.GameObjectPools;
using UnityEngine;

namespace SquareBattle.BattleCore.Characters
{
    /// <summary>
    /// 喝醉的大运 —— 向前行驶时持续向左/右漂移，直到撞墙或撞到敌人
    /// </summary>
    public class Car : CharacterBase
    {
        [Header("Car Drift")]
        [Tooltip("横向漂移加速度最小值（每秒增加的侧向速度）")]
        [SerializeField] private float minDriftAcceleration = 0.8f;

        [Tooltip("横向漂移加速度最大值（每秒增加的侧向速度）")]
        [SerializeField] private float maxDriftAcceleration = 2.5f;

        [Tooltip("最大横向漂移速度")]
        [SerializeField] private float maxDriftSpeed = 4f;

        // 当前使用的漂移加速度（每次碰撞后随机）
        private float driftAcceleration;

        [Tooltip("碰撞后原地等待再索敌的时间（秒）")]
        [SerializeField] private float waitAfterBounce = 1f;

        [Tooltip("碰撞后后退的速度")]
        [SerializeField] private float recoilSpeed = 4f;

        [Tooltip("后退持续时间（秒）")]
        [SerializeField] private float recoilDuration = 0.2f;

        [Header("Car Throw")]
        [Tooltip("碰撞后投掷的投射物预制体（需挂 Projectile.cs）")]
        [SerializeField] private GameObject projectilePrefab;

        [Tooltip("碰撞后延迟多少秒后投掷（应小于 waitAfterBounce）")]
        [SerializeField] private float throwDelayAfterBounce = 0.5f;

        [Header("Car Hit")]
        [Tooltip("撞到敌方角色时给对方施加的弹飞速度倍率")]
        [SerializeField] private float hitSpeedMultiplier = 3f;

        // 当前漂移方向：+1 向右，-1 向左
        private float driftDirection;
        // 当前累积的横向漂移速度
        private float currentDriftSpeed;

        // 等待计时器；> 0 时原地静止
        private float _waitTimer;

        // 后退计时器；> 0 时向后退
        private float _recoilTimer;

        // 后退方向（碰撞法线反方向）
        private Vector2 _recoilDir;

        // 投掷倒计时；> 0 时等待投掷
        private float _throwTimer;

        // 是否已完成本次碰撞的投掷
        private bool _hasThrown;

        // 缓存 Rigidbody2D，避免每帧 GetComponent
        private Rigidbody2D _rb;

        protected override void Awake()
        {
            base.Awake();
            _rb = GetComponent<Rigidbody2D>();
            // 随机初始漂移方向与加速度
            driftDirection = Random.value > 0.5f ? 1f : -1f;
            driftAcceleration = Random.Range(minDriftAcceleration, maxDriftAcceleration);
            currentDriftSpeed = 0f;
            _waitTimer = 0f;
            _recoilTimer = 0f;
            _throwTimer = 0f;
            _hasThrown = true;
        }

        protected override void Start()
        {
            base.Start();
            // 初始索敌：朝最近的敌方角色方向出发
            AcquireTarget();
            // 注册投射物预制体到对象池
            if (projectilePrefab != null)
                GameObjectPool.Instance.Register(projectilePrefab);
        }

        /// <summary>
        /// 寻找最近的不同阵营角色，并将 moveDir 指向该角色，同时旋转自身朝向目标
        /// </summary>
        private void AcquireTarget()
        {
            CharacterBase nearest = FindNearestEnemy();
            if (nearest != null)
            {
                moveDir = ((Vector2)(nearest.transform.position - transform.position)).normalized;
                // 朝向目标（左右翻转）
                Vector3 scale = transform.localScale;
                scale.x = moveDir.x > 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }

        /// <summary>
        /// 遍历 CharacterManager 中所有角色，返回最近的不同阵营角色
        /// </summary>
        private CharacterBase FindNearestEnemy()
        {
            float minDist = float.MaxValue;
            CharacterBase nearest = null;

            foreach (var c in SquareBattleCharacterRegistry.Instance.AllCharacters)
            {
                if (c == this) continue;
                if (c.GetCamp() == this.camp) continue;
                if (c.IsDead()) continue;

                float dist = Vector2.Distance(transform.position, c.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = c;
                }
            }

            return nearest;
        }

        protected override void Update()
        {
            base.Update();

            // 投掷倒计时处理
            if (_throwTimer > 0f)
            {
                _throwTimer -= Time.deltaTime;
                if (_throwTimer <= 0f && !_hasThrown)
                {
                    _throwTimer = 0f;
                    TryThrowProjectile();
                    _hasThrown = true;
                }
            }
        }

        /// <summary>
        /// 朝最近敌人投出一个投射物
        /// </summary>
        private void TryThrowProjectile()
        {
            if (projectilePrefab == null) return;

            CharacterBase target = FindNearestEnemy();
            if (target == null) return;

            Vector2 dir = ((Vector2)(target.transform.position - transform.position)).normalized;

            GameObject projObj = GameObjectPool.Instance.CreateGameObject(projectilePrefab.name);
            if (projObj == null) return;
            projObj.transform.position = transform.position;
            projObj.transform.rotation = Quaternion.identity;
            Projectile projectile = projObj.GetComponent<Projectile>();
            if (projectile != null)
                projectile.Init(dir, camp, this);
        }

        protected override void HandleMove()
        {
            if (isDead) return;

            // 后退阶段：向碰撞反方向滑行
            if (_recoilTimer > 0f)
            {
                _recoilTimer -= Time.fixedDeltaTime;
                _rb.velocity = _recoilDir * recoilSpeed;
                return;
            }

            // 等待阶段：原地静止，倒计时结束后索敌出发
            if (_waitTimer > 0f)
            {
                _waitTimer -= Time.fixedDeltaTime;
                _rb.velocity = Vector2.zero;

                if (_waitTimer <= 0f)
                {
                    _waitTimer = 0f;
                    // 等待结束，重新索敌
                    AcquireTarget();
                    currentDriftSpeed = 0f;
                }
                return;
            }

            // 累积横向漂移速度（随时间逐渐增大）
            currentDriftSpeed += driftAcceleration * Time.fixedDeltaTime;
            currentDriftSpeed = Mathf.Min(currentDriftSpeed, maxDriftSpeed);

            // 垂直于前进方向的侧向轴（顺时针旋转 90 度）
            Vector2 lateral = new Vector2(-moveDir.y, moveDir.x);

            // 保持固定前进速度，叠加侧向漂移速度
            _rb.velocity = moveDir * moveSpeed + lateral * (driftDirection * currentDriftSpeed);
        }

        public override void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDead) return;

            CharacterBase other = collision.collider.GetComponent<CharacterBase>();

            if (other == null)
            {
                // 撞墙：自己处理，不走基类角色对碰逻辑
                if (!collision.gameObject.CompareTag("Wall")) return;
                OnBounce(collision.contacts[0].normal);
                return;
            }

            // Car 撞敌方：永远由 Car 主动处理（额外效果：弹飞对方）
            if (other.GetCamp() != this.camp)
            {
                ContactPoint2D contact = collision.contacts[0];
                OnBounce(contact.normal);
                other.OnHitByCharacter(-contact.normal, hitSpeedMultiplier);
                return;
            }

            // 同阵营：走 InstanceID 仲裁，让较大的一方处理双方反射
            if (GetInstanceID() < other.GetInstanceID()) return;
            {
                ContactPoint2D contact = collision.contacts[0];
                OnBounce(contact.normal);
                other.OnBounce(-contact.normal);
            }
        }

        public override void OnBounce(Vector2 normal)
        {
            // 调用基类：反射 moveDir
            base.OnBounce(normal);
            // 碰撞后随机漂移方向与加速度
            driftDirection = Random.value > 0.5f ? 1f : -1f;
            driftAcceleration = Random.Range(minDriftAcceleration, maxDriftAcceleration);
            currentDriftSpeed = 0f;
            // 后退方向 = 碰撞法线方向（被推开方向）
            _recoilDir = normal.normalized;
            _recoilTimer = recoilDuration;
            // 只有不在等待期间才启动等待计时，避免静止中碰撞重置时间
            if (_waitTimer <= 0f)
            {
                _waitTimer = waitAfterBounce;
            }
            // 启动投掷倒计时
            _throwTimer = throwDelayAfterBounce;
            _hasThrown = false;
        }
    }
}
