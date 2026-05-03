using UnityEngine;

namespace SquareBattle.BattleCore.Characters
{
    /// <summary>
    /// 围绕 DogDun 旋转的盾牌（Trigger 模式）
    /// - Collider2D 勾选 Is Trigger，不产生物理推力
    /// - 仅拦截敌方 Projectile 并将其归还对象池
    /// </summary>
    public class Shield : MonoBehaviour
    {
        [Tooltip("盾牌距离主人的轨道半径")]
        [SerializeField] private float orbitRadius = 1.2f;

        private float _angle;
        private float _rotateSpeed;
        private Transform _owner;
        private Camp _ownerCamp;

        public void Init(Transform owner, Camp ownerCamp, float startAngle, float rotateSpeed)
        {
            _owner = owner;
            _ownerCamp = ownerCamp;
            _angle = startAngle;
            _rotateSpeed = rotateSpeed;
        }

        private void Update()
        {
            if (_owner == null)
            {
                Destroy(gameObject);
                return;
            }

            _angle += _rotateSpeed * Time.deltaTime;

            float rad = _angle * Mathf.Deg2Rad;
            Vector3 offset = new Vector3(Mathf.Cos(rad), Mathf.Sin(rad), 0f) * orbitRadius;
            transform.position = _owner.position + offset;

            transform.rotation = Quaternion.Euler(0f, 0f, _angle);
        }

        public float OrbitRadius => orbitRadius;

        private void OnTriggerEnter2D(Collider2D collision)
        {
            Projectile projectile = collision.GetComponent<Projectile>();
            if (projectile == null) return;

            // 只拦截敌方子弹，放行同阵营子弹
            if (projectile.OwnerCamp == _ownerCamp) return;

            projectile.ForceReturn();
        }
    }
}
