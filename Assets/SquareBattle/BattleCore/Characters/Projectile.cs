using Tools.GameObjectPools;
using UnityEngine;

namespace SquareBattle.BattleCore.Characters
{
    /// <summary>
    /// 通用投射物脚本，Arrow 和 Knife 统一使用此脚本。
    /// 通过 Inspector 参数区分行为：
    /// - spinSpeed > 0：飞行中自转（飞刀效果）
    /// - destroyOnWall：命中墙壁是否归还对象池
    /// - faceDirection：初始化时是否朝向飞行方向
    /// </summary>
    public class Projectile : MonoBehaviour, IPooledObject
    {
        [SerializeField] private float speed = 10f;
        [SerializeField] private float lifeTime = 5f;
        [SerializeField] private int damage = 20;

        [Tooltip("飞行中每秒自转角速度，0 = 不自转（箭矢），>0 = 旋转（飞刀）")]
        [SerializeField] private float spinSpeed = 0f;

        [Tooltip("命中墙壁时是否归还对象池")]
        [SerializeField] private bool destroyOnWall = false;

        [Tooltip("初始化时是否将自身旋转朝向飞行方向")]
        [SerializeField] private bool faceDirection = false;

        [Tooltip("faceDirection 开启时在飞行方向角度基础上额外叠加的 Z 轴偏移角（度）")]
        [SerializeField] private float faceDirectionOffset = 0f;

        private Vector2 _dir;
        private Camp _ownerCamp;
        private CharacterBase _owner;
        private bool _isActive;
        private float _lifeTimer;

        public Camp OwnerCamp => _ownerCamp;

        public void Init(Vector2 direction, Camp camp, CharacterBase shooter)
        {
            _dir = direction.normalized;
            _ownerCamp = camp;
            _owner = shooter;
            _isActive = true;
            _lifeTimer = lifeTime;

            if (faceDirection)
            {
                float angle = Mathf.Atan2(_dir.y, _dir.x) * Mathf.Rad2Deg + faceDirectionOffset;
                transform.rotation = Quaternion.Euler(0f, 0f, angle + 90f);
            }
            else
            {
                // 默认 Z 轴旋转 90°
                transform.rotation = Quaternion.Euler(0f, 0f, 90f);
            }

        }

        private void ReturnToPool()
        {
            if (!_isActive) return;
            _isActive = false;
            GameObjectPool.Instance.RemoveGameObject(gameObject);
        }

        /// <summary>
        /// 外部强制归还对象池（如被 Shield 拦截时调用）
        /// </summary>
        public void ForceReturn()
        {
            ReturnToPool();
        }

        private void Update()
        {
            if (!_isActive) return;

            _lifeTimer -= Time.deltaTime;
            if (_lifeTimer <= 0f)
            {
                ReturnToPool();
                return;
            }

            transform.position += (Vector3)(_dir * speed * Time.deltaTime);

            if (spinSpeed != 0f)
                transform.Rotate(0f, 0f, spinSpeed * Time.deltaTime);
        }

        private void OnTriggerEnter2D(Collider2D collision)
        {
            if (!_isActive) return;

            if (collision.CompareTag("Wall"))
            {
                if (destroyOnWall)
                    ReturnToPool();
                return;
            }

            CharacterBase target = collision.GetComponent<CharacterBase>();
            if (target == null) return;
            if (target.GetCamp() == _ownerCamp) return;
            if (target.IsDead()) return;

            target.TakeDamage(damage, _owner);
            ReturnToPool();
        }

        private void OnDisable()
        {
            _isActive = false;
        }

        // ---- IPooledObject 实现 ----

        /// <summary>
        /// 对象从池中复用取出时回调，重置运行时状态。
        /// 注意：不包含业务参数（方向/阵营/射手），业务参数由 Init() 负责。
        /// </summary>
        public void OnSpawned()
        {
            _isActive = false;
            _lifeTimer = 0f;
        }

        /// <summary>
        /// 对象归还池时回调。
        /// </summary>
        public void OnDespawned() { }
    }
}
