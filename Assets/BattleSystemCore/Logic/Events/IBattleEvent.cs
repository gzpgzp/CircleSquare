namespace BattleSystemCore.Logic.Events
{
    /// <summary>
    /// 战斗逻辑事件标记接口。所有逻辑事件必须为纯struct，只携带值快照
    /// （id、数值），不引用Actor等可变对象——事件派发时源对象可能已被移除。
    /// </summary>
    public interface IBattleEvent
    {
    }
}
