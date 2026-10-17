namespace BattleSystemCore.Logic.Events
{
    /// <summary>
    /// 逻辑层内部事件标记。与表现层事件（IBattleEvent/BattleEventQueue）严格区分：
    /// 逻辑事件经LogicEventBus同步立即派发，handler允许回写战斗状态（触发效果、追加Action等）；
    /// 表现事件帧末批量派发，handler禁止回写。所有逻辑事件必须为纯struct。
    /// </summary>
    public interface ILogicEvent
    {
    }
}
