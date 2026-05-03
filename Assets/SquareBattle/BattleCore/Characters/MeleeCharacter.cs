using System.Collections;
using Tools.ResourcesTool;
using UnityEngine;

namespace SquareBattle.BattleCore.Characters
{
    /// <summary>
    /// MeleeCharacter —— 近战英雄模板
    /// 
    /// 核心行为：
    ///   - 持续寻找最近敌人并朝向翻转
    ///   - 进入 attackRange 后触发一次或多次连段攻击
    ///   - 连段完成后进入冷却（attackCooldown），冷却结束才可发起下一轮
    /// 
    /// 继承本类后可 override OnMeleeHit() 实现具体的伤害/特效逻辑，
    /// 也可 override FindTarget() 替换目标筛选策略。
    /// </summary>
    public class MeleeCharacter : CharacterBase
    {
        // ──────────────────────────── Inspector ────────────────────────────

        [Header("Melee Attack")]
        [Tooltip("触发攻击的范围（与目标的距离）")]
        [SerializeField] protected float attackRange = 2.5f;

        [Tooltip("每次单段攻击造成的伤害")]
        [SerializeField] protected int attackDamage = 20;

        [Tooltip("连段攻击次数（1 = 单次，>1 = 连段）")]
        [SerializeField] protected int comboCount = 1;

        [Tooltip("连段中每次攻击之间的间隔（秒）")]
        [SerializeField] protected float comboInterval = 0.3f;

        [Tooltip("完成一轮连段后的冷却时间（秒）")]
        [SerializeField] protected float attackCooldown = 1.5f;

        [Tooltip("攻击瞬间打击判定的半径（通常略大于自身碰撞体）")]
        [SerializeField] protected float hitRadius = 0.6f;

        [Header("Critical Hit")]
        [Tooltip("暴击发生的概率（0.0-1.0）")]
        [SerializeField] protected float criticalChance = 0.2f;

        [Tooltip("暴击伤害倍率")]
        [SerializeField] protected float criticalMultiplier = 2.0f;

        protected const string hitEffectKey = "Prefabs/SquareBattle/SwordHitBlue";
        protected const string criticalHitEffectKey = "Prefabs/SquareBattle/SwordHitBlueCritical";

        // ──────────────────────────── Private State ────────────────────────

        private float _cooldownTimer;     // 冷却剩余时间
        private bool _isAttacking;        // 正在执行连段中
        private CharacterBase _target;    // 本轮攻击目标（连段期间锁定）

        // ──────────────────────────── Lifecycle ────────────────────────────

        protected override void Start()
        {
            base.Start();
            _cooldownTimer = 0f;
        }

        protected override void Update()
        {
            base.Update();

            // 冷却倒计时
            if (_cooldownTimer > 0f)
            {
                _cooldownTimer -= Time.deltaTime;
                return;
            }

            // 连段进行中，不重复触发
            if (_isAttacking) return;

            // 寻找目标
            CharacterBase target = FindTarget();
            if (target == null) return;

            // 朝向目标
            FaceTarget(target);

            // 在攻击范围内 → 发起一轮连段
            // 使用边缘间距（中心距 - 自身半径 - 目标半径），避免仅用中心点导致大角色进不了范围
            float dist = Vector2.Distance(transform.position, target.transform.position)
                         - GetRadius() - target.GetRadius();
            if (dist <= attackRange)
            {
                _target = target;
                StartCoroutine(ComboRoutine());
            }
        }

        // ──────────────────────────── Core Logic ───────────────────────────

        /// <summary>
        /// 连段协程：按 comboCount 依次执行每次攻击，间隔 comboInterval
        /// </summary>
        private IEnumerator ComboRoutine()
        {
            _isAttacking = true;

            for (int i = 0; i < comboCount; i++)
            {
                // 目标死亡或超出范围则中断连段
                if (_target == null || _target.IsDead())
                    break;

                float dist = Vector2.Distance(transform.position, _target.transform.position)
                             - GetRadius() - _target.GetRadius();
                if (dist > attackRange)
                    break;

                // 执行第 i+1 段攻击
                PerformHit(i);

                if (i < comboCount - 1)
                    yield return new WaitForSeconds(comboInterval);
            }

            _isAttacking = false;
            _cooldownTimer = attackCooldown;
        }

