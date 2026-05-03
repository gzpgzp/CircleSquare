using System;
using System.Collections.Generic;
using UnityEngine;
using SquareBattle.BattleCore.UI;
using Random = UnityEngine.Random;

namespace SquareBattle.BattleCore
{
    public enum MoveType
    {
        None,
        Normal,
        Drunk
    }

    public enum Camp
    {
        Camp1,
        Camp2
    }

    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterBase : MonoBehaviour
    {
        [SerializeField] private SpriteRenderer spriteRenderer;
        
        [Header("Base")]
        [SerializeField] protected Camp camp;
        
        [Header("Stats")]
        [SerializeField] protected int maxHp = 100;
        protected int currentHp;
        
        [Header("Movement")] [SerializeField] private MoveType moveType;
        [SerializeField] protected Vector2 moveDir = Vector2.right;
        [SerializeField] protected float moveSpeed = 5f;

        [Header("Hit Boost")]
        [Tooltip("被角色撞击后的临时加速倍率")]
        [SerializeField] private float hitBoostMultiplier = 2.5f;

        [Tooltip("被角色撞击后加速持续时间（秒）")]
        [SerializeField] private float hitBoostDuration = 0.4f;

        // 被撞加速计时器
        private float hitBoostTimer;
        // 弹飞速度倍率（由撞撻方传入，默认=1）
        private float _hitSpeedMultiplier = 1f;

        [Header("Init")]
        [Tooltip("启动时随机初始移动方向（MoveType=None 时无效）")]
        [SerializeField] private bool randomInitDir = true;

        private Rigidbody2D rb;
        // 当前正在接触的对象集合，防止同一次碰撞重复触发
        private readonly HashSet<Collider2D> _activeContacts = new();
        
        protected bool isDead = false;

        // Spawn 时传入的数据，在 Awake 末尾应用
        private CharacterSpawnData _pendingSpawnData;

        /// <summary>
        /// 在 Instantiate 后、Awake 前调用，设置生成数据。
        /// Awake 会自动应用。
        /// </summary>
        public void SetSpawnData(CharacterSpawnData data)
        {
            _pendingSpawnData = data;
        }
        
        protected virtual void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            rb.gravityScale = 0;
            rb.collisionDetectionMode = CollisionDetectionMode2D.Continuous;

            moveDir = moveDir.normalized;
            if (randomInitDir && moveType != MoveType.None)
                moveDir = Random.insideUnitCircle.normalized;
            currentHp = maxHp;
        }

        protected virtual void Start()
        {
            // 应用生成数据（Awake 后、Start 前 SetSpawnData 已被调用）
            ApplySpawnData(_pendingSpawnData);

            // Start 时注册血条（此时 HealthBarManager 已完成 Awake 初始化）
            HealthBarManager.Instance?.Register(this, currentHp, maxHp);
        }

        protected virtual void Update()
        {
        }

        protected virtual void OnEnable()
        {
            SquareBattleCharacterRegistry.Instance.Register(this);
        }

        protected virtual void OnDisable()
        {
            SquareBattleCharacterRegistry.Instance.Unregister(this);
            HealthBarManager.Instance?.Unregister(this);
        }

        protected virtual void FixedUpdate()
        {
            HandleMove();
        }

        protected virtual void HandleMove()
        {
            if (moveType == MoveType.None) return;

            // 被撞加速阶段：以 hitBoost 速度弹飞
            if (hitBoostTimer > 0f)
            {
                hitBoostTimer -= Time.fixedDeltaTime;
                rb.velocity = moveDir * (moveSpeed * hitBoostMultiplier * _hitSpeedMultiplier);
                return;
            }

            // 持续匀速移动
            rb.velocity = moveDir * moveSpeed;
        }

        private void ApplySpawnData(CharacterSpawnData data)
        {
            if (data == null) return;

            if (data.camp.HasValue)
                camp = data.camp.Value;

            if (data.maxHp > 0)
            {
                maxHp     = data.maxHp;
                currentHp = maxHp;
            }

            if (data.moveSpeed > 0f)
                moveSpeed = data.moveSpeed;

            if (data.randomInitDir.HasValue)
            {
                // 重新应用随机方向逻辑
                if (data.randomInitDir.Value && moveType != MoveType.None)
                    moveDir = Random.insideUnitCircle.normalized;
            }
        }

        protected virtual void OnDestroy()
        {
        }

