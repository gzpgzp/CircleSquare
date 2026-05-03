using Tools.GameObjectPools;
using UnityEngine;

namespace SquareBattle.BattleCore.Characters
{
    /// <summary>
    /// DogDun —— 持盾投刀角色
    /// - 一面盾牌围绕自身持续旋转，碰撞体 Tag 为 "Wall"，可弹开敌方角色
    /// - 每隔一定时间朝最近敌人投出飞刀
    /// </summary>
    public class DogDun : CharacterBase
    {
        [Header("Shield")]
        [Tooltip("盾牌预制体（需挂 Shield.cs + Collider2D，Tag = Wall）")]
        [SerializeField] private GameObject shieldPrefab;

        [Tooltip("盾牌旋转速度（度/秒）")]
        [SerializeField] private float shieldRotateSpeed = 180f;

        [Header("Knife")]
        [Tooltip("飞刀预制体（需挂 Projectile.cs + Collider2D，Is Trigger = true）")]
        [SerializeField] private GameObject knifePrefab;

        [Tooltip("投刀间隔（秒）")]
        [SerializeField] private float throwInterval = 2f;

        [Tooltip("投刀射程，超出范围不投")]
        [SerializeField] private float throwRange = 15f;

        private Shield _shield;
        private float _throwTimer;

        protected override void Start()
        {
            base.Start();

            // 生成盾牌并初始化
            if (shieldPrefab != null)
            {
                GameObject shieldObj = Instantiate(shieldPrefab, transform.position, Quaternion.identity);
                _shield = shieldObj.GetComponent<Shield>();
                if (_shield == null)
                {
                    Debug.LogError($"[DogDun] shieldPrefab '{shieldPrefab.name}' 上缺少 Shield 组件！请在预制体 Inspector 中添加。");
                    Destroy(shieldObj);
                }
                else
                {
                    _shield.Init(transform, camp, 0f, shieldRotateSpeed);
                }
            }

            _throwTimer = 0f;

            // 注册飞刀预制体到对象池
            if (knifePrefab != null)
                GameObjectPool.Instance.Register(knifePrefab);
        }

        protected override void Update()
        {
            base.Update();

            _throwTimer += Time.deltaTime;
            if (_throwTimer >= throwInterval)
            {
                TryThrowKnife();
                _throwTimer = 0f;
            }
        }

        protected override void OnDestroy()
        {
            base.OnDestroy();
            // 主人死亡时销毁盾牌
            if (_shield != null)
                Destroy(_shield.gameObject);
        }

        /// <summary>
        /// 寻找最近的不同阵营角色
        /// </summary>
        private CharacterBase FindNearestEnemy()
        {
            float minDist = float.MaxValue;
            CharacterBase nearest = null;

            foreach (var c in SquareBattleCharacterRegistry.Instance.AllCharacters)
            {
                if (c == this) continue;
                if (c.GetCamp() == camp) continue;
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

        /// <summary>
        /// 朝最近敌人投出飞刀
        /// </summary>
        private void TryThrowKnife()
        {
            if (knifePrefab == null) return;

            CharacterBase target = FindNearestEnemy();
            if (target == null) return;

            float dist = Vector2.Distance(transform.position, target.transform.position);
            if (dist > throwRange) return;

            Vector2 dir = ((Vector2)(target.transform.position - transform.position)).normalized;

            // 朝向目标（左右翻转）
            Vector3 scale = transform.localScale;
            scale.x = dir.x > 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;

            GameObject knifeObj = GameObjectPool.Instance.CreateGameObject(knifePrefab.name);
            knifeObj.transform.position = transform.position;
            knifeObj.transform.rotation = Quaternion.identity;
            Projectile projectile = knifeObj.GetComponent<Projectile>();
            projectile.Init(dir, camp, this);
        }
    }
}
