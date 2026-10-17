using System.Collections.Generic;
using BattleSystemCore.Logic.Actions;
using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Buffs;
using BattleSystemCore.Logic.Events;
using BattleSystemCore.Logic.Physics;
using BattleSystemCore.Logic.Projectiles;
using BattleSystemCore.Logic.Skills;
using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore.Logic
{
    /// <summary>
    /// 确定性自检：以固定种子与固定脚本输入完整跑两场相同战斗，逐帧比对状态哈希。
    /// 任何一帧哈希分歧即确定性被破坏。改动核心逻辑后应运行一次，也可作为单元测试的断言基础。
    /// </summary>
    public static class DeterminismChecker
    {
        // Buff静态定义（只读共享，两场战斗复用同一份数据，实例状态各自独立）
        private static readonly BuffData ShredBuff = new BuffData
        {
            buffId = 101,
            name = "CheckShred",
            durationTicks = 30,
            effects = { new StatModifierEffect { statType = StatType.Def, modType = ModifierType.Flat, value = -30 } },
        };

        private static readonly BuffData StunBuff = new BuffData
        {
            buffId = 102,
            name = "CheckStun",
            durationTicks = 15,
            isStun = true,
        };

        private static readonly BuffData AtkUpBuff = new BuffData
        {
            buffId = 103,
            name = "CheckAtkUp",
            durationTicks = 40,
            maxStacks = 3,
            effects = { new StatModifierEffect { statType = StatType.Atk, modType = ModifierType.Percent, value = 2000 } },
        };

        // 三环：每3次普攻命中，当帧追加一次伤害（relTick=0瞬时模板同帧结算，BuffEffect来源不反发普攻信号）
        private static readonly BuffData TriRingBuff = new BuffData
        {
            buffId = 104,
            name = "CheckTriRing",
            durationTicks = 100,
            effects =
            {
                new AttackTriggerEffect
                {
                    everyN = 3,
                    payload =
                    {
                        new TrackTemplate { type = TrackType.DoDamage, relTick = 0 },
                    },
                },
            },
        };

        // 回响：每2次普攻命中，当帧给目标上破甲 + 3tick后回响一击
        // （一条payload覆盖三种模板路径：同帧ApplyBuff、同帧DoDamage、延迟DoDamage走时间轴）
        private static readonly BuffData EchoBuff = new BuffData
        {
            buffId = 105,
            name = "CheckEcho",
            durationTicks = 100,
            effects =
            {
                new AttackTriggerEffect
                {
                    everyN = 2,
                    payload =
                    {
                        new TrackTemplate { type = TrackType.ApplyBuff, relTick = 0, buffId = 101 },
                        new TrackTemplate { type = TrackType.DoDamage, relTick = 3 },
                    },
                },
            },
        };

        // 技能：组合技（同帧眩晕+圆形形状攻击+击飞）——覆盖SkillCaster经TrackFactory
        // 到新轨道（AttackShape/Knockup）的全链路径；主目标保底确保B必吃到形状攻击
        private static readonly SkillData ComboSkill = new SkillData
        {
            skillId = 201,
            name = "CheckCombo",
            payload =
            {
                new TrackTemplate { type = TrackType.ApplyBuff, relTick = 0, buffId = 102 },
                new TrackTemplate
                {
                    type = TrackType.AttackShape, relTick = 0,
                    shapeType = (int)AttackShapeType.Sphere, shapeRadius = 3000,
                },
                new TrackTemplate { type = TrackType.Knockup, relTick = 0, vy0 = 800, gravity = 200 },
            },
        };

        // 直伤弹：命中时经载荷管线结算一次伤害（覆盖弹体飞行/命中/载荷同帧结算路径）
        private static readonly ProjectileData CheckBolt = new ProjectileData
        {
            projectileId = 301,
            name = "CheckBolt",
            speedMilliPerTick = 500,
            hitRadiusMilli = 400,
            maxRangeMilli = 6000,
            payload =
            {
                new TrackTemplate { type = TrackType.DoDamage, relTick = 0 },
            },
        };

        // 哑弹：射程内追不上目标，必然超程自毁（覆盖Expired路径；空载荷命中也不炸）
        private static readonly ProjectileData CheckDud = new ProjectileData
        {
            projectileId = 302,
            name = "CheckDud",
            speedMilliPerTick = 300,
            hitRadiusMilli = 100,
            maxRangeMilli = 1500,
        };

        // 爆炸弹：命中接触点/超程落点起爆（判定心=弹着点）——覆盖ImpactPoint两种触发源；
        // 载荷尾随两条特效：瞬时爆炸闪光 + 持续10tick余焰（均弹着点）——覆盖TriggerFx
        // 弹载荷场景（命中/超程爆点特效）与弹死后持续型特效入轨继续跑的路径
        private static readonly ProjectileData CheckBlast = new ProjectileData
        {
            projectileId = 304,
            name = "CheckBlast",
            speedMilliPerTick = 80,
            hitRadiusMilli = 100,
            maxRangeMilli = 9000,
            payload =
            {
                new TrackTemplate
                {
                    type = TrackType.AttackShape, relTick = 0,
                    shapeType = (int)AttackShapeType.Sphere, shapeRadius = 1500,
                    shapeCenterMode = (int)ShapeCenterMode.ImpactPoint,
                },
                new TrackTemplate
                {
                    type = TrackType.TriggerFx, relTick = 0,
                    fxId = "check_blast", fxUseImpact = true,
                },
                new TrackTemplate
                {
                    type = TrackType.TriggerFx, relTick = 0, durationTicks = 10,
                    fxId = "check_ember", fxUseImpact = true,
                },
            },
        };

        // 技能：命中爆弹（射程覆盖4000，弹接触C后在接触点起爆）——ImpactPoint的命中路径
        private static readonly SkillData MortarSkill = new SkillData
        {
            skillId = 204,
            name = "CheckMortar",
            payload =
            {
                new TrackTemplate
                {
                    type = TrackType.SpawnBullet, relTick = 0,
                    projectileId = 304, bulletRange = 4000,
                },
            },
        };

        // 技能：落点爆弹（射程覆盖1000，弹未接触任何目标即超程，在落点起爆波及C）——ImpactPoint的超程路径
        private static readonly SkillData FizzleSkill = new SkillData
        {
            skillId = 205,
            name = "CheckFizzle",
            payload =
            {
                new TrackTemplate
                {
                    type = TrackType.SpawnBullet, relTick = 0,
                    projectileId = 304, bulletRange = 1000,
                },
            },
        };

        // 技能：狙击（发射一发CheckBolt）——覆盖SpawnBulletTick轨道经模板工厂的全链路径
        private static readonly SkillData SnipeSkill = new SkillData
        {
            skillId = 202,
            name = "CheckSnipe",
            payload =
            {
                new TrackTemplate { type = TrackType.SpawnBullet, relTick = 0, projectileId = 301 },
            },
        };

        // 技能：制导弹（无模板内联慢速弹+句柄操控）——覆盖SpawnBullet全内联参数、label句柄注册、
        // ControlBullet持续追踪（弹自身每帧位移使追踪方向持续变化）、弹死后操控空转的完整链路
        private static readonly SkillData HomingSkill = new SkillData
        {
            skillId = 203,
            name = "CheckHoming",
            payload =
            {
                new TrackTemplate
                {
                    type = TrackType.SpawnBullet, relTick = 0,
                    projectileId = 0, bulletSpeed = 80, bulletRadius = 300, bulletRange = 9000, bulletLabel = 1,
                },
                new TrackTemplate
                {
                    type = TrackType.ControlBullet, relTick = 1, durationTicks = 60,
                    bulletLabel = 1, bulletCtrlMode = (int)BulletCtrlMode.FollowTarget,
                },
            },
        };

        // 技能：闪光（瞬时特效，fxUseImpact=true但技能直放无弹着点→回退origin=A位置）
        // ——覆盖TriggerFx的回退路径与瞬时形态（Enter广播一次，无停止配对）
        private static readonly SkillData FlashSkill = new SkillData
        {
            skillId = 206,
            name = "CheckFlash",
            payload =
            {
                new TrackTemplate
                {
                    type = TrackType.TriggerFx, relTick = 0,
                    fxId = "check_flash", fxUseImpact = true,
                },
            },
        };

        // 技能：光环（持续15tick特效，位置源origin=A跟随）——覆盖TriggerFx持续形态
        // （Enter广播触发→轨道入轨跑满15tick→Exit广播停止，与FxTriggered配对销毁）
        private static readonly SkillData AuraSkill = new SkillData
        {
            skillId = 207,
            name = "CheckAura",
            payload =
            {
                new TrackTemplate
                {
                    type = TrackType.TriggerFx, relTick = 0, durationTicks = 15,
                    fxId = "check_aura",
                },
            },
        };

        /// <summary>跑自检。divergeTick为首个分歧帧号（通过时为-1；
        /// 哈希分歧为帧号，特效事件序列分歧为-2）</summary>
        public static bool RunCheck(out int divergeTick, int tickCount = 100, uint seed = 12345u)
        {
            // 装载本检查器的配置集（先清空：Demo等宿主可能注册过自己的同名id定义）。
            // payload里的ApplyBuff模板按buffId经Library解析，同引用幂等，双跑复用安全
            BuffLibrary.Clear();
            BuffLibrary.Register(ShredBuff);
            BuffLibrary.Register(StunBuff);
            BuffLibrary.Register(AtkUpBuff);
            BuffLibrary.Register(TriRingBuff);
            BuffLibrary.Register(EchoBuff);

            // 技能库同样先清后注（Demo/Loader可能注册过自己的同名id）
            SkillLibrary.Clear();
            SkillLibrary.Register(ComboSkill);
            SkillLibrary.Register(SnipeSkill);
            SkillLibrary.Register(HomingSkill);
            SkillLibrary.Register(MortarSkill);
            SkillLibrary.Register(FizzleSkill);
            SkillLibrary.Register(FlashSkill);
            SkillLibrary.Register(AuraSkill);

            // 投射物库同理（弹体定义被轨道模板以id引用）
            ProjectileLibrary.Clear();
            ProjectileLibrary.Register(CheckBolt);
            ProjectileLibrary.Register(CheckDud);
            ProjectileLibrary.Register(CheckBlast);

            var fxLogA = new List<string>();
            var hashesA = RunBattle(tickCount, seed, fxLogA);
            var fxLogB = new List<string>();
            var hashesB = RunBattle(tickCount, seed, fxLogB);

            for (int i = 0; i < tickCount; i++)
            {
                if (hashesA[i] != hashesB[i])
                {
                    divergeTick = i;
                    return false;
                }
            }

            // 表现轨道信号不进状态哈希，另行独立比对两跑的TrackView事件序列
            // （句柄分配序/容器装配值/时机全量一致，含TriggerFx与ControlBullet两类）。
            // 空日志=信号链路根本没触发，同样视为失败（用例至少应触发六条Started）
            if (fxLogA.Count == 0 || fxLogA.Count != fxLogB.Count)
            {
                divergeTick = -2;
                return false;
            }

            for (int i = 0; i < fxLogA.Count; i++)
            {
                if (fxLogA[i] != fxLogB[i])
                {
                    divergeTick = -2;
                    return false;
                }
            }

            divergeTick = -1;
            return true;
        }

        /// <summary>表现轨道信号的数据面摘要（fxLog用）：按轨道类型读staticData拼确定串。
        /// 只记工厂装配的源数据（弹着点/origin/模式等）——最终落位是表现侧职责，逻辑侧不记。
        /// 新增有表现需求的轨道类型时此处补一个分支，保fxLog覆盖</summary>
        private static string DescribeViewTrack(BaseTrack track)
        {
            if (track is TriggerFxTrack || track is TriggerFxTick)
            {
                var st = (TriggerFxStaticData)track.SharedStaticData;
                return $"TriggerFx fx={st.fxId} impact={st.useImpactPos}"
                    + $" @({st.impactPos.x},{st.impactPos.y},{st.impactPos.z})"
                    + $" origin{st.originActorId} d{st.durationTicks} idx{st.trackIndex}";
            }

            if (track is ControlBulletTrack)
            {
                var st = (ControlBulletStaticData)track.SharedStaticData;
                return $"ControlBullet label={st.bulletLabel} owner{st.ownerActorId} mode{st.ctrlMode}"
                    + $" target{st.targetActorId} span[{track.startTick}..{track.endTick}]";
            }

            if (track is AttackShapeTick)
            {
                var st = (AttackShapeStaticData)track.SharedStaticData;
                return $"AttackShape type={st.shape.type} r{st.shape.radius} owner{st.originActorId}"
                    + $" target{st.targetActorId} impact={st.useImpactCenter}"
                    + $" @({st.impactCenter.x},{st.impactCenter.y},{st.impactCenter.z})";
            }

            return track.GetType().Name;
        }

        /// <summary>按固定脚本跑一场战斗，记录每帧状态哈希与表现轨道信号序列（fxLog）。覆盖伤害、暴击、死亡、速度积分、位移轨道、
        /// 单位推挤（完全重合/常规分离）、边界钳制、三形状区域命中（球/扇形/矩形）与主目标保底、
        /// 垂直击飞抛物线（腾空移动暂停/空中受击/落地事件回写钩子）、
        /// 技能链路（SkillCaster→模板工厂→形状攻击/击飞轨道）、
        /// 投射物链路（发射轨道→飞行积分→命中载荷结算/超程自毁）</summary>
        private static List<int> RunBattle(int tickCount, uint seed, List<string> fxLog)
        {
            var battle = new BattleSystem();
            battle.Init(seed);

            // 表现轨道信号记录：帧末Flush时逐条写入（创建/销毁都记，带帧号与句柄；
            // 数据经TrackViewRegistry读轨道staticData——注册表帧末Flush后才清，必可读）。
            // 两跑比对即验证信号链路的确定性（触发时机/句柄分配序/容器装配值）
            battle.eventQueue.Subscribe<TrackViewStartedEvent>(evt =>
            {
                var track = battle.trackViewRegistry.Get(evt.viewHandle);
                fxLog.Add($"t{battle.currTick} v{evt.viewHandle} start {DescribeViewTrack(track)}");
            });
            battle.eventQueue.Subscribe<TrackViewEndedEvent>(evt =>
            {
                fxLog.Add($"t{battle.currTick} v{evt.viewHandle} end");
            });

            // 落地钩子：落地单位被"摔破防"（上ShredBuff）——验证ActorLandEvent经logicBus同步回写的路径
            battle.logicBus.Subscribe<ActorLandEvent>(evt =>
            {
                var landed = battle.actorManager.GetActor(evt.actorId);
                landed?.buffs.ApplyBuff(ShredBuff);
            });

            var a = battle.actorManager.CreateActor();
            var b = battle.actorManager.CreateActor();
            var c = battle.actorManager.CreateActor();
            ConfigureFighter(a.actorContext, 1000, 120, 30, 3000, 15000);
            ConfigureFighter(b.actorContext, 900, 100, 45, 2500, 18000);
            ConfigureFighter(c.actorContext, 5000, 10, 0, 0, 0);

            // 竞技场：原点为心半径5单位（c的冲锋位移将越界，触发边界钳制路径）
            battle.physics.Init(Vec3.zero, 5000);

            // 身体碰撞半径（milli）：推挤参与与命中判定膨胀
            a.bodyRadius = 300;
            b.bodyRadius = 300;
            c.bodyRadius = 300;

            // 初始站位拉开5逻辑单位，双方互相追踪（Chase模式，每帧动态方向）；
            // c与a完全重合出生：首tick触发推挤的完全重合分离分支（uid定方向），随后被顶开原地站桩
            a.pos = new Vec3(-2500, 0, 0);
            b.pos = new Vec3(2500, 0, 0);
            c.pos = new Vec3(-2500, 0, 0);
            a.movement.ChaseTarget(b.actorContext.UID, 100, 2000);
            b.movement.ChaseTarget(a.actorContext.UID, 100, 2000);

            var hashes = new List<int>(tickCount);
            for (int t = 0; t < tickCount; t++)
            {
                // 每7tick双方互相攻击一次，命中定在下一tick（相对tick→绝对tick换算）
                if (t % 7 == 0)
                {
                    AddAttackAction(battle, a.actorContext.UID, b.actorContext.UID, battle.currTick + 1);
                    AddAttackAction(battle, b.actorContext.UID, a.actorContext.UID, battle.currTick + 1);
                }

                // 第20tick给A加一个3tick的位移效果轨道（向右冲1500milli，会冲过停止距离），
                // 冲锋后Chase方向反转续追：覆盖动态方向变化与双通道叠加路径
                if (t == 20)
                {
                    AddChargeAction(battle, a.actorContext.UID, battle.currTick + 1, 3);
                }

                // 第40tick给C一个3tick位移轨道（向+x冲8单位）：越出竞技场半径5被钳回圆周，覆盖边界路径
                if (t == 40)
                {
                    AddChargeAction(battle, c.actorContext.UID, battle.currTick + 1, 3);
                }

                // 爆炸弹：t==45 A对C施放命中爆弹（弹速80/射程覆盖4000/命中半径100）：
                // 弹飞至距C约400处接触命中，AttackShape(Sphere 1500, 弹着点心)在接触点起爆打C
                // ——覆盖ImpactPoint判定心的命中爆路径；
                // t==50 A对C施放落点爆弹（射程覆盖1000）：弹飞13tick未接触即超程，
                // 在落点起爆波及C——覆盖ImpactPoint的超程落点爆路径（A豁免，仅C受击）
                if (t == 45)
                {
                    SkillCaster.Cast(MortarSkill.skillId, a.actorContext.UID, c.actorContext.UID);
                }

                if (t == 50)
                {
                    SkillCaster.Cast(FizzleSkill.skillId, a.actorContext.UID, c.actorContext.UID);
                }

                // 区域攻击路径：t==25球形（A为心半径3单位，B/C均在内→多目标结算、逐者广播普攻信号）；
                // t==40扇形（A朝B半角60°：B正前命中，C背向走剪枝不命中，主目标保底路径）；
                // t==55矩形（A为心8×8单位，此时C已被钳在场地边缘）
                if (t == 25)
                {
                    FuncDelegator.DoAttackArea(a, Vec3.zero, AttackShape.Sphere(3000));
                }

                if (t == 40)
                {
                    FuncDelegator.DoAttackArea(a, a.pos.DirectionTo(b.pos), AttackShape.Sector(5000, 5000), b);
                }

                // 垂直通道：t==50击飞B（vy0=800,g=200→顶点1600milli@t=54，t=58落地），
                // 腾空期B意图移动暂停；t==53球形AoE命中腾空中的B（水平面判定，空中照常可被打）；
                // t=58落地经logicBus钩子给B上破甲（落地回写路径进哈希）
                if (t == 50)
                {
                    FuncDelegator.DoKnockup(b, 800, 200);
                }

                if (t == 53)
                {
                    FuncDelegator.DoAttackArea(a, Vec3.zero, AttackShape.Sphere(3000));
                }

                if (t == 55)
                {
                    FuncDelegator.DoAttackArea(a, Vec3.zero, AttackShape.Box(4000, 4000));
                }

                // 技能路径：t==60 A对B施放组合技（同帧眩晕+圆形形状攻击+击飞，均经模板工厂实例化）。
                // 击飞在t==68落地（vy0=800,g=200），落地钩子再上一次破甲；眩晕+腾空双重冻结B的意图移动
                if (t == 60)
                {
                    SkillCaster.Cast(ComboSkill.skillId, a.actorContext.UID, b.actorContext.UID);
                }

                // 投射物路径：t==30 A对B狙击（技能→SpawnBulletTick轨道→工厂→弹体飞行），
                // 弹速500/命中半径400，几tick后命中B并当帧结算伤害（75=atk120-def45）
                if (t == 30)
                {
                    SkillCaster.Cast(SnipeSkill.skillId, a.actorContext.UID, b.actorContext.UID);
                }

                // 制导弹：t==35 A对B施放（SpawnBullet全内联慢速弹注册label=1句柄，
                // ControlBullet自t==36起持续60tick追踪B）。弹速80追相距约2000的B，
                // 弹自身每帧位移使追踪方向持续变化（弯曲弹道），约25tick后命中B当帧结算伤害；
                // 弹死后操控轨道余下tick经GetByLabel空转——句柄剔除链路
                if (t == 35)
                {
                    SkillCaster.Cast(HomingSkill.skillId, a.actorContext.UID, b.actorContext.UID);
                }
                
                // 特效链路：t==65 A施放闪光（瞬时+无弹着点回退origin=A位置）；
                // t==66 A施放光环（持续15tick跟随A，t==81 Exit广播停止）——
                // Mortar/Fizzle的爆炸特效已随CheckBlast载荷覆盖（弹着点位置源）
                if (t == 65)
                {
                    SkillCaster.Cast(FlashSkill.skillId, a.actorContext.UID, b.actorContext.UID);
                }
                
                if (t == 66)
                {
                    SkillCaster.Cast(AuraSkill.skillId, a.actorContext.UID, b.actorContext.UID);
                }

                // 哑弹：t==70 直发一发超程弹（射程1500追不上间距2000的B）→ 超程自毁路径
                if (t == 70)
                {
                    FuncDelegator.DoSpawnBullet(a, a.pos.DirectionTo(b.pos),
                        CheckDud.speedMilliPerTick, CheckDud.hitRadiusMilli, CheckDud.maxRangeMilli,
                        CheckDud.payload, DamageSource.Skill, 0);
                }

                // Buff路径：破甲（伤害变化）→眩晕（A停走15tick）→叠层攻击up（重复施加叠到2层+刷新时长）
                if (t == 5)
                {
                    AddBuffAction(battle, ShredBuff, b.actorContext.UID, battle.currTick + 1);
                }

                if (t == 12)
                {
                    AddBuffAction(battle, StunBuff, a.actorContext.UID, battle.currTick + 1);
                }

                if (t == 18 || t == 30)
                {
                    AddBuffAction(battle, AtkUpBuff, a.actorContext.UID, battle.currTick + 1);
                }

                // 三环：开局直接施加在A身上（同步施加，非轨道），覆盖触发计数/同帧模板追加伤害/防递归路径
                if (t == 0)
                {
                    a.buffs.ApplyBuff(TriRingBuff);
                }

                // 回响：t==8经轨道施加在B身上（命中定在t==9，B的前2次命中发生在t==8之前不计数），
                // 生效后B的命中序列t=15(计1)、t=22(计2达标)：当帧给A上破甲 + t==25回响一击，
                // 覆盖同帧ApplyBuff模板与延迟DoDamage模板（时间轴载荷）路径
                if (t == 8)
                {
                    AddBuffAction(battle, EchoBuff, b.actorContext.UID, battle.currTick + 1);
                }

                battle.AdvanceTick();
                hashes.Add(battle.GetStateHash());
            }

            return hashes;
        }

        private static void ConfigureFighter(ActorContext ctx, int hp, int atk, int def, int criticalRate, int criticalMultiplier)
        {
            ctx.hp = hp;
            ctx.mp = 100;
            ctx.atk = atk;
            ctx.def = def;
            ctx.criticalRate = criticalRate;
            ctx.criticalMultiplier = criticalMultiplier;
        }

        private static void AddAttackAction(BattleSystem battle, uint originId, uint targetId, int hitTick)
        {
            var action = new BaseAction
            {
                actionId = 1,
                name = "CheckAttack",
            };
            var track = new DoDamageTick
            {
                startTick = hitTick,
                endTick = hitTick,
            };
            track.SetStaticData(new DoDamageStaticData
            {
                originActorId = originId,
                targetActorId = targetId,
            });
            action.tracks.Add(track);
            battle.actionManager.AddAction(action);
        }

        private static void AddChargeAction(BattleSystem battle, uint actorId, int startTick, int durationTicks)
        {
            var action = new BaseAction
            {
                actionId = 2,
                name = "CheckCharge",
            };
            var track = new MoveTrack
            {
                startTick = startTick,
                endTick = startTick + durationTicks - 1,
            };
            track.SetStaticData(new MoveStaticData
            {
                targetActorId = actorId,
                delta = new Vec3(1500, 0, 0),
            });
            action.tracks.Add(track);
            battle.actionManager.AddAction(action);
        }

        private static void AddBuffAction(BattleSystem battle, BuffData buff, uint targetId, int applyTick)
        {
            var action = new BaseAction
            {
                actionId = 3,
                name = "CheckBuff_" + buff.name,
            };
            var track = new ApplyBuffTick
            {
                startTick = applyTick,
                endTick = applyTick,
            };
            track.SetStaticData(new ApplyBuffStaticData
            {
                targetActorId = targetId,
                buff = buff,
            });
            action.tracks.Add(track);
            battle.actionManager.AddAction(action);
        }
    }
}
