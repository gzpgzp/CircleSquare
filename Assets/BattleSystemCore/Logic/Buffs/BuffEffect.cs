namespace BattleSystemCore.Logic.Buffs
{
    /// <summary>
    /// Buff效果基类（组合模式）：一个Buff由任意多个效果组成。
    /// 属性修改（StatModifierEffect）、攻击触发（AttackTriggerEffect）等均为子类实现，
    /// 新效果类型=新子类，BuffData保持数据组合结构。配置驱动阶段效果子类字段直接映射配表。
    /// </summary>
    public abstract class BuffEffect
    {
    }
}
