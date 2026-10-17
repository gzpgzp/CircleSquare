using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Physics;

namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>
    /// 形状攻击轨道（瞬时）：startTick当帧按shape判定范围，命中者逐个结算伤害。
    /// 判定心：FollowOrigin=origin角色当前位置（默认）；ImpactPoint=上下文弹着点（弹载荷：
    /// 命中接触点/超程落点，无弹着点回退origin角色）。
    /// 朝向：判定心→target的水平方向（扇形平分线；target无效时forward=zero，扇形退化为不命中，
    /// 圆/矩形不依赖朝向照常判定）。target同时作为主目标保底（走位脱判仍吃到）。
    /// 来源：context注入（技能=Skill/Buff触发=BuffEffect/弹载荷=弹source，均不广播普攻信号）。
    /// 参数收AttackShapeStaticData。表现：AoE指示圈——Enter广播通用表现信号，
    /// AttackShapeVTrack读staticData绘制（原SkillShapeCastEvent值快照通道已收编）。
    /// </summary>
    public class AttackShapeTick : TickTrack<AttackShapeStaticData>
    {
        public override void Enter()
        {
            // 表现信号先于结算：与本类其余信号轨道同构（Started/Ended永远配对，
            // 结算早退路径表现侧靠origin查不到自行跳过）
            PostViewStarted();
            var st = _RealStaticData;
            var battle = BattleSystem.Current;
            var attacker = battle.actorManager.GetActor(st.originActorId);
            if (attacker == null)
            {
                return;
            }

            // 判定心：ImpactPoint用弹着点（命中接触点/超程落点），否则attacker当前位置。
            // attacker仍是伤害来源与豁免主体（弹着点爆不改变攻击归属）
            Vec3 center = st.useImpactCenter ? st.impactCenter : attacker.pos;
            var target = battle.actorManager.GetActor(st.targetActorId);
            Vec3 forward = target != null ? center.DirectionTo(target.pos) : Vec3.zero;
            FuncDelegator.DoAttackArea(attacker, forward, st.shape, target, st.source,
                st.useImpactCenter ? st.impactCenter : (Vec3?)null);
        }

        public override void Exit()
        {
            PostViewEnded();
        }
    }

    /// <summary>形状攻击轨道参数容器：判定心/朝向参考/形状与伤害来源（工厂触发时刻装配）</summary>
    public class AttackShapeStaticData : BasicStaticData
    {
        public uint originActorId;

        /// <summary>主目标（保底+扇形朝向参考）。0=纯AoE（朝向zero，仅圆/矩形有意义）</summary>
        public uint targetActorId;

        /// <summary>判定心=上下文弹着点（工厂：shapeCenterMode==ImpactPoint且ctx.hasImpactPos）</summary>
        public bool useImpactCenter;

        /// <summary>弹着点坐标（useImpactCenter时的判定心）</summary>
        public Vec3 impactCenter;

        /// <summary>判定形状（工厂从模板标量组装）</summary>
        public AttackShape shape;

        public DamageSource source = DamageSource.Skill;
    }
}
