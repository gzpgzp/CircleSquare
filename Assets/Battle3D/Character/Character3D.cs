using Battle3D.Character.Motor;
using Battle3D.Combat;
using UnityEngine;

namespace Battle3D.Character
{
    /// <summary>
    /// 3D角色实现 - 继承BaseCharacter，使用CharacterMotor3D
    /// 挂载到角色Prefab上，需要搭配 CharacterController 组件
    /// </summary>
    [RequireComponent(typeof(CharacterMotor3D))]
    public class Character3D : BaseCharacter
    {
        private CharacterMotor3D motor3D;

        [Header("动画")]
        [SerializeField] private Animator animator;

        public Animator AnimatorComponent => animator;

        protected override void InitMotor()
        {
            motor3D = GetComponent<CharacterMotor3D>();
            motor3D.Init(transform);
            motor = motor3D;
        }

        public override void UpdateTick(float dt)
        {
            base.UpdateTick(dt);
            UpdateAnimator(dt);
        }

        public override void Tick(float dt)
        {
            base.Tick(dt);
        }

        /// <summary>
        /// 更新动画参数（子类或具体角色可重写）
        /// </summary>
        protected virtual void UpdateAnimator(float dt)
        {
            if (animator == null) return;

            // 速度参数
            float speed = motor != null ? motor.Velocity.magnitude : 0f;
            animator.SetFloat("MoveSpeed", speed, 0.1f, dt);

            // 是否在地面
            animator.SetBool("IsGrounded", motor?.IsGrounded ?? true);

            // 是否存活
            animator.SetBool("IsAlive", IsAlive);
        }

        protected override void OnHit(DamageInfo damageInfo, float actualDamage)
        {
            base.OnHit(damageInfo, actualDamage);

            // 触发受击动画
            if (animator != null)
            {
                animator.SetTrigger("Hit");
            }
        }

        protected override void OnDeath(BaseCharacter killer)
        {
            base.OnDeath(killer);

            // 触发死亡动画
            if (animator != null)
            {
                animator.SetTrigger("Death");
                animator.SetBool("IsAlive", false);
            }
        }

        /// <summary>
        /// 设置Animator引用（在Inspector中或通过代码）
        /// </summary>
        public void SetAnimator(Animator anim)
        {
            animator = anim;
        }
    }
}
