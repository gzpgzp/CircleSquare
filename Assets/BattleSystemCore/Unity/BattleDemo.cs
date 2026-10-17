using BattleSystemCore.Logic;
using BattleSystemCore.Logic.Actions;
using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Buffs;
using BattleSystemCore.Logic.Events;
using BattleSystemCore.Logic.Skills;
using BattleSystemCore.Logic.Tracks;
using System.Collections.Generic;
using UnityEngine;

namespace BattleSystemCore.Unity
{
    /// <summary>
    /// 阶段三验收Demo：双方相向逼近，远程阶段A发射火球（技能4：投射物飞行+命中爆炸载荷），
    /// 进入射程后互相攻击；A首次入射程释放技能1（破甲+眩晕+AoE+击飞）并冲锋，
    /// B血量过半反打技能2（扇形+自增益+延迟二段），A每60tick循环释放技能3（矩形+补刀）——技能全部由配表驱动。
    /// 表现层：逻辑pos为权威，BattleActorView影子同步（平滑/朝向/落地弹跳/受击闪白/离地拖尾/地面投影），
    /// 技能施放的AoE指示圈由AttackShapeVTrack绘制（TrackView信号驱动，与逻辑轨道同生共死）；物理与形状参数写死（不经配置表）。
    /// 启动时先跑确定性自检（覆盖伤害/暴击/死亡/速度积分/位移轨道/推挤/边界/区域命中/击飞落地/技能链路）。
    /// 挂到场景任意物体即可运行。
    /// </summary>
    public class BattleDemo : MonoBehaviour
    {
        /// <summary>攻击射程（逻辑单位，内部乘千分单位比较）</summary>
        private const int AttackRange = 2;
        private const int RangeSqMilli = AttackRange * AttackRange * FixMath.MilliPerUnit * FixMath.MilliPerUnit;

        // 物理参数（写死约定：碰撞/边界参数不做配置化，调整后直接改这里重编译）；
        // 击飞初速/重力属技能载荷参数，随技能入配表——见TrackTemplate.csv组401
        private const int BodyRadiusMilli = 300;      // 单位身体碰撞半径（推挤与命中膨胀）
        private const int ArenaRadiusMilli = 10000;   // 竞技场半径（10逻辑单位）

        [Header("调试参数（血量仅Play前生效，帧率Play中实时可调）")]
        [Tooltip("逻辑帧率（每秒推进tick数）：20≈设计标准，5≈慢动作细看击飞，60≈快节奏，200≈快进看结果")]
        [SerializeField] private int ticksPerSecond = 20;

        [Tooltip("A/B血量（重进Play生效）：调大笑看完整循环，调小快速进死亡流程")]
        [SerializeField] private int actorAHp = 1000;
        [SerializeField] private int actorBHp = 900;

        private float tickAccumulator;

        private BattleSystem battle;
        private int elapsedTicks;
        private bool finished;
        private bool charged;
        private bool countered;
        private bool fired;
        private readonly Dictionary<uint, BattleActorView> views = new Dictionary<uint, BattleActorView>();
        private readonly Dictionary<uint, ProjectileView> projectileViews = new Dictionary<uint, ProjectileView>();

        /// <summary>表现层轨道管理器（TrackViewStarted/Ended事件驱动，按逻辑轨道类型
        /// 建对应VTrack，与逻辑轨道实例同生共死）</summary>
        private VTrackManager vTrackManager;

        // Buff/技能/投射物定义不再手写：BattleXmlConfigLoader从XML配置装载进三个Library，
        // 按id取用（101破甲/102眩晕/104三环；技能1"雷霆一击"等，见StreamingAssets/BattleCore/battlecore.xml）

        private void Start()
        {
            bool ok = DeterminismChecker.RunCheck(out int divergeTick);
            Debug.Log(ok
                ? "[BattleDemo] 确定性自检通过：同种子两场战斗逐帧哈希一致"
                : $"[BattleDemo] 确定性自检失败：tick {divergeTick} 出现分歧！");

            StartBattle();
        }

