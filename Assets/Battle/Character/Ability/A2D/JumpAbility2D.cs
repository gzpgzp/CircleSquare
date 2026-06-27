using Battle.Inputs;
using UnityEngine;

namespace Battle.Character.Ability.A2D
{
    [Ability(AbilityEnum.Jump)]
    public class JumpAbility2D : Ability2D
    {
        private float jumpForce = 8f;
        
        private void Jump()
        {
            if (!character2D.motor.IsGrounded)
            {
                return;
            }

            if (character2D.motor.velocity.y < 0)
            {
                character2D.motor.SetVelocity(new Vector2(character2D.motor.velocity.x, 0));
            }

            character2D.motor.AddForce(Vector2.up * jumpForce);
        }

        public override void BindInput(InputAction input)
        {
            input.RegisterDown(Jump);
        }

        public override void UnbindInput(InputAction input)
        {
            input.RemoveDown(Jump);
        }
    }
}