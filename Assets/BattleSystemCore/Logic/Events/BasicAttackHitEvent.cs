namespace BattleSystemCore.Logic.Events
{
    /// <summary>
    /// 普攻命中信号（逻辑事件）：仅DamageSource.BasicAttack来源的伤害发出，
    /// 供攻击触发型Buff（三环等）订阅。BuffEffect来源的追加伤害不发本信号（防触发递归）。
    /// </summary>
    public struct BasicAttackHitEvent : ILogicEvent
    {
        public uint attackerId;
        public uint targetId;
        public int damage;
        public bool isCrit;
        public int hpAfter;
    }
}
