namespace BattleSystemCore.Logic.Events
{
    /// <summary>Buff施加/叠层/刷新事件（重复施加也会触发，携带最新层数与剩余时长）</summary>
    public struct BuffAppliedEvent : IBattleEvent
    {
        public uint actorId;
        public uint buffId;
        public int stacks;
        public int remainTicks;
    }
}
