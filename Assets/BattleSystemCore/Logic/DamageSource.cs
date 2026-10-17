namespace BattleSystemCore.Logic
{
    /// <summary>
    /// 伤害来源。只有BasicAttack会广播普攻命中信号（BasicAttackHitEvent）——
    /// BuffEffect来源的追加伤害不得再触发（防三环套三环的无限递归）。
    /// </summary>
    public enum DamageSource
    {
        /// <summary>普攻：广播命中信号，攻击触发型Buff计数</summary>
        BasicAttack,

        /// <summary>技能伤害：不广播命中信号</summary>
        Skill,

        /// <summary>Buff触发效果产生的伤害（三环追加、日炎灼烧等）：不广播命中信号</summary>
        BuffEffect,
    }
}
