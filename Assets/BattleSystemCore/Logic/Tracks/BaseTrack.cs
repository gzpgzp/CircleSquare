using BattleSystemCore.Logic.Events;

namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>
    /// 轨道基类：Action时间轴上的一个行为片段，由startTick/endTick划定生效区间。
    /// isDuration决定执行模型：瞬时轨道（TickTrack）约定startTick == endTick，当帧Enter后即Exit；
    /// 持续轨道（DurationTrack）在区间内每帧Process。
    /// </summary>
    public abstract class BaseTrack
    {
        public string name;
        public uint trackId;
        public bool enable = true;
        public int startTick;
        public int endTick;
        public bool isDuration;

        /// <summary>表现层轨道句柄（工厂从BattleSystem分配，0=本轨道无表现层轨道）。
        /// 有表现需求的轨道类型（TriggerFx/ControlBullet/AttackShape等）在工厂实例化时注册进
        /// TrackViewRegistry，Enter/Exit经PostViewStarted/PostViewEnded广播通用信号</summary>
        public uint viewHandle;

        // 运行时状态，由BaseAction维护：供打断时判断哪些轨道已进入未退出
        internal bool entered;
        internal bool exited;

        protected BasicStaticData staticData;
        protected BasicDynamicData dynamicData;

        /// <summary>表现层只读数据面：staticData为工厂触发时刻的配置快照（含运行时解析值），
        /// 表现层轨道按逻辑轨道类型cast成具体容器读取</summary>
        public BasicStaticData SharedStaticData
        {
            get { return staticData; }
        }

        /// <summary>表现层只读数据面：dynamicData为轨道运行时状态容器（迁移中，现仅示范轨道使用）</summary>
        public BasicDynamicData SharedDynamicData
        {
            get { return dynamicData; }
        }

        // 工厂/宿主装配用（宿主剧本直建轨道时也需挂容器，如BattleDemo的AddChargeAction；
        // 表现层只读SharedXxx，永不调用本对方法）
        public void SetStaticData(BasicStaticData data)
        {
            staticData = data;
        }

        public void SetDynamicData(BasicDynamicData data)
        {
            dynamicData = data;
        }

        /// <summary>广播“表现层轨道创建”信号（有表现需求的轨道Enter时调用）。
        /// 纯信号只带句柄——数据经TrackViewRegistry取本轨道引用后读容器</summary>
        protected void PostViewStarted()
        {
            var battle = BattleSystem.Current;
            if (battle != null)
            {
                battle.eventQueue.Post(new TrackViewStartedEvent { viewHandle = viewHandle });
            }
        }

        /// <summary>广播“表现层轨道销毁”信号并请求移出注册表（轨道Exit时调用，
        /// 打断补Exit同样走到这里——打断即停表现免费获得）。移除延迟到帧末Flush之后
        /// 生效：瞬时轨道同帧Enter+Exit，事件派发时数据必须仍可读</summary>
        protected void PostViewEnded()
        {
            var battle = BattleSystem.Current;
            if (battle != null)
            {
                battle.eventQueue.Post(new TrackViewEndedEvent { viewHandle = viewHandle });
                battle.trackViewRegistry.RequestRemove(viewHandle);
            }
        }

        public abstract void Enter();
        public abstract void Exit();
    }
    
    public abstract class BasicStaticData
    {
    }

    public abstract class BasicDynamicData
    {
        
    }
    
    public class DefaultDynamicData: BasicDynamicData
    {
    }

    /// <summary>瞬时轨道：行为写在Enter里，当帧Enter+Exit。约定startTick == endTick</summary>
    public abstract class TickTrack : BaseTrack
    {
        public TickTrack()
        {
            isDuration = false;
        }

        public override void Enter()
        {
        }

        public override void Exit()
        {
        }
    }

    public abstract class TickTrack<ST, DT> : TickTrack
        where ST : BasicStaticData, new()
        where DT : BasicDynamicData, new()
    {
        public TickTrack()
        {
            // 泛型约束new()：派生类实例化即持有容器，工厂随后SetStaticData覆盖装配
            staticData = new ST();
            dynamicData = new DT();
        }

        protected ST _RealStaticData {
            get { return staticData as ST; }
        }
        
        protected DT _RealDynamicData {
            get { return dynamicData as DT; }
        }
    }

    public abstract class TickTrack<ST> : TickTrack<ST, DefaultDynamicData>
        where ST : BasicStaticData, new()
    {
        
    }

    /// <summary>持续轨道：Enter → 区间内每帧Process → Exit，适合位移、引导、状态类行为</summary>
    public abstract class DurationTrack : BaseTrack
    {
        public DurationTrack()
        {
            isDuration = true;
        }

        public virtual void Process()
        {
        }

        public override void Enter()
        {
        }

        public override void Exit()
        {
        }
    }

    public abstract class DurationTrack<ST, DT> : DurationTrack
        where ST : BasicStaticData, new()
        where DT : BasicDynamicData, new()
    {
        public DurationTrack()
        {
            staticData = new ST();
            dynamicData = new DT();
        }

        protected ST _RealStaticData
        {
            get { return staticData as ST; }
        }

        protected DT _RealDynamicData
        {
            get { return dynamicData as DT; }
        }
    }

    public abstract class DurationTrack<ST> : DurationTrack<ST, DefaultDynamicData>
        where ST : BasicStaticData, new()
    {

    }
}
