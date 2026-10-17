namespace BattleSystemCore.Logic.Actors
{
    using BattleSystemCore.Logic.Buffs;

    public class ActorContext
    {
        public ActorContext(uint uID)
        {
            this.uID = uID;
        }

        private uint uID;
        public int camp;
        public int hp;
        public int mp;
        public int atk;
        public int def;
        public int speed;
        public int criticalRate;
        public int criticalMultiplier;
        
        
        public uint UID
        {
            get => uID; 
        }

        /// <summary>
        /// 基础属性值（未经Buff修改）。战斗结算读聚合值请用Actor.GetStat，
        /// 本方法仅供BuffComponent作聚合基准。字段即基础值语义。
        /// </summary>
        public int GetBaseStat(StatType type)
        {
            switch (type)
            {
                case StatType.Atk: return atk;
                case StatType.Def: return def;
                case StatType.Speed: return speed;
                case StatType.CritRate: return criticalRate;
                case StatType.CritMultiplier: return criticalMultiplier;
                default: return 0;
            }
        }
    }
}