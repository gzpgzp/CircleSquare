namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>
    /// 击飞轨道（瞬时）：startTick当帧对target施加垂直抛物线（vy0起跳，gravity回落，落地发事件）。
    /// 纯垂直位移——水平抛飞由"Knockup + Move"两条轨道组合表达。
    /// 目标腾空中则重置抛物线（空中接力）。
    /// 参数收KnockupStaticData。无表现需求：腾空轨迹由vertical组件驱动pos.y、
    /// BattleActorView逐帧跟随，落地弹跳走ActorLandEvent——viewHandle恒0
    /// </summary>
    public class KnockupTick : TickTrack<KnockupStaticData>
    {
        public override void Enter()
        {
            var st = _RealStaticData;
            var battle = BattleSystem.Current;
            var target = battle.actorManager.GetActor(st.targetActorId);
            FuncDelegator.DoKnockup(target, st.vy0, st.gravity);
        }
    }

    /// <summary>击飞轨道参数容器：工厂触发时刻装配（宿主剧本直建时手挂）</summary>
    public class KnockupStaticData : BasicStaticData
    {
        public uint targetActorId;

        /// <summary>垂直初速（milli/tick）</summary>
        public int vy0;

        /// <summary>重力（milli/tick²）</summary>
        public int gravity;
    }
}
