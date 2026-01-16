
namespace Battle.Character.Ability
{
    [Ability(AbilityEnum.HorizontalMove2D)]
    public class HorizontalMovement2D : BaseAbility
    {
        private float moveSpeed = 6f;

        private float accel;
        private float decel;

        private float currentSpeed;
        private bool canMove = true;

        public HorizontalMovement2D(BaseCharacter character) : base(character)
        {
            OwnTag = AbilityTag.Movement;
        }

        protected override void OnUpdate(float deltaTime)
        {
            if (!canMove) return;
        }

        private void Move(float horizontal)
        {
            
        }
    }
}