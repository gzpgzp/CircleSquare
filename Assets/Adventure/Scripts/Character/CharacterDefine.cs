namespace Adventure.Character
{
    /// <summary>
    /// 属性类型
    /// </summary>
    public enum StatType
    {
        MaxHp,
        Atk,
        Def,
        Spd,        // 攻击速度（每秒攻击次数）
        CritRate,   // 暴击率 0~1
        CritDmg,    // 暴击伤害倍率（如 1.5 = 150%）
    }

    /// <summary>
    /// 角色职业/类型
    /// </summary>
    public enum CharacterClass
    {
        Warrior,
        Mage,
        Archer,
        Healer
    }

    /// <summary>
    /// 角色阵营
    /// </summary>
    public enum CharacterCamp
    {
        Player,
        Enemy
    }

    /// <summary>
    /// 战斗状态
    /// </summary>
    public enum CombatState
    {
        Idle,
        Fighting,
        Dead
    }
}
