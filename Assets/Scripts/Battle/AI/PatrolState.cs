using Battle.Character;

namespace Battle.AI
{
    public class PatrolState : BaseState
    {
        public override StateEnum Id => StateEnum.Patrol;
        public PatrolState(BaseCharacter character,StateMachine machine) : base(character,machine)
        {
            
        }
    }
}