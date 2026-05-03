using Tools.GameObjectPools;
using UnityEngine;

namespace SquareBattle.BattleCore.Characters
{
    public class Archer : CharacterBase
    {
        [Header("Attack")]
        [SerializeField] private GameObject arrowPrefab;
        [SerializeField] private float attackInterval = 1.5f;
        [SerializeField] private float attackRange = 10f;

        [Header("Multi Shot")]
        [Tooltip("每次发射的子弹数量（1 = 单发）")]
        [SerializeField] private int bulletCount = 1;

        [Tooltip("相邻两发子弹的间隔角度（度），bulletCount=1 时无效")]
        [SerializeField] private float angleStep = 15f;

        private float attackTimer;

        protected override void Start()
        {
            base.Start();
            if (arrowPrefab != null)
                GameObjectPool.Instance.Register(arrowPrefab);
        }

        protected override void Update()
        {
            base.Update();

            attackTimer += Time.deltaTime;

            CharacterBase target = FindNearestEnemy();
            if (target == null) return;

            // 朝向最近敌人（左右翻转）
            Vector2 dir = (target.transform.position - transform.position).normalized;
            Vector3 scale = transform.localScale;
            scale.x = dir.x > 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;

            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist > attackRange) return;

            if (attackTimer >= attackInterval)
            {
                Shoot(dir);
                attackTimer = 0f;
            }
        }

        private CharacterBase FindNearestEnemy()
        {
            float minDist = float.MaxValue;
            CharacterBase nearest = null;

            foreach (var c in SquareBattleCharacterRegistry.Instance.AllCharacters)
            {
                if (c == this) continue;
                if (c.GetCamp() == this.camp) continue;

                float dist = Vector2.Distance(transform.position, c.transform.position);
                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = c;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 以 dir 为中心方向，展开 bulletCount 发子弹
        /// 多发时子弹对称分布在中心方向两侧
        /// </summary>
        private void Shoot(Vector2 dir)
        {
            // 总展开角 = (bulletCount - 1) * angleStep
            // 左起始角偏移 = -总展开角 / 2
            float totalSpread = (bulletCount - 1) * angleStep;
            float startAngle  = -totalSpread * 0.5f;

            for (int i = 0; i < bulletCount; i++)
            {
                float angle    = startAngle + i * angleStep;
                Vector2 rotDir = RotateVector(dir, angle);
                SpawnArrow(rotDir);
            }
        }

        private void SpawnArrow(Vector2 dir)
        {
            GameObject arrowObj = GameObjectPool.Instance.CreateGameObject(arrowPrefab.name);
            arrowObj.transform.position = transform.position;
            Projectile projectile = arrowObj.GetComponent<Projectile>();
            projectile.Init(dir, camp, this);
        }

        /// <summary>将向量按指定角度（度）旋转</summary>
        private static Vector2 RotateVector(Vector2 v, float degrees)
        {
            float rad = degrees * Mathf.Deg2Rad;
            float cos = Mathf.Cos(rad);
            float sin = Mathf.Sin(rad);
            return new Vector2(v.x * cos - v.y * sin, v.x * sin + v.y * cos);
        }
    }
}
