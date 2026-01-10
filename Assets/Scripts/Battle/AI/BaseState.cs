using Battle.Character;

namespace Battle.AI
{
    public abstract class BaseState
    {
        protected BaseCharacter character;

        public BaseState(BaseCharacter character)
        {
            this.character = character;
        }

        public virtual void Enter()
        {
            
        }

        public virtual void Update(float deltaTime)
        {
            
        }

        public virtual void Exit()
        {
            
        }
    }
}