
using Battle.Inputs;

namespace Battle.Character.Ability.A2D
{
    [Ability(AbilityEnum.HorizontalMove2D)]
    public class HorizontalMovement2D : Ability2D
    {
        private float moveSpeed = 6f;

        private float currentSpeed;
        private bool canMove = true;

        public HorizontalMovement2D()
        {
            OwnTag = AbilityTag.Movement;
        }

        public override void BindInput(InputAction input)
        {
            
        }

        public override void UnbindInput(InputAction input)
        {
            
        }

        protected override void OnTickUpdate(float deltaTime)
        {
            if (!canMove) return;
        }

        private void Move(float horizontal)
        {
            var speed = horizontal * moveSpeed;
            character2D.motor.SetVelocityX(speed);
        }
    }
}