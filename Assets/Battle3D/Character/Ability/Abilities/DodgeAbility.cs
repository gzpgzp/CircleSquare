using Battle3D.Character.Ability;
using UnityEngine;

namespace Battle3D.Character.Ability.Abilities
{
    /// <summary>
    /// 闪避能力 - 快速位移 + 无敌帧(i-frame)
    /// 消耗能量/体力，有冷却时间
    /// 闪避期间免疫伤害（通过 Invincible 标签实现）
    /// </summary>
    [AbilityId("dodge")]
    public class DodgeAbility : BaseAbility
    {
        [Header("闪避配置")]
        private float dodgeDistance = 5f;
        private float dodgeDuration = 0.3f;
        private float iFrameStart = 0.05f;   // 无敌帧开始时间
        private float iFrameEnd = 0.25f;     // 无敌帧结束时间

        // 运行时
        private float dodgeTimer;
        private bool iFrameActive;

        public override void Init(AbilityContext context)
        {
            base.Init(context);
            Priority = 20; // 闪避优先级高于普攻
        }

        protected override void OnActivate()
        {
            base.OnActivate();

            dodgeTimer = dodgeDuration;
            iFrameActive = false;

            // 计算闪避方向（默认向移动方向，无输入则向后）
            Vector3 dodgeDir = GetDodgeDirection();

            // 调用运动系统的闪避
            owner.Motor?.Dodge(dodgeDir, dodgeDistance, dodgeDuration);

            // 闪避动画后续再加，当前不触发动画
        }

        protected override void OnTickUpdate(float dt)
        {
            base.OnTickUpdate(dt);

            if (CurrentPhase == AbilityPhase.Windup || CurrentPhase == AbilityPhase.Active)
            {
                dodgeTimer -= dt;
                float elapsed = dodgeDuration - dodgeTimer;

                // 无敌帧管理
                if (elapsed >= iFrameStart && elapsed <= iFrameEnd && !iFrameActive)
                {
                    iFrameActive = true;
                    system.AddBlock(AbilityTag.Invincible);
                }

                if (elapsed > iFrameEnd && iFrameActive)
                {
                    iFrameActive = false;
                    system.RemoveBlock(AbilityTag.Invincible);
                }
            }
        }

        protected override void OnDeactivate()
        {
            // 确保无敌帧被移除
            if (iFrameActive)
            {
                iFrameActive = false;
                system.RemoveBlock(AbilityTag.Invincible);
            }

            base.OnDeactivate();
        }

        private Vector3 GetDodgeDirection()
        {
            // 优先使用角色的面朝方向
            if (owner != null)
            {
                return owner.transform.forward;
            }
            return Vector3.forward;
        }

        public override void OnInterrupted()
        {
            // 闪避不可被打断
        }
    }
}
