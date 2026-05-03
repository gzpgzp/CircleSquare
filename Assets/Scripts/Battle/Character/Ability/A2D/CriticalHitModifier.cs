using System;
using UnityEngine;
using Random = UnityEngine.Random;

namespace Battle.Character.Ability.A2D
{
    /// <summary>
    /// 暴击修饰器：为角色添加暴击能力
    /// </summary>
    [Ability(AbilityEnum.CriticalHit)]
    public class CriticalHitModifier : BaseAbility
    {
        // ──────────────────────── Inspector ────────────────────────

        [Header("Critical Hit")]
        [Tooltip("暴击发生的概率（0.0-1.0）")]
        [SerializeField] private float criticalChance = 0.2f;

        [Tooltip("暴击伤害倍率")]
        [SerializeField] private float criticalMultiplier = 2.0f;

        // ──────────────────────── Runtime State ────────────────────────

        private float _effectDuration = 0f;
        private bool _isActive = false;

        // ──────────────────────── Lifecycle ────────────────────────

        public override void InitContext(AbilityContext ctx)
        {
            base.InitContext(ctx);
            
            // 设置优先级
            priority = 100;
            OwnTag = AbilityTag.Action;
        }

        public override void BindCharacter(BaseCharacter owner)
        {
            base.BindCharacter(owner);
            
            // 初始化时设置为禁用状态
            SetEnabled(false);
        }

        // ──────────────────────── Core Logic ────────────────────────

        /// <summary>
        /// 是否应该暴击
        /// </summary>
        public bool ShouldCrit()
        {
            return _isActive && Random.value < criticalChance;
        }

        /// <summary>
        /// 获取暴击倍率
        /// </summary>
        public float CriticalMultiplier => criticalMultiplier;

        /// <summary>
        /// 启用暴击效果
        /// </summary>
        public void EnableEffect(float duration)
        {
            _effectDuration = duration;
            _isActive = true;
            SetEnabled(true);
        }

        /// <summary>
        /// 禁用暴击效果
        /// </summary>
        public void DisableEffect()
        {
            _effectDuration = 0f;
            _isActive = false;
            SetEnabled(false);
        }

        protected override void OnTickUpdate(float deltaTime)
        {
            if (_isActive && _effectDuration > 0f)
            {
                _effectDuration -= deltaTime;
                if (_effectDuration <= 0f)
                {
                    DisableEffect();
                }
            }
        }

        protected override void OnActive()
        {
            // 激活时可以播放特效或音效
        }

        protected override void OnUnactive()
        {
            // 非激活时可以清理资源
        }
    }
}
