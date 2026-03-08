using System;
using Tools.GameObjectPools;
using UnityEngine;

namespace Rouge.BattleCore.Bullets
{
    public class BaseBullet : MonoBehaviour
    {
        [SerializeField] private Rigidbody2D rb;
        private BulletContext ctx;
        
        public virtual void Init(BulletContext ctx)
        {
            this.ctx = ctx;
        }

        private void Update()
        {
            
        }

        protected virtual void OnShoot(Vector2 dir)
        {
            rb.velocity = dir.normalized * ctx.bulletSpeed;
        }

        protected virtual void OnCollision()
        {
            OnBulletDestroy();
        }

        protected virtual void OnBulletDestroy()
        {
            rb.velocity = Vector2.zero;
            GameObjectPool.Instance.RemoveGameObject(gameObject);
        }

        private void OnTriggerEnter(Collider other)
        {
            
        }
    }
}