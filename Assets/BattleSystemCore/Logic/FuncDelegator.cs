using System.Collections.Generic;
using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Buffs;
using BattleSystemCore.Logic.Events;
using BattleSystemCore.Logic.Physics;
using BattleSystemCore.Logic.Projectiles;
using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore.Logic
{
    /// <summary>
    /// 战斗功能函数集：伤害结算等纯逻辑功能入口。
    /// 全程整数运算，掷骰经由BattleSystem.random，保证确定性。
    /// </summary>
    public static class FuncDelegator
    {
        /// <summary>
        /// 一次攻击结算：基础伤害=max(1, atk-def)；万分比暴击判定；
        /// 暴击伤害=damage*criticalMultiplier/10000。
        /// 目标hp归零时投递死亡事件并进入延迟移除队列。
        /// source=BasicAttack时经logicBus同步广播普攻命中信号（驱动攻击触发型Buff，如三环）；
        /// BuffEffect来源的追加伤害不得用BasicAttack（防触发递归）。
        /// </summary>
        public static void DoAttack(Actor attacker, Actor target, DamageSource source = DamageSource.BasicAttack)
        {
            var battle = BattleSystem.Current;
            if (battle == null || attacker == null || target == null)
            {
                return;
            }

            var atkCtx = attacker.actorContext;
            var defCtx = target.actorContext;

            // 全部读聚合值（基础值+Buff修改），Buff对伤害的影响由此生效
            int atk = attacker.GetStat(StatType.Atk);
            int def = target.GetStat(StatType.Def);
            int critRate = attacker.GetStat(StatType.CritRate);
            int critMultiplier = attacker.GetStat(StatType.CritMultiplier);

            // 基础伤害：攻击减防御，最低保底1
            int damage = atk - def;
            if (damage < 1)
            {
                damage = 1;
            }

            // 暴击判定：critRate为万分比（10000=100%）
            bool isCrit = battle.random.Roll(critRate);
            if (isCrit && critMultiplier > 0)
            {
                // 中间用long防乘法溢出，结果一致地截断回int
                damage = (int)((long)damage * critMultiplier / 10000);
            }

            int hpAfter = target.TakeDamage(damage);

            battle.eventQueue.Post(new DamageDealtEvent
            {
                originActorId = atkCtx.UID,
                targetActorId = defCtx.UID,
                damage = damage,
                isCrit = isCrit,
                hpAfter = hpAfter,
            });

            if (hpAfter <= 0)
            {
                battle.eventQueue.Post(new ActorDeadEvent { actorId = defCtx.UID });
                battle.actorManager.RemoveActor(defCtx.UID);
            }

            // 普攻命中信号（同步派发）：三环等攻击触发型Buff在此计数与触发；
            // 触发产生的追加伤害（BuffEffect来源）不再广播，天然无递归
            if (source == DamageSource.BasicAttack)
            {
                battle.logicBus.Post(new BasicAttackHitEvent
                {
                    attackerId = atkCtx.UID,
                    targetId = defCtx.UID,
                    damage = damage,
                    isCrit = isCrit,
                    hpAfter = hpAfter,
                });
            }
        }

        /// <summary>
        /// 区域攻击结算：以attacker为origin按shape做命中判定，命中者逐个走DoAttack。
        /// center可选覆盖判定心（弹着点：命中接触点/超程落点）——attacker仍承担伤害来源与自身豁免。
        /// primaryTarget为普攻主目标保底（走位脱判仍吃到，动作游戏惯例）；null则纯形状判定（技能AoE）。
        /// 确定性：命中集合按actorManager创建序收集与结算；每个命中者独立掷骰与广播普攻信号
        /// （多目标普攻=多次命中计数，MOBA惯例）。attacker自身与已死亡单位不参与判定。
        /// </summary>
        public static void DoAttackArea(
            Actor attacker,
            Vec3 forward,
            in AttackShape shape,
            Actor primaryTarget = null,
            DamageSource source = DamageSource.BasicAttack,
            Vec3? center = null)
        {
            var battle = BattleSystem.Current;
            if (battle == null || attacker == null)
            {
                return;
            }

            Vec3 hitCenter = center ?? attacker.pos;
            List<Actor> hits = null;
            var actors = battle.actorManager.Actors;
            for (int i = 0; i < actors.Count; i++)
            {
                var target = actors[i];
                if (target == attacker || target.actorContext.hp <= 0)
                {
                    continue;
                }

                if (!HitTest.HitShape(shape, hitCenter, forward, target.pos, target.bodyRadius))
                {
                    continue;
                }

                hits ??= new List<Actor>();
                hits.Add(target);
            }

            // 主目标保底：指定了但未进形状（闪避距离）仍命中
            if (primaryTarget != null
                && primaryTarget != attacker
                && primaryTarget.actorContext.hp > 0
                && (hits == null || !hits.Contains(primaryTarget)))
            {
                hits ??= new List<Actor>();
                hits.Add(primaryTarget);
            }

            if (hits == null)
            {
                return;
            }

            for (int i = 0; i < hits.Count; i++)
            {
                DoAttack(attacker, hits[i], source);
            }
        }

        /// <summary>
        /// 击飞：目标以垂直初速抛起，经解析抛物线回落，落地发ActorLandEvent（双总线）。
        /// 腾空期间目标意图移动暂停（眩晕同款）、命中判定照常（水平面判定，空中仍可被打）。
        /// 水平击退请另行配MoveTrack位移轨道组合成"抛飞"；死亡目标不击飞。
        /// 空中目标被再击飞=重置抛物线（空中接力）。
        /// </summary>
        public static void DoKnockup(Actor target, int vy0Milli, int gravityMilli)
        {
            if (target == null || target.actorContext.hp <= 0)
            {
                return;
            }

            target.vertical.Launch(vy0Milli, gravityMilli);
        }

        /// <summary>
        /// 生成子弹：以origin当前位置为出生点、direction为单位方向生成弹体（交ProjectileManager飞行），
        /// 飞行参数为生成时解析值（模板值+内联覆盖已由调用方合成）；payload为命中载荷模板组共享引用；
        /// label>0时注册句柄供ControlBullet操控。帧末广播ProjectileLaunchedEvent（表现层创建子弹视图）。
        /// 命中判定与载荷结算由弹体自身负责。
        /// </summary>
        public static void DoSpawnBullet(
            Actor origin,
            Vec3 direction,
            int speedMilliPerTick,
            int hitRadiusMilli,
            int maxRangeMilli,
            List<TrackTemplate> payload,
            DamageSource source,
            int label)
        {
            var battle = BattleSystem.Current;
            if (battle == null || origin == null || speedMilliPerTick <= 0)
            {
                return;
            }

            var p = battle.projectileManager.Spawn(
                origin.actorContext.UID, origin.pos, direction,
                speedMilliPerTick, hitRadiusMilli, maxRangeMilli,
                payload ?? new List<TrackTemplate>(), source, label);
            battle.eventQueue.Post(new ProjectileLaunchedEvent
            {
                projectileUid = p.uid,
                ownerId = p.ownerActorId,
                x = p.pos.x,
                y = p.pos.y,
                z = p.pos.z,
                vx = p.velocity.x,
                vy = p.velocity.y,
                vz = p.velocity.z,
            });
        }
    }
}
