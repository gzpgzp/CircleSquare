namespace BattleSystemCore.Logic.Buffs
{
    /// <summary>
    /// 可被Buff修改的属性类型。Hp/Mp为"当前值"语义（受伤害/消耗改变），
    /// 不参与Buff聚合；上限类Buff（如maxHp）待后续扩展。
    /// </summary>
    public enum StatType
    {
        Atk,
        Def,
        /// <summary>移动速度：聚合基准为MovementComponent的标量速度</summary>
        Speed,
        /// <summary>暴击率（万分比）</summary>
        CritRate,
        /// <summary>暴击伤害倍率（万分比）</summary>
        CritMultiplier,
    }
}
