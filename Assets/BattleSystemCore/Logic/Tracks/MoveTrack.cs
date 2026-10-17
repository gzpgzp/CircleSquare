using BattleSystemCore.Logic.Actors;

namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>
    /// 位移效果轨道（持续）：在[startTick, endTick]内将target的总位移精确推进delta（千分单位）。
    /// 定位为参数预知的技能位移效果（冲锋/击退/闪现）：位移量与时长创建时已知，需与技能时间轴对齐，
    /// 总量模型保证精确到位。动态追踪类移动（方向每帧变、时长不定）请用MovementComponent.ChaseTarget，
    /// 那类行为不适合Track时间轴模型。
    /// 参数收MoveStaticData、运行时状态收MoveDynamicData（moved为总量模型已走量）。
    /// 无表现需求：不发表现信号，viewHandle恒0（位移表现由BattleActorView逐帧跟随actor.pos）。
    /// </summary>
    public class MoveTrack : DurationTrack<MoveStaticData, MoveDynamicData>
    {
        public override void Enter()
        {
            _RealDynamicData.moved = Vec3.zero;
        }

        public override void Process()
        {
            var st = _RealStaticData;
            var dt = _RealDynamicData;
            var battle = BattleSystem.Current;
            var actor = battle.actorManager.GetActor(st.targetActorId);
            if (actor == null)
            {
                return;
            }

            // 剩余Process次数（含当帧）：currTick==endTick时为1，步进=全部剩余，误差归零
            int ticksLeft = endTick - battle.currTick + 1;

            var step = (st.delta - dt.moved) / ticksLeft;
            actor.pos += step;
            dt.moved += step;
        }
    }

    /// <summary>位移轨道参数容器：工厂触发时刻装配（模板解析值）</summary>
    public class MoveStaticData : BasicStaticData
    {
        public uint targetActorId;

        /// <summary>总位移（千分单位），各分量可为负</summary>
        public Vec3 delta;
    }

    /// <summary>位移轨道运行时状态容器：moved为总量模型的已走量（误差收敛记录）</summary>
    public class MoveDynamicData : BasicDynamicData
    {
        /// <summary>已累计位移（千分单位），Enter清零</summary>
        public Vec3 moved;
    }
}
