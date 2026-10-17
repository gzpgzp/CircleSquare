using BattleSystemCore.Logic;
using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Events;
using BattleSystemCore.Logic.Physics;
using BattleSystemCore.Logic.Projectiles;
using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore
{
    /// <summary>
    /// 战斗逻辑核心。纯C#实现，不依赖Unity，可脱离引擎独立运行与单元测试。
    /// 一场战斗对应一个实例，由外部宿主（如Unity侧的BattleDriver）以固定步长调用AdvanceTick推进。
    /// 当前活跃实例可通过Current访问，供Track等功能代码获取上下文。
    /// </summary>
    public class BattleSystem
    {
        /// <summary>当前活跃的战斗实例。注意：同一时刻只应有一个活跃战斗，多实例并行（如回放）需自行管理切换。</summary>
        public static BattleSystem Current { get; private set; }

        public ActionManager actionManager { get; private set; }
        public ActorManager actorManager { get; private set; }

        /// <summary>投射物管理器：弹体生成/飞行积分/命中判定/超程自毁（tick中段执行，与Actor同帧序）</summary>
        public ProjectileManager projectileManager { get; private set; }

        /// <summary>逻辑事件队列：帧内Post收集，帧末Flush派发给表现层订阅者</summary>
        public BattleEventQueue eventQueue { get; private set; }

        /// <summary>逻辑事件总线：Post立即同步派发，handler可回写战斗状态（Buff触发等）。
        /// 与表现层eventQueue严格区分，重建于Init</summary>
        public LogicEventBus logicBus { get; private set; }

        /// <summary>确定性随机数（种子注入）。所有逻辑判定必须经由它，暴击等掷骰全在此处</summary>
        public BattleRandom random { get; private set; }

        /// <summary>战斗物理世界：单位推挤与竞技场边界（tick末执行，位置修正直接写pos进状态哈希）</summary>
        public PhysicsWorld physics { get; private set; }

        /// <summary>当前逻辑帧号，从0开始，每调用一次AdvanceTick自增1</summary>
        public int currTick { get; private set; }

        /// <summary>表现层轨道句柄分配器（有表现需求的轨道工厂实例化时领取，每次唯一递增）。
        /// 纯表现信号不进状态哈希；双跑各自独立分配，同输入序列下分配序一致，事件序列确定</summary>
        public uint nextViewHandle;

        /// <summary>活跃表现轨道注册表：viewHandle→逻辑轨道。表现层轨道经它取逻辑轨道
        /// 引用读数据容器（事件只发纯信号，数据走引用）；重建于Init</summary>
        public TrackViewRegistry trackViewRegistry { get; private set; }

        public void Init(uint seed = 1)
        {
            actionManager = new ActionManager();
            actorManager = new ActorManager();
            projectileManager = new ProjectileManager();
            eventQueue = new BattleEventQueue();
            logicBus = new LogicEventBus();
            random = new BattleRandom(seed);
            physics = new PhysicsWorld();
            actionManager.Init();
            actorManager.Init();
            projectileManager.Init();
            currTick = 0;
            nextViewHandle = 1;
            trackViewRegistry = new TrackViewRegistry();
            Current = this;
        }

        /// <summary>
        /// 推进一个逻辑帧。帧内固定顺序（确定性保证，不可调整）：
        /// 轨道Enter → 轨道Process → Actor更新 → 轨道Exit → 帧末清理。
        /// 帧内产生的增删请求统一延迟到帧末清理阶段生效。
        /// </summary>
        public void AdvanceTick()
        {
            UpdateLogicPartA();
            UpdateLogicPartB();
            LateUpdate();
            currTick++;
        }

        private void UpdateLogicPartA()
        {
            actionManager.UpdateFrontPart(currTick);
        }

        private void UpdateLogicPartB()
        {
            actorManager.Update(currTick);

            // 弹体飞行与命中：意图移动之后（读到移动后位置）、轨道位移与物理之前。
            // 命中载荷同帧结算（其中瞬时轨道直接Enter），弹中弹只登记不递归结算
            projectileManager.Update(currTick);

            actionManager.UpdateBackPart(currTick);
            // 全部移动（意图移动+位移轨道）结束后统一物理：推挤分离→边界钳制，
            // 本tick结束位置互不重叠且在场地内，下一tick的所有判定读到合规位置
            physics.Update(currTick);
        }

        private void LateUpdate()
        {
            actionManager.LateUpdate(currTick);
            actorManager.LateUpdate(currTick);
            projectileManager.LateUpdate(currTick);
            // 逻辑帧彻底结束（增删清理完毕）后，一次性把帧内积压事件派发给表现层；
            // 表现层在派发中可能按句柄读注册表，放最后清表——瞬时轨道同帧Enter+Exit，
            // 派发时数据必须仍可读
            eventQueue.Flush();
            trackViewRegistry.LateFlushRemove();
        }

        /// <summary>
        /// 当前战斗状态的确定性哈希（FNV-1a）：帧号、随机数状态、Action与Actor核心状态。
        /// 同输入的两场战斗若每帧哈希一致即确定性成立。用于回归自检与回放校验。
        /// </summary>
        public int GetStateHash()
        {
            uint hash = 2166136261u;
            hash = HashCombine(hash, (uint)currTick);
            hash = HashCombine(hash, random.State);

            var actions = actionManager.Actions;
            hash = HashCombine(hash, (uint)actions.Count);
            for (int i = 0; i < actions.Count; i++)
            {
                var action = actions[i];
                if (action == null)
                {
                    hash = HashCombine(hash, 0u);
                    continue;
                }

                hash = HashCombine(hash, (uint)action.actionId);
                hash = HashCombine(hash, (uint)action.EndTick);
            }

            var actors = actorManager.Actors;
            hash = HashCombine(hash, (uint)actors.Count);
            for (int i = 0; i < actors.Count; i++)
            {
                var ctx = actors[i].actorContext;
                hash = HashCombine(hash, ctx.UID);
                hash = HashCombine(hash, (uint)ctx.hp);
                hash = HashCombine(hash, (uint)ctx.mp);
                hash = HashCombine(hash, (uint)actors[i].pos.x);
                hash = HashCombine(hash, (uint)actors[i].pos.y);
                hash = HashCombine(hash, (uint)actors[i].pos.z);

                // 垂直运动状态：腾空中抛物线参数决定后续轨迹，须并入哈希
                // （地面时仅记0；落地后y=0，与起飞前等价，无需额外区分）
                var vertical = actors[i].vertical;
                hash = HashCombine(hash, vertical.airborne ? 1u : 0u);
                if (vertical.airborne)
                {
                    hash = HashCombine(hash, (uint)vertical.StartTick);
                    hash = HashCombine(hash, (uint)vertical.Vy0);
                    hash = HashCombine(hash, (uint)vertical.Gravity);
                    hash = HashCombine(hash, (uint)vertical.StartY);
                }

                // Buff状态：id/层数/剩余时长，Buff路径的确定性由此覆盖
                var buffs = actors[i].buffs.Buffs;
                hash = HashCombine(hash, (uint)buffs.Count);
                for (int b = 0; b < buffs.Count; b++)
                {
                    hash = HashCombine(hash, buffs[b].data.buffId);
                    hash = HashCombine(hash, (uint)buffs[b].stacks);
                    hash = HashCombine(hash, (uint)buffs[b].remainTicks);
                    // 效果计数状态（三环计数等）：触发路径的确定性由此覆盖
                    var states = buffs[b].effectStates;
                    for (int s = 0; s < states.Count; s++)
                    {
                        hash = HashCombine(hash, (uint)states[s]);
                    }
                }
            }

            var projectiles = projectileManager.Projectiles;
            hash = HashCombine(hash, (uint)projectiles.Count);
            for (int i = 0; i < projectiles.Count; i++)
            {
                // 弹体轨迹影响后续命中帧，位置/速度/射程累计均须入哈希；
                // 生成时解析的判定参数（速度/半径/射程）也入哈希——内联覆盖可产生
                // 同pos/velocity但判定参数不同的弹，避免哈希盲区
                var p = projectiles[i];
                hash = HashCombine(hash, p.uid);
                hash = HashCombine(hash, (uint)p.pos.x);
                hash = HashCombine(hash, (uint)p.pos.y);
                hash = HashCombine(hash, (uint)p.pos.z);
                hash = HashCombine(hash, (uint)p.velocity.x);
                hash = HashCombine(hash, (uint)p.velocity.y);
                hash = HashCombine(hash, (uint)p.velocity.z);
                hash = HashCombine(hash, (uint)p.traveledMilli);
                hash = HashCombine(hash, (uint)p.speedMilliPerTick);
                hash = HashCombine(hash, (uint)p.hitRadiusMilli);
                hash = HashCombine(hash, (uint)p.maxRangeMilli);
            }

            return unchecked((int)hash);
        }

        private static uint HashCombine(uint hash, uint value)
        {
            return (hash ^ value) * 16777619u;
        }
    }
}
