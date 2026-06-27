using Battle.Inputs;
using UnityEngine;
using UnityTimer;

namespace Battle.Character.Ability.A2D
{
    [Ability(AbilityEnum.Dash)]
    public class DashAbility2D : Ability2D
    {
        [Header("基础设置")]
        public float dashSpeed = 20f; // 冲刺速度
        public float dashDuration = 0.15f; // 冲刺持续时间

        [Header("输入设置")] public KeyCode dashKey = KeyCode.LeftShift; // 如果不用右键，可以用键盘

        private bool isDashing = false;
        private Timer dashTimer;

        private Vector3 dir = Vector3.zero;
        
        public DashAbility2D()
        {
            OwnTag = AbilityTag.Action;
        }

        private void Dash()
        {
            // 冷却计时
            if (abilityTimer > 0f)
                abilityTimer -= Time.deltaTime;

            dir = ctx.DirectionInput(character2D.transform.position);
            
            if (!isDashing && abilityTimer <= 0f)
            {
                if (dir.sqrMagnitude > 0.001f)
                {
                    dir.Normalize();
                    StartDash(dir);
                }
            }
        }

        private void StartDash(Vector2 direction)
        {
            if (isDashing)
                return;

            isDashing = true;
            abilityTimer = coolDown;

            // 记录原本状态
            float originalGravity = character2D.motor.gravity;
            Vector2 originVelocity = character2D.motor.velocity;

            // 冲刺前处理
            character2D.motor.SetGravityScale(0);
            character2D.motor.SetVelocity(Vector2.zero);

            // 启动 Timer
            dashTimer = Timer.Register(
                dashDuration,

                () =>
                {
                    // 冲刺结束
                    character2D.motor.SetGravityScale(originalGravity);
                    character2D.motor.SetVelocity(
                        character2D.motor.velocity.normalized * originVelocity.magnitude
                    );

                    isDashing = false;
                },

                (elapsed) =>
                {
                    // 冲刺期间每帧保持速度
                    character2D.motor.SetVelocity(direction * dashSpeed);
                }
            );
        }

        public override void BindInput(InputAction input)
        {
            input.RegisterDown(Dash);
        }

        public override void UnbindInput(InputAction input)
        {
            input.RemoveDown(Dash);
        }
    }
}