        private void StartBattle()
        {
            battle = new BattleSystem();
            battle.Init(20261005u);

            // 从XML配置装载全部Buff/技能/投射物定义（内部Clear+Register，自检的手写源被替换为配置源）。
            // 三环/破甲/眩晕/技能从此改XML不改代码（也可用编辑器工具 Tools/BattleSystemCore/战斗配置编辑器 编辑）
            BattleXmlConfigLoader.Load();

            // 特效库注册内置程序化特效 + 表现层轨道管理器（TriggerFx等轨道的表现经VTrack落地）
            BattleFxLibrary.RegisterDefaults();
            vTrackManager = gameObject.GetComponent<VTrackManager>();
            if (vTrackManager == null)
            {
                vTrackManager = gameObject.AddComponent<VTrackManager>();
            }

            vTrackManager.Init(battle);

            var a = battle.actorManager.CreateActor();
            ConfigureFighter(a.actorContext, actorAHp, 120, 30, 3000, 15000);
            a.pos = new Vec3(-3000, 0, 0);
            var b = battle.actorManager.CreateActor();
            ConfigureFighter(b.actorContext, actorBHp, 100, 45, 2500, 18000);
            b.pos = new Vec3(3000, 0, 0);
            a.bodyRadius = BodyRadiusMilli;
            b.bodyRadius = BodyRadiusMilli;

            // 竞技场边界（写死常量）：把互追与冲锋都约束在场内
            battle.physics.Init(Vec3.zero, ArenaRadiusMilli);

            // 双方互相追踪（Chase模式：每帧动态取向，进射程内停步）
            a.movement.ChaseTarget(b.actorContext.UID, 100, AttackRange * FixMath.MilliPerUnit);
            b.movement.ChaseTarget(a.actorContext.UID, 100, AttackRange * FixMath.MilliPerUnit);

            // A自带三环：每3次普攻追加一击并2tick后回响（同tick第二条伤害行 + 回响行）
            a.buffs.ApplyBuff(BuffLibrary.Get(104));

            SetupPresentation(a, b);

            battle.eventQueue.Subscribe<DamageDealtEvent>(OnDamageDealt);
            battle.eventQueue.Subscribe<ActorDeadEvent>(OnActorDead);
            battle.eventQueue.Subscribe<BuffAppliedEvent>(OnBuffApplied);
            battle.eventQueue.Subscribe<BuffExpiredEvent>(OnBuffExpired);
            battle.eventQueue.Subscribe<ActorLandEvent>(OnActorLand);
            battle.eventQueue.Subscribe<ProjectileLaunchedEvent>(OnProjectileLaunched);
            battle.eventQueue.Subscribe<ProjectileHitEvent>(OnProjectileHit);
            battle.eventQueue.Subscribe<ProjectileExpiredEvent>(OnProjectileExpired);
            battle.eventQueue.Subscribe<TrackViewStartedEvent>(OnTrackViewStarted);
            battle.eventQueue.Subscribe<TrackViewEndedEvent>(OnTrackViewEnded);

            Debug.Log($"[BattleDemo] 战斗开始（{ticksPerSecond}tick/秒）：Actor#{a.actorContext.UID}(hp {actorAHp}, atk 120, def 30) VS Actor#{b.actorContext.UID}(hp {actorBHp}, atk 100, def 45)，相距6逻辑单位，射程{AttackRange}");
        }

        private void Update()
        {
            if (battle == null || finished)
            {
                return;
            }

            // 逻辑帧推进：ticksPerSecond决定游戏速度（wall-clock间隔，不影响确定性——tick内容不变）
            tickAccumulator += Time.deltaTime;
            float tickInterval = 1f / Mathf.Max(1, ticksPerSecond);
            while (tickAccumulator >= tickInterval && !finished)
            {
                tickAccumulator -= tickInterval;
                StepLogic();
            }
        }

