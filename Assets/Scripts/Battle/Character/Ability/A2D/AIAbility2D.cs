using Battle.Character.CharacterGlue;

namespace Battle.Character.Ability.A2D
{
    /// <summary>
    /// AI 专用 Ability 基类（2D）。
    /// 继承自 Ability2D，同时实现 IAIAbility 接口。
    /// 子类可直接通过 <see cref="blackboard"/> 读取感知数据，无需绑定任何按键输入。
    /// </summary>
    public abstract class AIAbility2D : Ability2D, IAIAbility
    {
        protected AIBlackboard blackboard;

        public void BindBlackboard(AIBlackboard board)
        {
            blackboard = board;
        }

        // AI Ability 不需要绑定输入，提供空实现以免子类重复书写
        public override void BindInput(Battle.Inputs.InputAction input) { }
        public override void UnbindInput(Battle.Inputs.InputAction input) { }
    }
}
