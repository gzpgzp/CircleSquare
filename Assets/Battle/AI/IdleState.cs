using Battle.Character;

namespace Battle.AI
{
    public class IdleState : BaseState
    {
        public override StateEnum Id => StateEnum.Idle;
        public IdleState(BaseCharacter character,StateMachine machine) : base(character,machine)
        {
            
        }

    }
}