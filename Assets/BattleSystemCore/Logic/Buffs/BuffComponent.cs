using System.Collections.Generic;
using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Events;
using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore.Logic.Buffs
{
    /// <summary>
    /// Buff容器（挂在Actor上）：管理当前生效的Buff实例，自主计时到期移除——
    /// 效果失效由"到期移除即失效"的聚合模型保证（实时遍历计算，无状态还原路径）。
    /// 施加动作（ApplyBuffTick）只是触发器，Buff生命周期独立于施放者的Action。
    /// 效果模型（组合模式）：属性修改(StatModifierEffect)走GetStat聚合；
    /// 攻击触发(AttackTriggerEffect)订阅逻辑事件总线的普攻命中信号，计数达阈值即同步执行触发动作。
    /// 聚合公式（单一属性，与条目顺序无关，确定）：(基础 + ΣFlat) * (10000 + ΣPercent) / 10000。
    /// </summary>
    public class BuffComponent
    {
        private readonly Actor owner;
        private readonly List<BuffInstance> buffs = new List<BuffInstance>();

        /// <summary>当前生效Buff（只读视图）。顺序即施加顺序，确定</summary>
        public IReadOnlyList<BuffInstance> Buffs => buffs;

        public BuffComponent(Actor actor)
        {
            owner = actor;
            // 订阅本战斗的逻辑事件总线（同步派发、可回写）：普攻命中信号驱动攻击触发型效果。
            // 总线随BattleSystem.Init重建，重开战斗时旧订阅随旧总线一起废弃
            BattleSystem.Current.logicBus.Subscribe<BasicAttackHitEvent>(OnBasicAttackHit);
        }

        /// <summary>是否被眩晕（任意生效中的眩晕buff，与层数无关）</summary>
        public bool IsStunned
        {
            get
            {
                for (int i = 0; i < buffs.Count; i++)
                {
                    if (buffs[i].data.isStun)
                    {
                        return true;
                    }
                }

                return false;
            }
        }

        /// <summary>
        /// 施加Buff：同buffId已存在则刷新剩余时长并叠层（至上限）；不存在则新建。
        /// 效果即时生效（下一次GetStat/信号即体现）。
        /// </summary>
        public void ApplyBuff(BuffData data)
        {
            if (data == null || data.durationTicks <= 0)
            {
                return;
            }

            for (int i = 0; i < buffs.Count; i++)
            {
                var existing = buffs[i];
                if (existing.data.buffId != data.buffId)
                {
                    continue;
                }

                existing.remainTicks = data.durationTicks;
                if (existing.stacks < data.maxStacks)
                {
                    existing.stacks++;
                }

                PostApplied(existing);
                return;
            }

            var instance = new BuffInstance
            {
                data = data,
                remainTicks = data.durationTicks,
                stacks = 1,
            };
            for (int i = 0; i < data.effects.Count; i++)
            {
                instance.effectStates.Add(0);
            }
            buffs.Add(instance);
            PostApplied(instance);
        }

        /// <summary>每帧计时：剩余tick归零的Buff移除（效果随之失效）。由Actor.Update驱动</summary>
        public void Update()
        {
            for (int i = buffs.Count - 1; i >= 0; i--)
            {
                buffs[i].remainTicks--;
                if (buffs[i].remainTicks <= 0)
                {
                    var expired = buffs[i];
                    buffs.RemoveAt(i);
                    BattleSystem.Current?.eventQueue.Post(new BuffExpiredEvent
                    {
                        actorId = owner.actorContext.UID,
                        buffId = expired.data.buffId,
                    });
                }
            }
        }

        /// <summary>聚合属性：当前生效值=(base+ΣFlat)*(10000+ΣPercent)/10000，负数钳制为0</summary>
        public int GetStat(StatType type, int baseValue)
        {
            int flat = 0;
            int percent = 0;
            for (int i = 0; i < buffs.Count; i++)
            {
                var buff = buffs[i];
                var effects = buff.data.effects;
                for (int e = 0; e < effects.Count; e++)
                {
                    if (effects[e] is StatModifierEffect statMod && statMod.statType == type)
                    {
                        // 每层各贡献一份
                        for (int layer = 0; layer < buff.stacks; layer++)
                        {
                            if (statMod.modType == ModifierType.Flat)
                            {
                                flat += statMod.value;
                            }
                            else
                            {
                                percent += statMod.value;
                            }
                        }
                    }
                }
            }

            long value = (long)(baseValue + flat) * (10000 + percent) / 10000;
            if (value < 0)
            {
                value = 0;
            }

            return (int)value;
        }

        /// <summary>
        /// 普攻命中信号处理：仅处理本Actor为攻击者的信号。遍历各buff的攻击触发型效果，
        /// 计数达everyN即触发并清零（计数状态写回effectStates，参与状态哈希）。
        /// 触发在本调用栈内执行（与普攻同帧同序）：payload模板经TrackPayloadRunner实例化——
        /// 当帧瞬时模板同帧结算、延迟/持续模板入时间轴；伤害来源固定BuffEffect不广播普攻信号，
        /// 否则会再次计数造成无限递归。onTrigger（程序侧逃生口）在payload之后执行。
        /// </summary>
        private void OnBasicAttackHit(BasicAttackHitEvent evt)
        {
            if (evt.attackerId != owner.actorContext.UID)
            {
                return;
            }

            var battle = BattleSystem.Current;
            for (int b = 0; b < buffs.Count; b++)
            {
                var buff = buffs[b];
                var effects = buff.data.effects;
                for (int e = 0; e < effects.Count; e++)
                {
                    if (!(effects[e] is AttackTriggerEffect trigger)
                        || trigger.everyN <= 0
                        || (trigger.payload.Count == 0 && trigger.onTrigger == null))
                    {
                        continue;
                    }

                    int count = buff.effectStates[e] + 1;
                    if (count >= trigger.everyN)
                    {
                        buff.effectStates[e] = 0;
                        if (trigger.payload.Count > 0)
                        {
                            var context = new TrackContext
                            {
                                casterId = evt.attackerId,
                                targetId = evt.targetId,
                                ownerId = owner.actorContext.UID,
                                damageSource = DamageSource.BuffEffect,
                            };
                            TrackPayloadRunner.Run(trigger.payload, context, battle.currTick);
                        }

                        trigger.onTrigger?.Invoke(battle, evt.attackerId, evt.targetId);
                    }
                    else
                    {
                        buff.effectStates[e] = count;
                    }
                }
            }
        }

        private void PostApplied(BuffInstance instance)
        {
            BattleSystem.Current?.eventQueue.Post(new BuffAppliedEvent
            {
                actorId = owner.actorContext.UID,
                buffId = instance.data.buffId,
                stacks = instance.stacks,
                remainTicks = instance.remainTicks,
            });
        }
    }
}
