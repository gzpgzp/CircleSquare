namespace BattleSystemCore.Logic.Actors
{
    using BattleSystemCore.Logic.Buffs;

    public class Actor
    {
        public ActorContext actorContext { get; private set; }

        /// <summary>空间位置（千分单位），位移轨道与移动组件直接写此字段</summary>
        public Vec3 pos;

        /// <summary>身体碰撞半径（milli，水平圆）。0=不参与推挤且命中判定不加成；
        /// 作为HitTest的pointRadius参与命中（形状按此值膨胀）与PhysicsWorld推挤</summary>
        public int bodyRadius;

        /// <summary>常规移动组件（速度积分通道）</summary>
        public MovementComponent movement { get; private set; }

        /// <summary>垂直运动组件（跳跃/击飞解析抛物线通道）</summary>
        public VerticalComponent vertical { get; private set; }

        /// <summary>Buff容器：属性修改与控制状态的载体</summary>
        public BuffComponent buffs { get; private set; }

        public void OnCreate(uint actorID)
        {
            actorContext = new ActorContext(actorID);
            movement = new MovementComponent(this);
            vertical = new VerticalComponent(this);
            buffs = new BuffComponent(this);
        }

        public void Init()
        {

        }

        /// <summary>Actor级每帧更新（ActorManager.Update阶段统一驱动，时序确定）。
        /// 顺序：Buff计时/到期（属性变更当帧生效）→ 意图移动（读到最新聚合属性，
        /// 空中/眩晕时暂停）→ 垂直积分（解析采样，落地当帧发事件）</summary>
        public void Update(int currTick)
        {
            buffs.Update();
            movement.Update();
            vertical.Update(currTick);
        }

        /// <summary>读取聚合属性：基础值经Buff修改后的当前生效值</summary>
        public int GetStat(StatType type)
        {
            return buffs.GetStat(type, actorContext.GetBaseStat(type));
        }

        /// <summary>承受伤害并扣减hp（下限0），返回剩余hp</summary>
        public int TakeDamage(int damage)
        {
            if (damage < 0)
            {
                damage = 0;
            }

            actorContext.hp -= damage;
            if (actorContext.hp < 0)
            {
                actorContext.hp = 0;
            }

            return actorContext.hp;
        }
    }
}
