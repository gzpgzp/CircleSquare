using Battle.Character.CharacterGlue;

namespace Battle.Character.Ability.A2D
{
    /// <summary>
    /// 标记接口：实现此接口的 Ability 会被 AIBrain 自动注入 Blackboard。
    /// AI Ability 通过 Blackboard 感知目标，不依赖任何 Input。
    /// </summary>
    public interface IAIAbility
    {
        void BindBlackboard(AIBlackboard blackboard);
    }
}
