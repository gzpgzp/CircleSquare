namespace BattleSystemCore.Logic.Actors
{
    using BattleSystemCore.Logic.Buffs;

    /// <summary>意图移动模式</summary>
    public enum MoveMode
    {
        /// <summary>静止</summary>
        Idle,

        /// <summary>固定速度向量（手动操控/AI直接给方向）</summary>
        Velocity,

        /// <summary>追踪目标：每帧动态取目标方向移动，进入停止距离后停步，目标远离自动续追</summary>
        Chase,
    }

    /// <summary>
    /// 移动组件：意图移动通道，负责"我想去哪"的持续移动。
    /// Chase模式每帧重新计算方向（目标移动则方向随之更新），时长不定，到点/目标消失即停——
    /// 这类动态移动无法用Track时间轴表达（参数创建时不可知），故独立成组件通道。
    /// 与MoveTrack（技能位移效果通道：冲锋/击退/闪现等参数预知的精确位移）相互独立、纯加法叠加。
    /// </summary>
    public class MovementComponent
    {
        private readonly Actor owner;

        public MoveMode mode;

        /// <summary>Velocity模式速度（milli/tick）</summary>
        public Vec3 velocity;

        // Chase模式参数
        public uint chaseTargetId;
        /// <summary>追踪标量速度（milli/tick）</summary>
        public int chaseSpeed;
        /// <summary>与目标距离（milli）小于等于该值时停步；目标远离后继续追</summary>
        public int stopDistance;

        public MovementComponent(Actor actor)
        {
            owner = actor;
        }

        /// <summary>固定速度向量移动（milli/tick）</summary>
        public void SetVelocity(Vec3 v)
        {
            mode = MoveMode.Velocity;
            velocity = v;
        }

        public void SetVelocity(int x, int y, int z)
        {
            SetVelocity(new Vec3(x, y, z));
        }

        /// <summary>
        /// 追踪目标：每帧朝目标方向以speed移动，进入stopDistance内停步。
        /// 目标死亡/移除后自动转Idle。
        /// </summary>
        public void ChaseTarget(uint targetId, int speedMilliPerTick, int stopDistanceMilli)
        {
            mode = MoveMode.Chase;
            chaseTargetId = targetId;
            chaseSpeed = speedMilliPerTick;
            stopDistance = stopDistanceMilli;
        }

        /// <summary>停止意图移动（不影响位移效果轨道）</summary>
        public void Stop()
        {
            mode = MoveMode.Idle;
            velocity = Vec3.zero;
        }

        /// <summary>每帧积分。由Actor.Update调用（在Buff更新之后），ActorManager按创建顺序驱动，时序确定。
        /// 眩晕/腾空时意图移动整体暂停（状态保持，解除后续追/落地恢复）；位移效果轨道不受影响</summary>
        public void Update()
        {
            if (owner.buffs.IsStunned || owner.vertical.airborne)
            {
                return;
            }

            switch (mode)
            {
                case MoveMode.Velocity:
                    owner.pos += velocity;
                    break;

                case MoveMode.Chase:
                    UpdateChase();
                    break;
            }
        }

        private void UpdateChase()
        {
            var battle = BattleSystem.Current;
            var target = battle.actorManager.GetActor(chaseTargetId);
            if (target == null)
            {
                // 目标已死亡/移除：追踪自然终止
                Stop();
                return;
            }

            long distSq = Vec3.DistanceSq(owner.pos, target.pos);
            long stopSq = (long)stopDistance * stopDistance;
            if (distSq <= stopSq)
            {
                // 已进入停止距离：停步但保持Chase模式，目标远离自动续追
                return;
            }

            // 追踪速度吃Buff聚合（减速/加速生效）：方向单位向量（长度约1000）×聚合速度/1000
            int speed = owner.buffs.GetStat(StatType.Speed, chaseSpeed);
            owner.pos += owner.pos.DirectionTo(target.pos) * speed / FixMath.MilliPerUnit;
        }
    }
}