        /// <summary>推进一个逻辑tick：先按剧本注入指令，再AdvanceTick</summary>
        private void StepLogic()
        {
            var actors = battle.actorManager.Actors;
            if (actors.Count == 2)
            {
                var a = actors[0];
                var b = actors[1];
                long distSq = Vec3.DistanceSq(a.pos, b.pos);
                bool inRange = distSq <= RangeSqMilli;

                if (!inRange)
                {
                    // 远程消耗：逼近中且间距≤4.5单位时A发射一发火球（技能4：弹体飞行，命中时爆炸载荷结算）
                    if (!fired && distSq <= 4500 * 4500)
                    {
                        fired = true;
                        SkillCaster.Cast(4, a.actorContext.UID, b.actorContext.UID);
                        Debug.Log($"[BattleDemo] tick {elapsedTicks} 间距 {(distSq / (float)(FixMath.MilliPerUnit * FixMath.MilliPerUnit)):F2}，Actor#{a.actorContext.UID} 发射[火球术]");
                    }

                    // 射程外：继续逼近，每秒（20tick）报一次间距
                    if (elapsedTicks % 20 == 0)
                    {
                        int dist = Vec3.Distance(a.pos, b.pos);
                        Debug.Log($"[BattleDemo] tick {elapsedTicks} 逼近中：间距 {dist / (float)FixMath.MilliPerUnit:F2} 逻辑单位");
                    }
                }
                else
                {
                    // 已入射程：Chase自带停止距离，无需手动停步；
                    // 首次入射程时A释放技能1"雷霆一击"（配表载荷组401：破甲+眩晕+AoE+击飞），同时冲锋过位——
                    // B眩晕且腾空期间意图移动暂停（无法追击），落地且眩晕解除后Chase自动掉头回位
                    if (!charged)
                    {
                        charged = true;
                        SkillCaster.Cast(1, a.actorContext.UID, b.actorContext.UID);
                        AddChargeAction(a.actorContext.UID, 2000, 5);
                        Debug.Log($"[BattleDemo] tick {elapsedTicks} 进入射程！Actor#{a.actorContext.UID} 释放技能[雷霆一击]并冲锋");
                    }

                    // 每10tick互相平A一次，命中定在下一tick
                    if (elapsedTicks % 10 == 0)
                    {
                        AddAttackAction(a.actorContext.UID, b.actorContext.UID);
                        AddAttackAction(b.actorContext.UID, a.actorContext.UID);
                    }

                    // B血量跌破一半时反打一次「旋风斩」：扇形伤害+给自己攻击强化（Owner语义），8tick后扇形二段
                    if (!countered && b.actorContext.hp * 2 < actorBHp)
                    {
                        countered = true;
                        SkillCaster.Cast(2, b.actorContext.UID, a.actorContext.UID);
                        Debug.Log($"[BattleDemo] tick {elapsedTicks} Actor#{b.actorContext.UID} 血量过半，反打[旋风斩]！");
                    }

                    // A每60tick循环释放「地裂」：矩形判定+6tick后单体补刀
                    if (charged && elapsedTicks % 60 == 0)
                    {
                        SkillCaster.Cast(3, a.actorContext.UID, b.actorContext.UID);
                        Debug.Log($"[BattleDemo] tick {elapsedTicks} Actor#{a.actorContext.UID} 释放[地裂]");
                    }
                }
            }

            battle.AdvanceTick();
            elapsedTicks++;

            // 兜底：伤害配置过肉导致打不死时强制结束
            if (elapsedTicks >= 3000)
            {
                finished = true;
                Debug.Log("[BattleDemo] 达到tick上限，战斗以平局结束");
            }
        }

        private void AddAttackAction(uint originId, uint targetId)
        {
            var action = new BaseAction
            {
                actionId = 1,
                name = "Attack",
            };
            var track = new DoDamageTick
            {
                startTick = battle.currTick + 1,
                endTick = battle.currTick + 1,
            };
            track.SetStaticData(new DoDamageStaticData
            {
                originActorId = originId,
                targetActorId = targetId,
            });
            action.tracks.Add(track);
            battle.actionManager.AddAction(action);
        }

