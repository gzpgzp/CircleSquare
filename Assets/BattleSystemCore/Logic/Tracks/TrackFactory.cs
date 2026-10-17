using System;
using System.Collections.Generic;
using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Buffs;
using BattleSystemCore.Logic.Physics;

namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>
    /// 轨道工厂：TrackType→实例化委托的注册表，模板（可序列化数据）→真轨道（运行时对象）的唯一转换入口。
    /// 职责三件事：解析RoleRef语义角色→具体actorId；相对tick（relTick）→绝对tick（baseTick锚点换算）；
    /// 配置层标量→运行时结构（x/y/z组装Vec3、buffId查BuffLibrary）。
    /// 技能层与Buff触发层共用本工厂：注册一种轨道，两侧同时获得该能力（配置驱动的扩展点）。
    /// 加新轨道类型=实现轨道类+在此注册一项工厂。
    /// </summary>
    public static class TrackFactory
    {
        private static readonly Dictionary<TrackType, Func<TrackTemplate, TrackContext, int, BaseTrack>> factories =
            new Dictionary<TrackType, Func<TrackTemplate, TrackContext, int, BaseTrack>>();

        static TrackFactory()
        {
            factories.Add(TrackType.DoDamage, CreateDoDamage);
            factories.Add(TrackType.ApplyBuff, CreateApplyBuff);
            factories.Add(TrackType.Move, CreateMove);
            factories.Add(TrackType.AttackShape, CreateAttackShape);
            factories.Add(TrackType.Knockup, CreateKnockup);
            factories.Add(TrackType.SpawnBullet, CreateSpawnBullet);
            factories.Add(TrackType.ControlBullet, CreateControlBullet);
            factories.Add(TrackType.TriggerFx, CreateTriggerFx);
        }

        /// <summary>
        /// 按模板实例化轨道。baseTick为触发基准帧（模板relTick的换算锚点），
        /// context提供RoleRef解析与伤害来源。未注册的类型视为配置错误，抛异常。
        /// </summary>
        public static BaseTrack Create(TrackTemplate template, TrackContext context, int baseTick)
        {
            if (template == null || !factories.TryGetValue(template.type, out var factory))
            {
                throw new ArgumentException($"TrackFactory: 未注册的轨道类型 {template?.type.ToString() ?? "null模板"}");
            }

            return factory(template, context, baseTick);
        }

        /// <summary>伤害轨道：origin对target结算一次攻击，来源取context（BuffEffect/Skill均不广播普攻信号）。
        /// 参数收DoDamageStaticData；无表现需求不注册viewHandle</summary>
        private static BaseTrack CreateDoDamage(TrackTemplate t, TrackContext ctx, int baseTick)
        {
            int tick = baseTick + t.relTick;
            var track = new DoDamageTick
            {
                startTick = tick,
                endTick = tick,
            };
            track.SetStaticData(new DoDamageStaticData
            {
                originActorId = ctx.Resolve(t.origin),
                targetActorId = ctx.Resolve(t.target),
                source = ctx.damageSource,
            });
            return track;
        }

        /// <summary>施加Buff轨道：向target施加buffId对应的Buff（经BuffLibrary解析）。
        /// 参数收ApplyBuffStaticData；无表现需求不注册viewHandle</summary>
        private static BaseTrack CreateApplyBuff(TrackTemplate t, TrackContext ctx, int baseTick)
        {
            var buff = BuffLibrary.Get(t.buffId);
            if (buff == null)
            {
                throw new ArgumentException($"TrackFactory: ApplyBuff模板引用了未注册的buffId {t.buffId}");
            }

            int tick = baseTick + t.relTick;
            var track = new ApplyBuffTick
            {
                startTick = tick,
                endTick = tick,
            };
            track.SetStaticData(new ApplyBuffStaticData
            {
                targetActorId = ctx.Resolve(t.target),
                buff = buff,
            });
            return track;
        }

        /// <summary>位移轨道：target在durationTicks内精确位移(x,y,z)千分单位。
        /// durationTicks防御下限1（避免endTick&lt;startTick的非法区间）。
        /// 参数收MoveStaticData；无表现需求不注册viewHandle</summary>
        private static BaseTrack CreateMove(TrackTemplate t, TrackContext ctx, int baseTick)
        {
            int duration = t.durationTicks < 1 ? 1 : t.durationTicks;
            int start = baseTick + t.relTick;
            var track = new MoveTrack
            {
                startTick = start,
                endTick = start + duration - 1,
            };
            track.SetStaticData(new MoveStaticData
            {
                targetActorId = ctx.Resolve(t.target),
                delta = new Vec3(t.x, t.y, t.z),
            });
            return track;
        }

        /// <summary>形状攻击轨道：origin按形状判定范围伤害，target为主目标保底与朝向参考。
        /// 判定心可配：shapeCenterMode=ImpactPoint且上下文携带弹着点时以弹着点为心（弹载荷场景）。
        /// 有表现需求：工厂分配viewHandle并注册进TrackViewRegistry（表现层AttackShapeVTrack
        /// 读staticData画AoE指示圈，含弹载荷的弹着点爆圈）</summary>
        private static BaseTrack CreateAttackShape(TrackTemplate t, TrackContext ctx, int baseTick)
        {
            int tick = baseTick + t.relTick;
            var track = new AttackShapeTick
            {
                startTick = tick,
                endTick = tick,
            };
            track.SetStaticData(new AttackShapeStaticData
            {
                originActorId = ctx.Resolve(t.origin),
                targetActorId = ctx.Resolve(t.target),
                useImpactCenter = t.shapeCenterMode == (int)ShapeCenterMode.ImpactPoint && ctx.hasImpactPos,
                impactCenter = ctx.impactPos,
                shape = new AttackShape
                {
                    type = (AttackShapeType)t.shapeType,
                    radius = t.shapeRadius,
                    minDot = t.shapeMinDot,
                    halfExtents = new Vec3(t.shapeHalfX, 0, t.shapeHalfZ),
                },
                source = ctx.damageSource,
            });
            RegisterViewTrack(BattleSystem.Current, track);
            return track;
        }

        /// <summary>击飞轨道：target以vy0/gravity抛物线腾空，落地发事件。
        /// 参数收KnockupStaticData；无表现需求不注册viewHandle</summary>
        private static BaseTrack CreateKnockup(TrackTemplate t, TrackContext ctx, int baseTick)
        {
            int tick = baseTick + t.relTick;
            var track = new KnockupTick
            {
                startTick = tick,
                endTick = tick,
            };
            track.SetStaticData(new KnockupStaticData
            {
                targetActorId = ctx.Resolve(t.target),
                vy0 = t.vy0,
                gravity = t.gravity,
            });
            return track;
        }

        /// <summary>生成子弹轨道：origin朝target方向发射（模板引用+内联覆盖由轨道运行时合成）。
        /// bulletLabel>0时弹注册句柄供后续ControlBullet操控。
        /// 参数收SpawnBulletTickStaticData；无表现需求不注册viewHandle</summary>
        private static BaseTrack CreateSpawnBullet(TrackTemplate t, TrackContext ctx, int baseTick)
        {
            int tick = baseTick + t.relTick;
            var track = new SpawnBulletTick
            {
                startTick = tick,
                endTick = tick,
            };
            track.SetStaticData(new SpawnBulletTickStaticData
            {
                originActorId = ctx.Resolve(t.origin),
                targetActorId = ctx.Resolve(t.target),
                projectileId = t.projectileId,
                bulletLabel = t.bulletLabel,
                speedOverride = t.bulletSpeed,
                radiusOverride = t.bulletRadius,
                rangeOverride = t.bulletRange,
                source = ctx.damageSource,
            });
            return track;
        }

        /// <summary>操控子弹轨道：origin名下bulletLabel的存活弹在durationTicks内每帧重设方向。
        /// duration防御下限1（照Move惯例）。有表现需求：工厂分配viewHandle并注册进
        /// TrackViewRegistry（表现层ControlBulletVTrack读staticData画导向标记）</summary>
        private static BaseTrack CreateControlBullet(TrackTemplate t, TrackContext ctx, int baseTick)
        {
            int duration = t.durationTicks < 1 ? 1 : t.durationTicks;
            int start = baseTick + t.relTick;
            var track = new ControlBulletTrack
            {
                startTick = start,
                endTick = start + duration - 1,
            };
            track.SetStaticData(new ControlBulletStaticData
            {
                ownerActorId = ctx.Resolve(t.origin),
                bulletLabel = t.bulletLabel,
                ctrlMode = (BulletCtrlMode)t.bulletCtrlMode,
                targetActorId = ctx.Resolve(t.target),
                fixedDirection = new Vec3(t.x, t.y, t.z),
            });
            RegisterViewTrack(BattleSystem.Current, track);
            return track;
        }

        /// <summary>触发特效轨道（纯信号）：durationTicks≤1分派瞬时形态（TriggerFxTick），
        /// ≥2分派持续形态（TriggerFxTrack）。两形态共用TriggerFxStaticData；工厂已按弹着点
        /// 惯例裁决回退（fxUseImpact且上下文携带弹着点→固定点，否则origin角色），
        /// 最终落位由表现层轨道计算。viewHandle每次实例化从BattleSystem领取唯一递增号</summary>
        private static BaseTrack CreateTriggerFx(TrackTemplate t, TrackContext ctx, int baseTick)
        {
            var battle = BattleSystem.Current;
            bool useImpact = t.fxUseImpact && ctx.hasImpactPos;
            int start = baseTick + t.relTick;
            var st = new TriggerFxStaticData
            {
                fxId = t.fxId,
                trackIndex = ctx.trackIndex,
                useImpactPos = useImpact,
                impactPos = ctx.impactPos,
                originActorId = ctx.Resolve(t.origin),
                durationTicks = t.durationTicks,
            };

            BaseTrack track;
            if (t.durationTicks >= 2)
            {
                track = new TriggerFxTrack
                {
                    startTick = start,
                    endTick = start + t.durationTicks - 1,
                };
            }
            else
            {
                track = new TriggerFxTick
                {
                    startTick = start,
                    endTick = start,
                };
            }

            track.SetStaticData(st);
            RegisterViewTrack(battle, track);
            return track;
        }

        /// <summary>为有表现需求的轨道分配viewHandle并注册进TrackViewRegistry
        /// （viewHandle!=0是轨道Enter广播表现层创建信号的前提）</summary>
        private static void RegisterViewTrack(BattleSystem battle, BaseTrack track)
        {
            if (battle == null)
            {
                return;
            }

            track.viewHandle = battle.nextViewHandle++;
            battle.trackViewRegistry.Register(track.viewHandle, track);
        }
    }
}
