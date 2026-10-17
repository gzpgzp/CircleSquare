namespace BattleSystemCore.Logic.Events
{
    /// <summary>
    /// 表现层轨道销毁信号（通用对之二）：逻辑轨道Exit（播完/打断）时广播。
    /// 表现层按句柄销毁对应VTrack；注册表移除延迟到帧末Flush之后，派发时数据仍可读。
    /// </summary>
    public struct TrackViewEndedEvent : IBattleEvent
    {
        /// <summary>轨道实例句柄（与TrackViewStartedEvent配对）</summary>
        public uint viewHandle;
    }
}
