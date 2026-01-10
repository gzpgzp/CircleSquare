using System.Collections.Generic;
using Battle.Character;

namespace Battle.AI
{
    public enum StateEnum
    {
        Idle,
    }

    public class StateMachine
    {
        private BaseCharacter character;
        private BaseState curState;
        private Dictionary<StateEnum, BaseState> states = new Dictionary<StateEnum, BaseState>();

        public void Init(BaseCharacter character)
        {
            this.character = character;
            states.Add(StateEnum.Idle,new IdleState(character));
        }

        public void Update(float deltaTime)
        {
            curState?.Update(deltaTime);
        }

        public void ChangeState(StateEnum state)
        {
            curState?.Exit();
            curState = states[state];
            curState?.Enter();
        }
    }
}