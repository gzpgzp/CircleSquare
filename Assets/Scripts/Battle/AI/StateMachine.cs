using System;
using System.Collections.Generic;
using Battle.Character;

namespace Battle.AI
{
    public enum StateEnum
    {
        Idle,
        Patrol,
        Attack
    }

    public class StateMachine
    {
        private BaseState curState;
        private Dictionary<StateEnum, BaseState> states = new Dictionary<StateEnum, BaseState>();

        public void AddState(BaseState state)
        {
            states.Add(state.Id, state);
        }

        public void Update(float deltaTime)
        {
            curState?.Update(deltaTime);
        }

        public void ChangeState(StateEnum state)
        {
            if (!states.TryGetValue(state, out var nextState))
                return;
            curState?.Exit();
            curState = nextState;
            curState?.Enter();
        }
    }
}