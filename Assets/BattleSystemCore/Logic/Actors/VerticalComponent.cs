namespace BattleSystemCore.Logic.Actors
{
    using BattleSystemCore.Logic.Events;

    /// <summary>
    /// 垂直运动组件：跳跃/击飞的第三移动通道，解析抛物线驱动（非数值积分）。
    /// 数学：Launch时快照 startY/vy0/gravity/startTick，此后每帧按
    /// y(t) = startY + vy0·t − g·t²/2（t=currTick−startTick，全整数）直接采样，
    /// y≤0 即落地钳回0并发ActorLandEvent——解析式无逐帧误差累积，落点精确可复现（确定性核心诉求）。
    /// 约定：地面高度 y=0，pos.y 即相对地面高度（milli）；gravity必须>0（否则永不落地）；
    /// 空中再被击飞=重置抛物线（以当前高度重新起算，支持空中接力）。
    /// 水平击退不走本组件（走MoveTrack位移轨道），两通道组合出"抛飞"效果。
    /// </summary>
    public class VerticalComponent
    {
        private readonly Actor owner;

        /// <summary>是否腾空。Launch置位，落地帧清除。MovementComponent据此暂停意图移动</summary>
        public bool airborne { get; private set; }

        // 抛物线参数只读视图（GetStateHash覆盖腾空轨迹用）
        public int StartTick => startTick;
        public int Vy0 => vy0;
        public int Gravity => gravity;
        public int StartY => startY;

        // 抛物线参数快照（Launch时写入，状态哈希覆盖）
        private int startY;
        private int vy0;
        private int gravity;
        private int startTick;
        private int peakY;

        public VerticalComponent(Actor actor)
        {
            owner = actor;
        }

        /// <summary>
        /// 起跳/被击飞：从当前tick起算抛物线。vy0为垂直初速（milli/tick，向上为正），
        /// gravity为重力（milli/tick²）。腾空中重复调用=重置抛物线（空中接力）。
        /// </summary>
        public void Launch(int vy0Milli, int gravityMilli)
        {
            var battle = BattleSystem.Current;
            airborne = true;
            startY = owner.pos.y;
            vy0 = vy0Milli;
            gravity = gravityMilli;
            startTick = battle.currTick;
            peakY = startY;
        }

        /// <summary>每帧解析采样。由Actor.Update调用（意图移动之后）。Launch与采样同帧时序安全：
        /// AdvanceTick的currTick++在帧末，Launch当帧Update拿到t=0——起跳当帧不积分（y停在起飞高度），
        /// 下帧t=1起正常采样；帧内结算阶段（Actor.Update之后）击飞则本帧已跳过、下帧同样t=1。落地帧钳回地面并发事件</summary>
        public void Update(int currTick)
        {
            if (!airborne)
            {
                return;
            }

            int t = currTick - startTick;
            if (t <= 0)
            {
                return;
            }
            long y = startY + (long)vy0 * t - (long)gravity * t * t / 2;
            if (y > peakY)
            {
                peakY = (int)y;
            }

            if (y <= 0)
            {
                // 落地：钳回地面并广播（双总线各取所需）
                owner.pos.y = 0;
                airborne = false;
                var battle = BattleSystem.Current;
                var evt = new ActorLandEvent
                {
                    actorId = owner.actorContext.UID,
                    peakHeight = peakY,
                };
                battle.logicBus.Post(evt);
                battle.eventQueue.Post(evt);
                return;
            }

            owner.pos.y = (int)y;
        }
    }
}
