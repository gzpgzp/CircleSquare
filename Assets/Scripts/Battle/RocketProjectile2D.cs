using UnityEngine;

namespace Battle
{
    public class RocketProjectile2D : MonoBehaviour
    {
        [Header("生命周期")]
        public float lifeTime = 3f;          // 多少秒后自动爆炸（防止飞太远）

        [Header("爆炸参数")]
        public float explosionRadius = 1.5f; // 爆炸半径
        public float explosionForce = 12f;   // 爆炸冲击力（推开刚体）
        public int damage = 1;               // 伤害（先留着，后面接敌人系统）

        public LayerMask hitLayers;          // 爆炸影响哪些层（Player/Enemy/Objects等）

        private float timer;

        void Update()
        {
            timer += Time.deltaTime;
            if (timer >= lifeTime)
            {
                Explode();
            }
        }

        void OnCollisionEnter2D(Collision2D collision)
        {
            // 一旦碰到任何碰撞体就爆炸，你也可以根据 Tag 细分
            Explode();
        }

        void Explode()
        {
            // 1. 查找爆炸范围内所有目标
            Collider2D[] hits = Physics2D.OverlapCircleAll(transform.position, explosionRadius, hitLayers);

            foreach (Collider2D col in hits)
            {
                Rigidbody2D rb = col.attachedRigidbody;
                if (rb != null)
                {
                    Vector2 dir = (rb.position - (Vector2)transform.position).normalized;
                    rb.AddForce(dir * explosionForce, ForceMode2D.Impulse);
                }

                // 2. 如果有血量组件，就在这里扣血（先预留）
                // var hp = col.GetComponent<Health>();
                // if (hp != null)
                // {
                //     hp.TakeDamage(damage);
                // }
            }

            // 3. TODO：这里可以播放爆炸特效/音效
            // Instantiate(explosionVfxPrefab, transform.position, Quaternion.identity);

            Destroy(gameObject);
        }

        // Scene 视图中可视化爆炸范围
        void OnDrawGizmosSelected()
        {
            Gizmos.color = Color.red;
            Gizmos.DrawWireSphere(transform.position, explosionRadius);
        }
    }
}