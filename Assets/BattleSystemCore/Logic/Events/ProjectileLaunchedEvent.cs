namespace BattleSystemCore.Logic.Events
{
    /// <summary>
    /// 投射物发射事件（表现总线）：弹体生成时广播，供表现层创建子弹视图。
    /// 纯表现信号，逻辑侧无消费者（命中伤害走DamageDealtEvent等既有事件），只挂IBattleEvent。
    /// 坐标/速度均为milli标量快照。
    /// </summary>
    public struct ProjectileLaunchedEvent : IBattleEvent
    {
        /// <summary>投射物实例id</summary>
        public uint projectileUid;

        /// <summary>发射者</summary>
        public uint ownerId;

        /// <summary>出生位置（milli）</summary>
        public int x;
        public int y;
        public int z;

        /// <summary>每tick位移向量（milli/tick，直线弹恒定；表现层可据此定朝向与拖尾方向）</summary>
        public int vx;
        public int vy;
        public int vz;
    }
}