        /// <summary>
        /// 获取角色的碰撞半径。
        /// 优先读 CircleCollider2D，其次读 CapsuleCollider2D 短边，没有则返回 0。
        /// </summary>
        public float GetRadius()
        {
            var circle = GetComponent<CircleCollider2D>();
            if (circle != null)
                return circle.radius * Mathf.Max(Mathf.Abs(transform.lossyScale.x),
                                                  Mathf.Abs(transform.lossyScale.y));

            var capsule = GetComponent<CapsuleCollider2D>();
            if (capsule != null)
                return Mathf.Min(capsule.size.x, capsule.size.y) * 0.5f
                       * Mathf.Max(Mathf.Abs(transform.lossyScale.x),
                                   Mathf.Abs(transform.lossyScale.y));

            return 0f;
        }

        /// <summary>
        /// 碰撞墙壁反弹：对移动方向做镜面反射
        /// </summary>
        public virtual void OnBounce(Vector2 normal)
        {
            moveDir = Vector2.Reflect(moveDir, normal).normalized;
        }

        /// <summary>
        /// 被角色撞击时的弹飞：沿法线方向弹出并短暂加速
        /// 弹飞期间免疫再次被撞
        /// </summary>
        public virtual void OnHitByCharacter(Vector2 normal, float speedMultiplier = 1f)
        {
            // 弹飞期间免疫，不重置方向
            if (hitBoostTimer > 0f) return;

            moveDir = normal.normalized;
            hitBoostTimer = hitBoostDuration;
            _hitSpeedMultiplier = speedMultiplier;
        }

        public virtual void OnCollisionEnter2D(Collision2D collision)
        {
            if (isDead) return;
            if (!_activeContacts.Add(collision.collider)) return; // 已在接触中，跳过

            CharacterBase other = collision.collider.GetComponent<CharacterBase>();

            if (other == null)
            {
                // 撞墙：自己处理反射
                if (!collision.gameObject.CompareTag("Wall")) return;

                ContactPoint2D contact = collision.contacts[0];
                OnBounce(contact.normal);
                return;
            }

            // 角色间碰撞：只让 InstanceID 较大的一方来处理，避免双方各自处理导致方向抵消
            if (GetInstanceID() < other.GetInstanceID()) return;

            ContactPoint2D c = collision.contacts[0];
            if (other.camp != this.camp)
            {
                OnBounce(c.normal);
                other.OnBounce(-c.normal);
            }
            else
            {
                // 同阵营：双方都反射
                OnBounce(c.normal);
                other.OnBounce(-c.normal);
            }
        }
        
        public virtual void OnCollisionExit2D(Collision2D collision)
        {
            _activeContacts.Remove(collision.collider);
        }

        public virtual void TakeDamage(int damage, CharacterBase attacker)
        {
            if (isDead) return;

            currentHp -= damage;
            currentHp = Mathf.Max(currentHp, 0);

            Debug.Log($"{name} 受到 {damage} 伤害，剩余HP: {currentHp}");

            OnDamaged(damage, attacker);
            HealthBarManager.Instance?.UpdateHealth(this, currentHp, maxHp);

            if (currentHp <= 0)
            {
                Die(attacker);
            }
        }

        protected virtual void OnDamaged(int damage, CharacterBase attacker)
        {
            // 👉 受伤反馈（闪红、抖动等）
            if (spriteRenderer != null)
            {
                spriteRenderer.color = Color.red;
                Invoke(nameof(ResetColor), 0.1f);
            }
        }

        private void ResetColor()
        {
            if (spriteRenderer != null)
                spriteRenderer.color = Color.white;
        }

        protected virtual void Die(CharacterBase killer)
        {
            if (isDead) return;

            isDead = true;

            Debug.Log($"{name} 被 {killer.name} 击杀");

            OnDie(killer);

            Destroy(gameObject, 0.1f);
        }

        protected virtual void OnDie(CharacterBase killer)
        {
            // 👉 死亡效果（爆炸 / 粒子）
        }

        public Camp GetCamp()
        {
            return camp;
        }

        public void SetCamp(Camp newCamp)
        {
            camp = newCamp;
        }

        public bool IsDead()
        {
            return isDead;
        }

        public Vector2 GetMoveDir() => moveDir;

        public void SetMoveDir(Vector2 dir) => moveDir = dir.normalized;
    }
}