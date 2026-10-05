using System;

namespace Battle3D.Character.Ability
{
    /// <summary>
    /// 能力标签 - 用于能力间的阻塞和互斥机制
    /// 使用 Flags 枚举支持组合
    /// </summary>
    [Flags]
    public enum AbilityTag
    {
        None = 0,

        /// <summary>动作类（攻击、技能释放动作）</summary>
        Action = 1 << 0,

        /// <summary>移动类（冲刺、闪避）</summary>
        Movement = 1 << 1,

        /// <summary>受击类（受击硬直、击退）</summary>
        Hurt = 1 << 2,

        /// <summary>控制类（眩晕、冰冻等控制效果）</summary>
        Control = 1 << 3,

        /// <summary>无敌类（i-frame无敌帧）</summary>
        Invincible = 1 << 4,

        /// <summary>跳跃/空中类</summary>
        Airborne = 1 << 5,
    }

    /// <summary>
    /// 技能槽位枚举 - 定义角色可用的技能位置
    /// </summary>
    public enum AbilitySlot
    {
        BasicAttack,    // 普攻
        Dodge,          // 闪避
        Skill1,         // 技能1
        Skill2,         // 技能2
        Ultimate,       // 终极技能
        Passive,        // 被动技能
    }

    /// <summary>
    /// 能力阶段枚举
    /// </summary>
    public enum AbilityPhase
    {
        None,       // 未激活
        Ready,      // 准备就绪
        Windup,     // 前摇
        Active,     // 生效中（攻击判定帧）
        Recovery,   // 后摇
        Cooldown,   // 冷却中
    }
}
