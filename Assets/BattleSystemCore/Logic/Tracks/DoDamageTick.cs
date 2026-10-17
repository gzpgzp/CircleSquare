namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>伤害轨道（瞬时）：在startTick当帧由origin对target结算一次攻击。
    /// 参数收DoDamageStaticData。无表现需求：伤害表现走DamageDealtEvent（值快照事件通道，
    /// 受击闪白等宿主直接订阅），viewHandle恒0不发轨道信号</summary>
    public class DoDamageTick : TickTrack<DoDamageStaticData>
    {
        public override void Enter()
        {
            var st = _RealStaticData;
            var targetActor = BattleSystem.Current.actorManager.GetActor(st.targetActorId);
            var originActor = BattleSystem.Current.actorManager.GetActor(st.originActorId);
            if (targetActor != null && originActor != null)
            {
                FuncDelegator.DoAttack(originActor, targetActor, st.source);
            }
        }
    }

    /// <summary>伤害轨道参数容器：工厂触发时刻装配（宿主剧本直建时手挂）</summary>
    public class DoDamageStaticData : BasicStaticData
    {
        public uint targetActorId;
        public uint originActorId;

        /// <summary>伤害来源：默认BasicAttack（手写普通攻击行为，命中后广播触发信号）；
        /// 技能/触发载荷工厂按调用路径注入Skill/BuffEffect（不广播，防触发递归）</summary>
        public DamageSource source = DamageSource.BasicAttack;
    }
}