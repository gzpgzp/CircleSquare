using System.Collections.Generic;
using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Projectiles;

namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>生成子弹轨道参数容器：工厂触发时刻装配（宿主剧本直建时手挂）</summary>
    public class SpawnBulletTickStaticData : BasicStaticData
    {
        public uint originActorId;

        /// <summary>朝向参考（origin→target方向即弹道方向）</summary>
        public uint targetActorId;

        /// <summary>弹体定义id（经ProjectileLibrary解析；0=无模板全内联）</summary>
        public uint projectileId;

        /// <summary>弹句柄（0=不注册）</summary>
        public int bulletLabel;

        /// <summary>速度覆盖（milli/tick，0=用模板值）</summary>
        public int speedOverride;

        /// <summary>命中半径覆盖（milli，0=用模板值）</summary>
        public int radiusOverride;

        /// <summary>最大射程覆盖（milli，0=用模板值）</summary>
        public int rangeOverride;

        public DamageSource source = DamageSource.Skill;
    }

    /// <summary>
    /// 生成子弹轨道（瞬时）：startTick当帧以origin为发射点、origin→target方向发射。
    /// 弹体参数合成：projectileId>0时引模板（ProjectileLibrary解析，查无则不发射），
    /// bulletSpeed/bulletRadius/bulletRange>0时覆盖模板值；projectileId=0时全内联
    /// （bulletSpeed必须>0，否则不发射——无速弹无意义）。命中载荷取模板payload（无模板=空载荷哑弹）。
    /// bulletLabel>0时注册句柄，后续ControlBullet轨道可按(owner,label)操控该弹。
    /// target同时是朝向参考：无效或与origin重合时不发射（弹必须可定向）。
    /// 来源取context（技能弹=Skill）。参数收SpawnBulletTickStaticData。
    /// 无表现需求：弹视图由ProjectileLaunchedEvent（值快照事件通道）驱动ProjectileView，
    /// 生命周期跟弹实体不跟轨道——viewHandle恒0；弹的命中判定与载荷结算由ProjectileManager负责，
    /// 本轨道只管"发射"这一下
    /// </summary>
    public class SpawnBulletTick : TickTrack<SpawnBulletTickStaticData>
    {
        public override void Enter()
        {
            var st = _RealStaticData;
            var battle = BattleSystem.Current;
            if (battle == null)
            {
                return;
            }

            var origin = battle.actorManager.GetActor(st.originActorId);
            var target = battle.actorManager.GetActor(st.targetActorId);
            if (origin == null || target == null)
            {
                return;
            }

            // 模板解析与参数合成：覆盖>0 ? 覆盖 : 模板值（无模板时模板值取0）
            ProjectileData data = st.projectileId > 0 ? ProjectileLibrary.Get(st.projectileId) : null;
            if (st.projectileId > 0 && data == null)
            {
                return;
            }

            int speed = st.speedOverride > 0 ? st.speedOverride : (data != null ? data.speedMilliPerTick : 0);
            if (speed <= 0)
            {
                return;
            }

            int radius = st.radiusOverride > 0 ? st.radiusOverride : (data != null ? data.hitRadiusMilli : 0);
            int range = st.rangeOverride > 0 ? st.rangeOverride : (data != null ? data.maxRangeMilli : 0);
            List<TrackTemplate> payload = data != null ? data.payload : null;

            Vec3 direction = origin.pos.DirectionTo(target.pos);
            if (direction == Vec3.zero)
            {
                return;
            }

            FuncDelegator.DoSpawnBullet(origin, direction, speed, radius, range, payload, st.source, st.bulletLabel);
        }
    }
}
