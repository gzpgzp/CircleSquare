using Battle3D.Character.Ability;
using Battle3D.Core;
using UnityEngine;

namespace Battle3D.Character.Animation
{
    /// <summary>
    /// 角色动画控制器 - 统一管理层动画参数和事件
    /// 桥接 AbilitySystem 和 Animator，处理动画事件回调
    /// 当前为简化版：仅支持移动/攻击/受击/死亡
    /// </summary>
    [RequireComponent(typeof(Animator))]
    public class CharacterAnimator : MonoBehaviour
    {
        private Animator animator;
        private BaseCharacter owner;

        // Animator 参数名常量
        public static readonly int AnimMoveSpeed = Animator.StringToHash("MoveSpeed");
        public static readonly int AnimIsGrounded = Animator.StringToHash("IsGrounded");
        public static readonly int AnimIsAlive = Animator.StringToHash("IsAlive");
        public static readonly int AnimAttack = Animator.StringToHash("Attack");
        public static readonly int AnimHit = Animator.StringToHash("Hit");
        public static readonly int AnimDeath = Animator.StringToHash("Death");

        private void Awake()
        {
            animator = GetComponent<Animator>();
        }

        /// <summary>
        /// 初始化
        /// </summary>
        public void Init(BaseCharacter character)
        {
            owner = character;
        }

        private void Update()
        {
            UpdateBaseParameters();
        }

        /// <summary>
        /// 更新基础动画参数（速度、地面状态等）
        /// </summary>
        private void UpdateBaseParameters()
        {
            if (animator == null || owner == null) return;

            float speed = owner.Motor?.Velocity.magnitude ?? 0f;
            bool grounded = owner.Motor?.IsGrounded ?? true;
            bool alive = owner.IsAlive;

            animator.SetFloat(AnimMoveSpeed, speed, 0.1f, Time.deltaTime);
            animator.SetBool(AnimIsGrounded, grounded);
            animator.SetBool(AnimIsAlive, alive);
        }

        #region 动画触发方法

        /// <summary>触发攻击动画</summary>
        public void TriggerAttack(int comboIndex = 0)
        {
            animator?.SetTrigger(AnimAttack);
        }

        /// <summary>触发受击动画</summary>
        public void TriggerHit()
        {
            animator?.SetTrigger(AnimHit);
        }

        /// <summary>触发死亡动画</summary>
        public void TriggerDeath()
        {
            if (animator == null) return;
            animator.SetTrigger(AnimDeath);
            animator.SetBool(AnimIsAlive, false);
        }

        #endregion

        #region Animation Event 回调（由动画帧上的事件调用）

        /// <summary>
        /// 动画事件：攻击判定帧
        /// 在 Animator 中的攻击动画帧上添加 AnimationEvent，调用此方法
        /// </summary>
        public void OnAttackHitFrame()
        {
            owner?.TriggerAbilityAnimationEvent("HitDetect");
        }

        /// <summary>
        /// 动画事件：脚步声（可用于音效和尘土特效）
        /// </summary>
        public void OnFootstep()
        {
            owner?.TriggerAbilityAnimationEvent("Footstep");
        }

        /// <summary>
        /// 通用动画事件
        /// </summary>
        public void OnAnimationEvent(string eventName)
        {
            owner?.TriggerAbilityAnimationEvent(eventName);
        }

        #endregion
    }
}
