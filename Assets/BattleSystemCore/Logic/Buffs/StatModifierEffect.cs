namespace BattleSystemCore.Logic.Buffs
{
    /// <summary>修改类型：Flat=直接加减；Percent=万分比（-2000=-20%），作用于(基础+ΣFlat之和)</summary>
    public enum ModifierType
    {
        Flat,
        Percent,
    }

    /// <summary>
    /// 属性修改效果：每层各贡献一份。
    /// Flat=直接加减；Percent=万分比（-2000=-20%），作用于(基础+ΣFlat)。
    /// </summary>
    public class StatModifierEffect : BuffEffect
    {
        public StatType statType;
        public ModifierType modType;
        public int value;
    }
}