        private void AddChargeAction(uint actorId, int deltaMilli, int durationTicks)
        {
            var action = new BaseAction
            {
                actionId = 2,
                name = "Charge",
            };
            var track = new MoveTrack
            {
                startTick = battle.currTick + 1,
                endTick = battle.currTick + durationTicks,
            };
            track.SetStaticData(new MoveStaticData
            {
                targetActorId = actorId,
                delta = new Vec3(deltaMilli, 0, 0),
            });
            action.tracks.Add(track);
            battle.actionManager.AddAction(action);
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

        /// <summary>
        /// 搭建表现层：地面/竞技场指示 + 为每个逻辑单位生成影子视图（Cube，逻辑pos为权威）。
        /// 视图为运行时动态创建，场景里无需预摆任何战斗物体。
        /// </summary>
        private void SetupPresentation(Actor a, Actor b)
        {
            var ground = GameObject.CreatePrimitive(PrimitiveType.Plane);
            ground.name = "ArenaGround";
            ground.transform.localScale = new Vector3(3f, 1f, 3f);   // 30×30单位，覆盖arena半径10
            ground.GetComponent<Renderer>().material.color = new Color(0.25f, 0.28f, 0.33f);

            CreateActorView(a, new Color(0.85f, 0.35f, 0.3f));       // A暖红
            CreateActorView(b, new Color(0.3f, 0.65f, 0.85f));       // B冷蓝
        }

        private void CreateActorView(Actor actor, Color color)
        {
            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"ActorView#{actor.actorContext.UID}";
            // 视觉方块尺寸=身体碰撞直径（0.6单位），视觉与碰撞体一致，半高0.3用于落地站立垫高
            float diameter = BodyRadiusMilli * 2f / FixMath.MilliPerUnit;
            go.transform.localScale = new Vector3(diameter, diameter, diameter);
            var view = go.AddComponent<BattleActorView>();
            view.Bind(actor, color, diameter * 0.5f);
            views.Add(actor.actorContext.UID, view);
        }

        private void OnDamageDealt(DamageDealtEvent evt)
        {
            // 受击闪白（纯表现，读事件不回写）
            if (views.TryGetValue(evt.targetActorId, out var hitView))
            {
                hitView.PlayHitFlash();
            }

            string crit = evt.isCrit ? " 暴击!" : string.Empty;
            Debug.Log($"[BattleDemo] tick {battle.currTick} Actor#{evt.originActorId} → Actor#{evt.targetActorId} 造成 {evt.damage} 伤害{crit}（剩余hp {evt.hpAfter}）");
        }

        private void OnActorDead(ActorDeadEvent evt)
        {
            finished = true;
            if (views.TryGetValue(evt.actorId, out var view))
            {
                Destroy(view.gameObject);
                views.Remove(evt.actorId);
            }

            var actors = battle.actorManager.Actors;
            string survivor = actors.Count > 0 ? $"胜者 Actor#{actors[0].actorContext.UID}" : "双方同归于尽";
            Debug.Log($"[BattleDemo] tick {battle.currTick} Actor#{evt.actorId} 阵亡，{survivor}，战斗结束（共 {elapsedTicks} tick）");
        }

        /// <summary>落地表现：视图纵压回弹（逻辑事件驱动，表现层只读不回写）</summary>
        private void OnActorLand(ActorLandEvent evt)
        {
            if (views.TryGetValue(evt.actorId, out var view))
            {
                view.PlayLandBounce();
            }

            Debug.Log($"[BattleDemo] tick {battle.currTick} Actor#{evt.actorId} 落地（腾空最高 {evt.peakHeight / (float)FixMath.MilliPerUnit:F2} 单位）");
        }

        /// <summary>弹体生成：创建子弹视图（小方块，追随逻辑弹体飞行）</summary>
        private void OnProjectileLaunched(ProjectileLaunchedEvent evt)
        {
            var p = battle.projectileManager.Get(evt.projectileUid);
            if (p == null)
            {
                return;
            }

            var go = GameObject.CreatePrimitive(PrimitiveType.Cube);
            go.name = $"Projectile#{evt.projectileUid}";
            go.transform.localScale = new Vector3(0.22f, 0.22f, 0.34f);
            var view = go.AddComponent<ProjectileView>();
            view.Bind(p, new Color(1f, 0.45f, 0.15f), 0.3f);
            projectileViews.Add(evt.projectileUid, view);
            Debug.Log($"[BattleDemo] tick {battle.currTick} Actor#{evt.ownerId} 发射投射物#{evt.projectileUid}（速度 {evt.vx},{evt.vy},{evt.vz} milli/tick）");
        }

        /// <summary>弹体命中：销毁视图 + 命中点快速收缩圈当爆炸视觉（载荷伤害另有DamageDealt日志）</summary>
        private void OnProjectileHit(ProjectileHitEvent evt)
        {
            DestroyProjectileView(evt.projectileUid);
            var go = new GameObject("ProjectileBlast");
            var blast = go.AddComponent<SkillCastIndicator>();
            blast.Init(1, 900, 0, 0, 0, new Vector2(evt.x, evt.z), Vector2.zero,
                new Color(1f, 0.5f, 0.2f), 0.06f, 0.22f);
            Debug.Log($"[BattleDemo] tick {battle.currTick} 投射物#{evt.projectileUid} 命中 Actor#{evt.targetActorId}，命中载荷结算");
        }

        /// <summary>弹体超程：销毁视图（落空弹消散）</summary>
        private void OnProjectileExpired(ProjectileExpiredEvent evt)
        {
            DestroyProjectileView(evt.projectileUid);
            Debug.Log($"[BattleDemo] tick {battle.currTick} 投射物#{evt.projectileUid} 超出射程消散");
        }

        private void DestroyProjectileView(uint projectileUid)
        {
            if (projectileViews.TryGetValue(projectileUid, out var view))
            {
                Destroy(view.gameObject);
                projectileViews.Remove(projectileUid);
            }
        }

        /// <summary>表现层轨道创建：转发管理器（按逻辑轨道类型建对应VTrack——
        /// TriggerFx建特效轨道、ControlBullet建导向标记轨道等，viewHandle配对销毁）</summary>
        private void OnTrackViewStarted(TrackViewStartedEvent evt)
        {
            vTrackManager.StartTrack(evt);
        }

        /// <summary>表现层轨道销毁：逻辑轨道播完/被打断，按句柄销毁对应VTrack</summary>
        private void OnTrackViewEnded(TrackViewEndedEvent evt)
        {
            vTrackManager.EndTrack(evt);
        }

        private void OnBuffApplied(BuffAppliedEvent evt)
        {
            Debug.Log($"[BattleDemo] tick {battle.currTick} Actor#{evt.actorId} 获得 Buff#{evt.buffId}（层数{evt.stacks}，剩余{evt.remainTicks}tick）");
        }

        private void OnBuffExpired(BuffExpiredEvent evt)
        {
            Debug.Log($"[BattleDemo] tick {battle.currTick} Actor#{evt.actorId} 的 Buff#{evt.buffId} 到期失效");
        }
    }
}
