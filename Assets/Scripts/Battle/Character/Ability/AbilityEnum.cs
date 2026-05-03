namespace Battle.Character.Ability
{
    public enum AbilityEnum
    {
        None = 0,
        HorizontalMove2D = 10,
        Dash = 20,
        Hurt = 30,
        Jump = 40,
        MeleeAttack2D = 50,   // AI 近战连段攻击
        RangedAttack2D = 60,  // AI 远程投射攻击
        MultiRangedAttack2D = 65,  // AI 多子弹远程投射攻击
        AutoMove2D = 70,      // AI 自动向敌人移动（Archer/DogDun）
        DriftMove2D = 80,     // Car 醉酒漂移移动
        ShieldOrbit2D = 90,   // DogDun 盾牌绕轨
        CriticalHit = 100,     // 暴击修饰器
    }
}