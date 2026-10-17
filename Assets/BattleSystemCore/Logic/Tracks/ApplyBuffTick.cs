using BattleSystemCore.Logic.Buffs;

namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>
    /// 施加Buff轨道（瞬时）：startTick当帧向target施加一个Buff。
    /// 只做触发，Buff生命周期由目标BuffComponent自主管理（独立于本Action——施放者Action结束/被打断，Buff照常生效到期）。
    /// 参数收ApplyBuffStaticData。无表现需求：Buff表现走BuffAppliedEvent（值快照事件通道），viewHandle恒0
    /// </summary>
    public class ApplyBuffTick : TickTrack<ApplyBuffStaticData>
    {
        public override void Enter()
        {
            var st = _RealStaticData;
            var target = BattleSystem.Current.actorManager.GetActor(st.targetActorId);
            target?.buffs.ApplyBuff(st.buff);
        }
    }

    /// <summary>施加Buff轨道参数容器：工厂触发时刻装配（宿主剧本直建时手挂）</summary>
    public class ApplyBuffStaticData : BasicStaticData
    {
        public uint targetActorId;

        /// <summary>Buff静态定义（全局共享只读）</summary>
        public BuffData buff;
    }
}
