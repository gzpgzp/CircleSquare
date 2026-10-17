namespace BattleSystemCore.Logic.Events
{
    /// <summary>伤害结算事件：一次攻击命中后的结果快照</summary>
    public struct DamageDealtEvent : IBattleEvent
    {
        public uint originActorId;
        public uint targetActorId;
        public int damage;
        public bool isCrit;
        public int hpAfter;
    }
}
