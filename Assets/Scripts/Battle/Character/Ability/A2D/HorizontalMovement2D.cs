
namespace Battle.Character.Ability.A2D
{
    [Ability(AbilityEnum.HorizontalMove2D)]
    public class HorizontalMovement2D : Ability2D
    {
        private float moveSpeed = 20f;

        private float currentSpeed;

        public HorizontalMovement2D()
        {
            OwnTag = AbilityTag.Movement;
        }

        protected override void OnTickUpdate(float deltaTime)
        {
            Move(input.Horizontal * moveSpeed * deltaTime);
        }

        private void Move(float horizontal)
        {
            var speed = horizontal * moveSpeed;
            motor.SetVelocityX(speed);
        }
    }
}