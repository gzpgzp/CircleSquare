using System.Collections.Generic;
using UnityEngine;

namespace Battle3D.AI
{
    /// <summary>
    /// AI状态枚举
    /// </summary>
    public enum AIStateEnum
    {
        Idle,
        Patrol,
        Chase,
        Attack,
        Retreat,
        Stun,
        Dead,
    }

    /// <summary>
    /// 状态机 - 管理AI状态的切换和更新
    /// 参考现有项目 StateMachine 设计并增强
    /// </summary>
    public class StateMachine
    {
        private BaseState currentState;
        private Dictionary<AIStateEnum, BaseState> states = new Dictionary<AIStateEnum, BaseState>();
        private AIStateEnum previousStateId = AIStateEnum.Idle;

        public BaseState CurrentState => currentState;
        public AIStateEnum CurrentStateId => currentState?.StateId ?? AIStateEnum.Idle;
        public AIStateEnum PreviousStateId => previousStateId;

        /// <summary>
        /// 注册状态
        /// </summary>
        public void AddState(BaseState state)
        {
            states[state.StateId] = state;
        }

        /// <summary>
        /// 切换到指定状态
        /// </summary>
        public void ChangeState(AIStateEnum newStateId)
        {
            if (!states.TryGetValue(newStateId, out var nextState))
            {
                Debug.LogWarning($"[StateMachine] State {newStateId} not found!");
                return;
            }

            if (currentState?.StateId == newStateId) return;

            previousStateId = currentState?.StateId ?? AIStateEnum.Idle;
            currentState?.Exit();
            currentState = nextState;
            currentState.Enter();
        }

        /// <summary>
        /// 切换到上一个状态
        /// </summary>
        public void ChangeToPreviousState()
        {
            ChangeState(previousStateId);
        }

        /// <summary>
        /// 更新当前状态
        /// </summary>
        public void Update(float deltaTime)
        {
            currentState?.Update(deltaTime);
        }

        /// <summary>
        /// 物理更新
        /// </summary>
        public void FixedUpdate(float fixedDeltaTime)
        {
            currentState?.FixedUpdate(fixedDeltaTime);
        }

        /// <summary>
        /// 强制进入初始状态
        /// </summary>
        public void SetInitialState(AIStateEnum stateId)
        {
            if (states.TryGetValue(stateId, out var state))
            {
                currentState = state;
                currentState.Enter();
            }
        }

        /// <summary>
        /// 检查是否有指定状态
        /// </summary>
        public bool HasState(AIStateEnum stateId)
        {
            return states.ContainsKey(stateId);
        }
    }
}
