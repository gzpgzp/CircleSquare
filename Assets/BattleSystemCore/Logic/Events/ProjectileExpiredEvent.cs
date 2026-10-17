namespace BattleSystemCore.Logic.Events
{
    /// <summary>
    /// 投射物失效事件（表现总线）：累计飞行超出射程自毁时广播（哑弹/落空弹），
    /// 供表现层销毁弹体视图（可加落地烟尘）。纯表现信号，只挂IBattleEvent。
    /// </summary>
    public struct ProjectileExpiredEvent : IBattleEvent
    {
        /// <summary>投射物实例id</summary>
        public uint projectileUid;

        /// <summary>失效位置（milli）</summary>
        public int x;
        public int y;
        public int z;
    }
}
