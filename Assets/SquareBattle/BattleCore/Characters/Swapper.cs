using Tools.ResourcesTool;
using UnityEngine;

namespace SquareBattle.BattleCore.Characters
{
    /// <summary>
    /// Swapper —— 换位近战角色
    /// 
    /// 核心能力：
    ///   - 继承 MeleeCharacter 的近战连段攻击
    ///   - 额外能力：每隔 swapInterval 秒，寻找范围内最近的目标（可配置是否包含队友/敌人）
    ///   - 与目标交换世界坐标位置，同时互换移动方向
    ///   - 交换后进入冷却，期间不再触发
    /// </summary>
    public class Swapper : MeleeCharacter
    {
        // ──────────────────────────── Inspector ────────────────────────────

        [Header("Swap")]
        [Tooltip("触发换位的最大距离")]
        [SerializeField] private float swapRange = 6f;

        [Tooltip("换位冷却时间（秒）")]
        [SerializeField] private float swapInterval = 3f;

        [Tooltip("换位冷却随机浮动范围（秒），实际冷却 = swapInterval ± swapIntervalRandom")]
        [SerializeField] private float swapIntervalRandom = 1f;

        [Tooltip("是否可与同阵营队友换位")]
        [SerializeField] private bool swapWithAlly = true;

        [Tooltip("是否可与敌方换位")]
        [SerializeField] private bool swapWithEnemy = true;



        private const string SwapEffectKey = "Prefabs/SquareBattle/BalloonPopExplosion";

        // ──────────────────────────── Private State ────────────────────────

        private float _swapTimer;

        // ──────────────────────────── Lifecycle ────────────────────────────



        protected override void Start()
        {
            base.Start();
            // 随机初始冷却，避免多个 Swapper 同时换位
            _swapTimer = Random.Range(0f, swapInterval + swapIntervalRandom);
        }

        protected override void Update()
        {
            base.Update(); // 执行近战攻击逻辑

            _swapTimer -= Time.deltaTime;
            if (_swapTimer > 0f) return;

            CharacterBase target = FindSwapTarget();
            if (target == null) return;

            PerformSwap(target);
            _swapTimer = swapInterval + Random.Range(-swapIntervalRandom, swapIntervalRandom);
        }

        // ──────────────────────────── Core Logic ───────────────────────────

        /// <summary>
        /// 在 swapRange 内寻找最近的可换位目标
        /// </summary>
        private CharacterBase FindSwapTarget()
        {
            float minDist = float.MaxValue;
            CharacterBase nearest = null;

            foreach (var c in SquareBattleCharacterRegistry.Instance.AllCharacters)
            {
                if (c == this) continue;
                if (c.IsDead()) continue;

                bool isSameCamp = c.GetCamp() == this.camp;

                if (isSameCamp && !swapWithAlly) continue;
                if (!isSameCamp && !swapWithEnemy) continue;

                float dist = Vector2.Distance(transform.position, c.transform.position);
                if (dist > swapRange) continue;

                if (dist < minDist)
                {
                    minDist = dist;
                    nearest = c;
                }
            }

            return nearest;
        }

        /// <summary>
        /// 执行换位：交换位置 + 交换移动方向
        /// </summary>
        private void PerformSwap(CharacterBase target)
        {
            Vector3 myPos     = transform.position;
            Vector3 targetPos = target.transform.position;

            // 在交换前的两个位置播放特效
            SpawnSwapEffect(myPos);
            SpawnSwapEffect(targetPos);

            transform.position        = targetPos;
            target.transform.position = myPos;

            Vector2 myDir     = GetMoveDir();
            Vector2 targetDir = target.GetMoveDir();
            SetMoveDir(targetDir);
            target.SetMoveDir(myDir);
        }

        private void SpawnSwapEffect(Vector3 pos)
        {
            var fx = MyResourcesManager.Instance.LoadAndInstantiate(SwapEffectKey);
            if (fx != null)
                fx.transform.position = pos;
        }



        // ──────────────────────────── Gizmos ───────────────────────────────

#if UNITY_EDITOR
        protected override void OnDrawGizmosSelected()
        {
            base.OnDrawGizmosSelected(); // 绘制近战攻击/判定范围

            // 换位范围（紫色）
            Gizmos.color = new Color(0.8f, 0.2f, 1f, 0.3f);
            Gizmos.DrawWireSphere(transform.position, swapRange);
        }
#endif
    }
}