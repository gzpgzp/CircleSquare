namespace BattleSystemCore.Logic.Events
{
    /// <summary>Buff到期移除事件（驱散等强制移除后续扩展时同样复用）</summary>
    public struct BuffExpiredEvent : IBattleEvent
    {
        public uint actorId;
        public uint buffId;
    }
}
