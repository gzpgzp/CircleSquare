using System.Collections.Generic;
using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Events;
using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore.Logic.Projectiles
{
    /// <summary>
    /// 投射物管理器（照ActorManager模式）：确定性列表按spawn序遍历，
    /// id点查，销毁请求延迟到帧末生效。
    /// 每tick顺序（确定）：位移积分 → 命中判定（actors创建序，首个命中者结算并销毁）→ 超程结算+自毁。
    /// 命中/超程载荷均经TrackPayloadRunner同帧结算并携带弹着点impactPos（命中=接触点、超程=落点）。
    /// 命中载荷relTick=0瞬时轨道与命中同一调用栈，"命中当帧生效"；
    /// 另维护弹句柄表：(ownerActorId, label)→弹列表，SpawnBullet注册、ControlBullet查询——
    /// 挂在Manager而非Action，因为技能载荷的relTick=0瞬时轨道走同帧结算路径无Action宿主。
    /// </summary>
    public class ProjectileManager
    {
        private uint nextUId = 1;
        private readonly List<Projectile> projectiles = new List<Projectile>();
        private readonly Dictionary<uint, Projectile> projectileMap = new Dictionary<uint, Projectile>();
        private readonly List<uint> pendingRemove = new List<uint>();
        private readonly Dictionary<(uint ownerActorId, int label), List<Projectile>> labelMap =
            new Dictionary<(uint ownerActorId, int label), List<Projectile>>();

        /// <summary>全部存活投射物（只读视图）。遍历顺序即spawn顺序，确定</summary>
        public IReadOnlyList<Projectile> Projectiles => projectiles;

        public void Init()
        {
            nextUId = 1;
            projectiles.Clear();
            projectileMap.Clear();
            pendingRemove.Clear();
            labelMap.Clear();
        }

        /// <summary>
        /// 生成一发投射物。direction为单位向量（DirectionTo风格，分量长约1000）；
        /// 飞行参数为生成时解析值（模板值+内联覆盖已由调用方合成）：
        /// velocity = direction * speed / 1000（整除向零取整，确定性成立）。
        /// 出生位置=发射者当前位置（y随发射者，平飞不变；命中判定只看水平面，与HitTest惯例一致）。
        /// label>0时注册句柄表（同一键可多弹共存，操控按spawn序作用）；弹死亡时由帧末清理剔除。
        /// </summary>
        public Projectile Spawn(
            uint ownerActorId,
            Vec3 origin,
            Vec3 direction,
            int speedMilliPerTick,
            int hitRadiusMilli,
            int maxRangeMilli,
            List<TrackTemplate> payload,
            DamageSource source,
            int label)
        {
            var p = new Projectile
            {
                uid = nextUId++,
                ownerActorId = ownerActorId,
                payload = payload,
                source = source,
                label = label,
                speedMilliPerTick = speedMilliPerTick,
                hitRadiusMilli = hitRadiusMilli,
                maxRangeMilli = maxRangeMilli,
                pos = origin,
                velocity = direction * speedMilliPerTick / 1000,
            };
            projectiles.Add(p);
            projectileMap.Add(p.uid, p);
            if (label > 0)
            {
                var key = (ownerActorId, label);
                if (!labelMap.TryGetValue(key, out var list))
                {
                    list = new List<Projectile>();
                    labelMap.Add(key, list);
                }

                list.Add(p);
            }

            return p;
        }

        /// <summary>按句柄取该owner名下的存活弹（spawn序，确定）。空列表=无弹可操控</summary>
        public List<Projectile> GetByLabel(uint ownerActorId, int label)
        {
            var result = new List<Projectile>();
            if (label > 0 && labelMap.TryGetValue((ownerActorId, label), out var list))
            {
                for (int i = 0; i < list.Count; i++)
                {
                    if (list[i] != null && list[i].alive)
                    {
                        result.Add(list[i]);
                    }
                }
            }

            return result;
        }

        public Projectile Get(uint uid)
        {
            return projectileMap.TryGetValue(uid, out var p) ? p : null;
        }

        /// <summary>请求销毁投射物，帧末统一生效</summary>
        public void Remove(uint uid)
        {
            if (!pendingRemove.Contains(uid))
            {
                pendingRemove.Add(uid);
            }
        }

        public void Update(int currTick)
        {
            var battle = BattleSystem.Current;
            if (battle == null)
            {
                return;
            }

            for (int i = 0; i < projectiles.Count; i++)
            {
                var p = projectiles[i];
                if (p == null || !p.alive)
                {
                    continue;
                }

                // 位移积分：先移动后判定，"飞到即命中"（操控轨道已在前半帧重设过方向）
                p.pos += p.velocity;
                p.traveledMilli += p.speedMilliPerTick;

                // 命中判定：actors创建序遍历（确定），发射者与尸体豁免；
                // 水平圆判定 distSq <= (hitRadius + bodyRadius)^2，与HitTest同风格（y不参与）
                uint hitTargetId = 0;
                for (int a = 0; a < battle.actorManager.Actors.Count; a++)
                {
                    var target = battle.actorManager.Actors[a];
                    if (target.actorContext.UID == p.ownerActorId || target.actorContext.hp <= 0)
                    {
                        continue;
                    }

                    long dx = p.pos.x - target.pos.x;
                    long dz = p.pos.z - target.pos.z;
                    long radius = p.hitRadiusMilli + target.bodyRadius;
                    if (dx * dx + dz * dz > radius * radius)
                    {
                        continue;
                    }

                    hitTargetId = target.actorContext.UID;
                    break;
                }

                if (hitTargetId != 0)
                {
                    // 命中载荷：同帧结算（relTick=0瞬时轨道与命中同一调用栈）。
                    // caster/owner=发射者、target=命中者；来源沿用弹的source（不广播普攻信号防递归）
                    TrackPayloadRunner.Run(p.payload, new TrackContext
                    {
                        casterId = p.ownerActorId,
                        targetId = hitTargetId,
                        ownerId = p.ownerActorId,
                        damageSource = p.source,
                        impactPos = p.pos,
                        hasImpactPos = true,
                    }, currTick);

                    battle.eventQueue.Post(new ProjectileHitEvent
                    {
                        projectileUid = p.uid,
                        ownerId = p.ownerActorId,
                        targetActorId = hitTargetId,
                        x = p.pos.x,
                        y = p.pos.y,
                        z = p.pos.z,
                    });

                    p.alive = false;
                    Remove(p.uid);
                    continue;
                }

                // 超程结算+自毁：累计飞行距离超出射程（落空弹）。结算走落点爆语义——
                // 弹着点=最终飞行位置，targetId=0（单体载荷自然空转），坐标型载荷
                // （AttackShape shapeCenterMode=ImpactPoint）在落点起爆；无载荷弹零行为变化
                if (p.traveledMilli >= p.maxRangeMilli)
                {
                    TrackPayloadRunner.Run(p.payload, new TrackContext
                    {
                        casterId = p.ownerActorId,
                        targetId = 0,
                        ownerId = p.ownerActorId,
                        damageSource = p.source,
                        impactPos = p.pos,
                        hasImpactPos = true,
                    }, currTick);

                    battle.eventQueue.Post(new ProjectileExpiredEvent
                    {
                        projectileUid = p.uid,
                        x = p.pos.x,
                        y = p.pos.y,
                        z = p.pos.z,
                    });

                    p.alive = false;
                    Remove(p.uid);
                }
            }
        }

        /// <summary>帧末清理：应用延迟销毁请求，并同步剔除句柄表中的死弹引用</summary>
        public void LateUpdate(int currTick)
        {
            for (int i = 0; i < pendingRemove.Count; i++)
            {
                var id = pendingRemove[i];
                if (projectileMap.TryGetValue(id, out var p))
                {
                    projectileMap.Remove(id);
                    projectiles.Remove(p);
                    if (p.label > 0)
                    {
                        if (labelMap.TryGetValue((p.ownerActorId, p.label), out var list))
                        {
                            list.Remove(p);
                            if (list.Count == 0)
                            {
                                labelMap.Remove((p.ownerActorId, p.label));
                            }
                        }
                    }
                }
            }

            pendingRemove.Clear();
        }
    }
}
