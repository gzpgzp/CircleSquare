using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore.Logic.Skills
{
    /// <summary>
    /// 技能施放入口：skillId→载荷模板组实例化执行。
    /// caster/target为语义角色锚点（模板内RoleRef相对它们解析）；damageSource=Skill
    /// （技能伤害不广播普攻信号，与触发载荷同规则，防递归）。
    /// 施放时刻由调用方保证（Demo脚本/未来输入层/帧同步指令层），本类不含冷却与消耗判定（属技能系统上层）。
    /// </summary>
    public static class SkillCaster
    {
        /// <summary>施放技能。未注册的skillId返回false（调用方自行处理）</summary>
        public static bool Cast(uint skillId, uint casterId, uint targetId)
        {
            var skill = SkillLibrary.Get(skillId);
            var battle = BattleSystem.Current;
            if (skill == null || battle == null)
            {
                return false;
            }

            var context = new TrackContext
            {
                casterId = casterId,
                targetId = targetId,
                ownerId = casterId,
                damageSource = DamageSource.Skill,
            };
            TrackPayloadRunner.Run(skill.payload, context, battle.currTick);
            return true;
        }
    }
}
