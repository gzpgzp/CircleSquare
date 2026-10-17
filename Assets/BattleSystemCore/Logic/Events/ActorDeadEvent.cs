namespace BattleSystemCore.Logic.Events
{
    /// <summary>参与者死亡事件。派发时（帧末）该Actor已完成移除，只凭id语义行事</summary>
    public struct ActorDeadEvent : IBattleEvent
    {
        public uint actorId;
    }
}
