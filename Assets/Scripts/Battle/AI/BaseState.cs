using System;
using Battle.Character;

namespace Battle.AI
{
    [Serializable]
    public class BaseStateContext
    {
        public StateEnum state;
    }

    public abstract class BaseState
    {
        public abstract StateEnum Id { get; }
        
        protected BaseCharacter character;
        protected StateMachine machine;

        public BaseState(BaseCharacter character,StateMachine machine)
        {
            this.character = character;
            this.machine = machine;
        }

        public virtual void SetConfig(BaseStateContext config)
        {
            
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