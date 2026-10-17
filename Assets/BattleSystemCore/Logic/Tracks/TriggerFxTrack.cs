using BattleSystemCore.Logic.Actors;

namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>
    /// 特效轨道静态数据（工厂触发时刻装配）：fxId为表现库键（逻辑层不解析）；
    /// useImpactPos/impactPos/originActorId为位置源——工厂已按弹着点惯例裁决回退
    /// （useImpactPos=true为固定点；false跟随origin角色），最终落位由表现层轨道计算；
    /// durationTicks为模板持续时长（瞬时形态只作记录，行为判定用基类isDuration）。
    /// </summary>
    public class TriggerFxStaticData : BasicStaticData
    {
        /// <summary>特效库键（透传，逻辑层不解析）</summary>
        public string fxId = "";

        /// <summary>载荷内轨道索引（调试/编辑器联动用）</summary>
        public int trackIndex;

        /// <summary>位置来源：true=弹着点（工厂已确认上下文携带），false=origin角色位置</summary>
        public bool useImpactPos;

        /// <summary>弹着点快照（useImpactPos=true时有效）</summary>
        public Vec3 impactPos;

        /// <summary>origin解析的actorId（位置来源与持续型跟随对象）</summary>
        public uint originActorId;

        /// <summary>持续tick数（工厂由模板durationTicks装配）</summary>
        public int durationTicks;
    }

    /// <summary>
    /// 触发特效轨道（纯信号）：Enter广播TrackViewStarted、Exit广播配对的TrackViewEnded——
    /// 零战斗状态改动、不进状态哈希、逻辑侧无消费者。特效创建/落位/跟随全在表现层轨道
    /// TriggerFxVTrack（数据经staticData透传，位置计算在表现侧）。
    /// Action打断补Exit即"打断即停特效"免费获得。
    /// </summary>
    public class TriggerFxTrack : DurationTrack<TriggerFxStaticData>
    {
        public override void Enter()
        {
            PostViewStarted();
        }

        public override void Exit()
        {
            PostViewEnded();
        }
    }

    /// <summary>
    /// 触发特效轨道的瞬时形态（durationTicks≤1）：Enter广播一次创建信号，Exit广播
    /// 配对销毁信号（表现层特效自带生命周期自灭，销毁信号只负责清VTrack与注册表）。
    /// 数据语义与持续形态完全一致（同一StaticData）。
    /// </summary>
    public class TriggerFxTick : TickTrack<TriggerFxStaticData>
    {
        public override void Enter()
        {
            PostViewStarted();
        }

        public override void Exit()
        {
            PostViewEnded();
        }
    }
}
