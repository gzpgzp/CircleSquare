using Battle3D.Character;
using Battle3D.Character.Brain;

namespace Battle3D.AI
{
    /// <summary>
    /// AI状态基类 - 定义状态的生命周期
    /// 参考现有项目 BaseState 并增强
    /// </summary>
    public abstract class BaseState
    {
        public abstract AIStateEnum StateId { get; }

        protected BaseCharacter character;
        protected StateMachine machine;
        protected AIBrain aiBrain;

        public BaseState(BaseCharacter character, StateMachine machine)
        {
            this.character = character;
            this.machine = machine;

            // 获取AIBrain引用
            if (character is Character3D)
            {
                // AIBrain 需要通过 Brain 属性访问
                // 这里通过反射或其他方式获取，简化处理
            }
        }

        /// <summary>设置AIBrain引用</summary>
        public void SetBrain(AIBrain brain)
        {
            aiBrain = brain;
        }

        /// <summary>进入状态</summary>
        public virtual void Enter() { }

        /// <summary>状态更新（Update）</summary>
        public virtual void Update(float deltaTime) { }

        /// <summary>物理更新（FixedUpdate）</summary>
        public virtual void FixedUpdate(float fixedDeltaTime) { }

        /// <summary>退出状态</summary>
        public virtual void Exit() { }
    }
}
