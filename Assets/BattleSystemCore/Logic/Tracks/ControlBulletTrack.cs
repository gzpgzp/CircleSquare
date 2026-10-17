using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Projectiles;

namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>
    /// 操控子弹轨道静态数据（工厂触发时刻装配）：操控者/弹句柄/模式/目标/固定向量。
    /// 逻辑Process读它结算，表现层轨道ControlBulletVTrack读它查弹列表画导向标记。
    /// </summary>
    public class ControlBulletStaticData : BasicStaticData
    {
        /// <summary>操控者（=句柄注册时的owner键，工厂取origin解析）</summary>
        public uint ownerActorId;

        /// <summary>弹句柄（须与SpawnBullet的bulletLabel一致；0在配置层被校验拦截）</summary>
        public int bulletLabel;

        /// <summary>操控模式（FollowTarget=追踪 / FixedDirection=固定向量）</summary>
        public BulletCtrlMode ctrlMode;

        /// <summary>追踪目标（FollowTarget模式用）</summary>
        public uint targetActorId;

        /// <summary>固定方向原始向量（FixedDirection模式用，工厂由模板x/y/z组装）</summary>
        public Vec3 fixedDirection;
    }

    /// <summary>
    /// 操控子弹轨道（持续）：在[startTick, endTick]内每帧对本caster名下bulletLabel对应的
    /// 存活弹重设飞行方向（只改向不改速，速率为弹自身的speedMilliPerTick）：
    /// FollowTarget=朝target当前位置（目标死亡/位置重合则当帧跳过，保持原向）；
    /// FixedDirection=重设为fixedDirection方向（原始向量，内部经DirectionTo归一化；零向量跳过）。
    /// 句柄由同技能（或同caster任意载荷）前置的SpawnBullet轨道注册；操控轨道结束或技能打断后，
    /// 弹保持最后速度直线飞行。多弹同句柄时按spawn序逐个操控，确定。
    /// 结算是纯逻辑（重设弹velocity）；另有表现需求——Enter/Exit广播TrackViewStarted/
    /// TrackViewEnded，表现层ControlBulletVTrack读staticData查弹列表画导向标记。
    /// </summary>
    public class ControlBulletTrack : DurationTrack<ControlBulletStaticData>
    {
        public override void Enter()
        {
            PostViewStarted();
        }

        public override void Process()
        {
            var st = _RealStaticData;
            var battle = BattleSystem.Current;
            if (battle == null || st.bulletLabel <= 0)
            {
                return;
            }

            var bullets = battle.projectileManager.GetByLabel(st.ownerActorId, st.bulletLabel);
            if (bullets.Count == 0)
            {
                return;
            }

            Vec3 direction;
            if (st.ctrlMode == BulletCtrlMode.FollowTarget)
            {
                var target = battle.actorManager.GetActor(st.targetActorId);
                if (target == null || target.actorContext.hp <= 0)
                {
                    return;
                }

                // 追踪方向按弹各自位置计算（多弹时各追各的）；重合返回zero则跳过该弹
                for (int i = 0; i < bullets.Count; i++)
                {
                    var p = bullets[i];
                    direction = p.pos.DirectionTo(target.pos);
                    if (direction == Vec3.zero)
                    {
                        continue;
                    }

                    p.velocity = direction * p.speedMilliPerTick / 1000;
                }

                return;
            }

            // FixedDirection：原始向量归一化一次（零向量整条轨道无操作）
            if (st.fixedDirection == Vec3.zero)
            {
                return;
            }

            direction = Vec3.zero.DirectionTo(st.fixedDirection);
            for (int i = 0; i < bullets.Count; i++)
            {
                bullets[i].velocity = direction * bullets[i].speedMilliPerTick / 1000;
            }
        }

        public override void Exit()
        {
            PostViewEnded();
        }
    }
}
