using System;
using System.Collections.Generic;
using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore.Logic.Buffs
{
    /// <summary>
    /// 攻击触发效果：每everyN次普攻命中触发一次载荷（计数状态存BuffInstance.effectStates）。
    /// 触发与层数无关（层数只放大属性类效果）。
    /// payload为模板载荷（可序列化，配置驱动主通道）：当帧瞬时模板同帧结算（保住同帧语义），
    /// 延迟/持续模板走Action时间轴；RoleRef语义角色在触发时刻解析成具体id。
    /// onTrigger为程序侧逃生口（特殊逻辑直接写代码，无法进配表），优先使用payload。
    /// 纪律：触发载荷伤害来源固定BuffEffect（不再广播普攻信号，防无限递归）。
    /// </summary>
    public class AttackTriggerEffect : BuffEffect
    {
        /// <summary>每N次普攻命中触发（N≥1）</summary>
        public int everyN = 1;

        /// <summary>触发载荷：轨道模板组（与技能共用的效果词汇，TrackFactory实例化）</summary>
        public List<TrackTemplate> payload = new List<TrackTemplate>();

        /// <summary>触发动作(battle, attackerId, targetId)。程序侧逃生口，在payload之后执行</summary>
        public Action<BattleSystem, uint, uint> onTrigger;
    }
}
