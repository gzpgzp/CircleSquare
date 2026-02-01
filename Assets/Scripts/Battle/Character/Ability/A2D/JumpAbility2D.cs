using Battle.Inputs;
using UnityEngine;

namespace Battle.Character.Ability.A2D
{
    [Ability(AbilityEnum.Jump)]
    public class JumpAbility2D : Ability2D
    {
        private float jumpForce = 8f;
        private float jumpHoldTime = 0.5f;
        private float fallMultiplier = 2.5f;
        private float lowJumpMultiplier = 2f;

        private bool isJumpPressed = false;
        private bool isJumping = false;
        private float jumpTimeCounter;

        private void Jump()
        {
            isJumpPressed = true;
        }

        private void KeepJump()
        {
            //isJumping = true;
        }

        protected override void OnTickUpdate(float dt)
        {
            // 起跳
            if (motor.IsGrounded && isJumpPressed)
            {
                isJumping = true;
                jumpTimeCounter = jumpHoldTime;
                motor.SetVelocityY(jumpForce); // 起跳瞬间
            }

            // 按住跳跃延长跳跃
            if (isJumping)
            {
                jumpTimeCounter -= dt;

                // 如果仍在允许跳跃时间且Y速度小于跳跃初速度，则保持
                if (jumpTimeCounter > 0 && motor.velocity.y < jumpForce)
                {
                    motor.SetVelocityY(jumpForce); // 保持原跳跃动力
                }
                else
                {
                    isJumping = false;
                }
            }

            // 下落加速（Gravity增强）
            Vector2 vel = motor.velocity;
            if (vel.y < 0)
            {
                motor.AddVelocity(Vector2.up * Physics2D.gravity.y * (fallMultiplier - 1) * dt);
            }
            else if (vel.y > 0 && !Input.GetButton("Jump"))
            {
                motor.AddVelocity(Vector2.up * Physics2D.gravity.y * (lowJumpMultiplier - 1) * dt);
            }

            isJumpPressed = false;
        }

        public override void BindInput(InputAction input, BaseInput playerInput)
        {
            base.BindInput(input, playerInput);
            input.RegisterDown(Jump);
            input.RegisterHold(KeepJump);
        }

        public override void UnbindInput(InputAction input)
        {
            input.RemoveDown(Jump);
            input.RemoveHold(KeepJump);
        }
    }
}