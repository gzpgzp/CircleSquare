using UnityEngine;

namespace Adventure.Character
{
    /// <summary>
    /// 移动状态 - 角色正在移动到目标点
    /// 状态切换由 CharacterStateMachine 监听 mover 的移动状态自动处理
    /// </summary>
    public class MovingState : CharacterState
    {
        public MovingState()
        {
            StateType = CharacterStateType.Moving;
        }

        public override void Enter()
        {
            base.Enter();
            // 可以在这里添加移动开始时的逻辑，如播放移动音效
        }

        public override void Execute()
        {
            // 移动逻辑由 CharacterMover 处理
            // 状态切换由 CharacterStateMachine 监听 onMovingStateChanged 自动处理
        }

        public override void Exit()
        {
            base.Exit();
            // 可以在这里添加移动结束时的逻辑
        }
    }
}
