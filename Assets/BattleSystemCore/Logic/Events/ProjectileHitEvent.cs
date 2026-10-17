namespace BattleSystemCore.Logic.Events
{
    /// <summary>
    /// 投射物命中事件（表现总线）：命中者结算完载荷后广播，供表现层播放命中/爆炸特效并销毁弹体视图。
    /// 纯表现信号，只挂IBattleEvent（载荷产生的伤害/Buff另有既有事件可观测）。
    /// </summary>
    public struct ProjectileHitEvent : IBattleEvent
    {
        /// <summary>投射物实例id</summary>
        public uint projectileUid;

        /// <summary>发射者</summary>
        public uint ownerId;

        /// <summary>命中单位</summary>
        public uint targetActorId;

        /// <summary>命中位置（milli）</summary>
        public int x;
        public int y;
        public int z;
    }
}
