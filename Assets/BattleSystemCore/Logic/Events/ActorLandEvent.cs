namespace BattleSystemCore.Logic.Events
{
    /// <summary>
    /// 落地事件（双总线）：垂直抛物线回落到地面时发出。
    /// logicBus同步派发→逻辑钩子（落地触发Buff/落地AoE等，handler可回写）；
    /// eventQueue帧末派发→表现反馈（落地尘土/音效/硬直动画，handler禁止回写）。
    /// </summary>
    public struct ActorLandEvent : ILogicEvent, IBattleEvent
    {
        /// <summary>落地单位</summary>
        public uint actorId;

        /// <summary>本次腾空最高点（milli，相对地面）。主动小跳与高击飞可据此分级表现</summary>
        public int peakHeight;
    }
}
