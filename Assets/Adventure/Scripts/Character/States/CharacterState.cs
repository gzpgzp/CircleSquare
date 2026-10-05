using UnityEngine;

namespace Adventure.Character
{
    /// <summary>
    /// 角色状态抽象基类
    /// 所有具体状态都需要继承此类并实现相应方法
    /// </summary>
    public abstract class CharacterState
    {
        /// <summary>状态类型</summary>
        public CharacterStateType StateType { get; protected set; }

        /// <summary>状态机引用</summary>
        protected CharacterStateMachine stateMachine;

        /// <summary>角色Mover引用</summary>
        protected CharacterMover mover;

        /// <summary>角色Animator引用</summary>
        protected CharacterAnimatorController animatorCtrl;

        /// <summary>
        /// 初始化状态
        /// </summary>
        public void Initialize(CharacterStateMachine stateMachine)
        {
            this.stateMachine = stateMachine;
            this.mover = stateMachine.Mover;
            this.animatorCtrl = stateMachine.AnimatorCtrl;
            OnInitialize();
        }

        /// <summary>
        /// 状态初始化（可选重写）
        /// </summary>
        protected virtual void OnInitialize() { }

        /// <summary>
        /// 进入状态时调用
        /// </summary>
        public virtual void Enter()
        {
            Debug.Log($"[State] 进入状态: {StateType}");
        }

        /// <summary>
        /// 每帧更新（类似Update）
        /// </summary>
        public virtual void Execute()
        {
        }

        /// <summary>
        /// 物理更新（类似FixedUpdate）
        /// </summary>
        public virtual void PhysicsExecute()
        {
        }

        /// <summary>
        /// 退出状态时调用
        /// </summary>
        public virtual void Exit()
        {
            Debug.Log($"[State] 退出状态: {StateType}");
        }

        /// <summary>
        /// 切换到新状态
        /// </summary>
        protected void ChangeState(CharacterStateType newState)
        {
            stateMachine?.ChangeState(newState);
        }
    }
}
