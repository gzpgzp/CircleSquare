using BattleSystemCore.Logic.Actors;

namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>
    /// 触发上下文：触发时刻由调用方（BuffComponent触发/技能施放层）构造，
    /// 把RoleRef语义角色解析成具体actorId，并携带本上下文的伤害来源。
    /// 全值类型（纯struct），构造后视为只读快照。
    /// </summary>
    public struct TrackContext
    {
        /// <summary>触发者id：普攻命中事件的攻击者 / 技能施放者</summary>
        public uint casterId;

        /// <summary>作用目标id：被打的人 / 技能目标</summary>
        public uint targetId;

        /// <summary>Buff持有者id（技能场景等于caster）</summary>
        public uint ownerId;

        /// <summary>本上下文载荷的伤害来源：Buff触发固定BuffEffect、技能层用Skill——
        /// 均不广播普攻命中信号，递归在类型层面被切断；禁止填BasicAttack（会再次触发攻击型Buff）</summary>
        public DamageSource damageSource;

        /// <summary>弹着点（弹命中/超程瞬间的子弹当前位置）。仅弹的payload结算上下文携带：
        /// ProjectileManager命中时填接触点、超程时填落点，其他触发源为false。
        /// AttackShape的shapeCenterMode=ImpactPoint时作为判定心（无弹着点时工厂回退origin角色）</summary>
        public Vec3 impactPos;

        /// <summary>impactPos是否有效（Vec3为struct无法用null表达缺席）</summary>
        public bool hasImpactPos;

        /// <summary>本次触发载荷内的轨道索引（TrackPayloadRunner循环填充，仅供TriggerFx等
        /// 信号轨道透传给表现层做调试/编辑器联动定位；非Runner创建的轨道保持默认0）</summary>
        public int trackIndex;

        /// <summary>语义角色→具体id。无效角色返回0（GetActor(0)为null，轨道自行空转）</summary>
        public uint Resolve(RoleRef role)
        {
            switch (role)
            {
                case RoleRef.Caster: return casterId;
                case RoleRef.Target: return targetId;
                case RoleRef.Owner: return ownerId;
                default: return 0;
            }
        }
    }
}
