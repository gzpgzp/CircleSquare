namespace BattleSystemCore.Logic.Events
{
    /// <summary>
    /// 表现层轨道创建信号（通用对之一）：任何"有表现层轨道"的逻辑轨道Enter时广播，
    /// 与TrackViewEndedEvent成对。纯信号只带句柄——数据（staticData/dynamicData）经
    /// TrackViewRegistry取轨道引用后读取；零战斗状态、不进状态哈希、逻辑侧无消费者。
    /// </summary>
    public struct TrackViewStartedEvent : IBattleEvent
    {
        /// <summary>轨道实例句柄（工厂从BattleSystem.nextViewHandle领取）</summary>
        public uint viewHandle;
    }
}