        /// <summary>
        /// 执行单次打击：以自身与目标的中点为圆心做范围检测
        /// </summary>
        /// <param name="comboIndex">连段序号（0-based），子类可据此区分段数做不同表现</param>
        protected virtual void PerformHit(int comboIndex)
        {
            if (isDead) return;
            if (_target == null) return;

            // 检测原点：自身与目标的中点（比自身中心更接近实际接触区域）
            Vector2 hitOrigin = ((Vector2)transform.position + (Vector2)_target.transform.position) * 0.5f;

            Collider2D[] hits = Physics2D.OverlapCircleAll(hitOrigin, hitRadius);
            foreach (var col in hits)
            {
                CharacterBase other = col.GetComponent<CharacterBase>();
                if (other == null) continue;
                if (other == this) continue;
                if (other.GetCamp() == this.camp) continue;
                if (other.IsDead()) continue;

                // 命中点：取自身边缘与目标边缘的中间，供特效生成使用
                Vector2 selfEdge   = (Vector2)transform.position
                                     + (hitOrigin - (Vector2)transform.position).normalized * GetRadius();
                Vector2 otherEdge  = (Vector2)other.transform.position
                                     + ((Vector2)transform.position - (Vector2)other.transform.position).normalized * other.GetRadius();
                Vector2 hitPoint   = (selfEdge + otherEdge) * 0.5f;

                // 暴击判定
                bool isCritical = Random.value < criticalChance;
                OnMeleeHit(other, comboIndex, hitPoint, isCritical);
            }
        }

        /// <summary>
        /// 近战命中回调（可 override 实现特殊伤害/特效/击退等）
        /// 默认直接调用 TakeDamage
        /// </summary>
        /// <param name="target">被命中的角色</param>
        /// <param name="comboIndex">当前连段序号</param>
        /// <param name="hitPoint">命中点世界坐标（可用于生成特效、飘字等）</param>
        /// <param name="isCritical">是否为暴击</param>
        protected virtual void OnMeleeHit(CharacterBase target, int comboIndex, Vector2 hitPoint, bool isCritical = false)
        {
            // 计算伤害
            int damage = isCritical ? (int)(attackDamage * criticalMultiplier) : attackDamage;
            target.TakeDamage(damage, this);

            // 选择特效
            string effectKey = isCritical ? criticalHitEffectKey : hitEffectKey;
            
            // 命中特效：在 hitPoint 位置生成预制体
            if (!string.IsNullOrEmpty(effectKey))
            {
                var fx = MyResourcesManager.Instance.LoadAndInstantiate(effectKey);
                if (fx != null)
                    fx.transform.position = hitPoint;
            }

            // 暴击视觉反馈
            if (isCritical)
            {
                // 显示暴击文字
                ShowCriticalText(hitPoint);
                
                // 播放暴击音效（如果存在）
                PlayCriticalSound();
            }
        }

        /// <summary>
        /// 显示暴击文字效果
        /// </summary>
        /// <param name="position">显示位置</param>
        protected virtual void ShowCriticalText(Vector2 position)
        {
            // 简单的暴击文字显示逻辑
            Debug.Log("CRITICAL HIT!");
            
            // 这里可以集成TextMeshPro飘字系统
            // 由于项目中未发现现成的飘字系统，先使用Debug.Log作为占位
            // 实际项目中可以替换为TextMeshPro实例化
        }

        /// <summary>
        /// 播放暴击音效
        /// </summary>
        protected virtual void PlayCriticalSound()
        {
            // 这里可以集成音效播放逻辑
            // 由于项目中未发现现成的音效系统，先使用空实现
            // 实际项目中可以替换为AudioSource.PlayOneShot
        }

        /// <summary>
        /// 寻找最近敌方角色（可 override 替换为其他目标策略）
        /// </summary>
        protected virtual CharacterBase FindTarget()
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

        /// <summary>
        /// 朝向目标：按 Sprite 默认面左的规则翻转 X 轴缩放
        /// </summary>
        protected virtual void FaceTarget(CharacterBase target)
        {
            Vector2 dir = (target.transform.position - transform.position).normalized;
            Vector3 scale = transform.localScale;
            // Sprite 默认朝左：dir.x > 0（目标在右）→ 翻转为负（面右）
            scale.x = dir.x > 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            transform.localScale = scale;
        }

        // ──────────────────────────── Gizmos ───────────────────────────────

#if UNITY_EDITOR
        protected virtual void OnDrawGizmosSelected()
        {
            // 攻击范围（蓝色）
            Gizmos.color = new Color(0.2f, 0.6f, 1f, 0.4f);
            Gizmos.DrawWireSphere(transform.position, attackRange);

            // 打击判定半径（红色）
            Gizmos.color = new Color(1f, 0.2f, 0.2f, 0.5f);
            Gizmos.DrawWireSphere(transform.position, hitRadius);
        }
#endif
    }
}
