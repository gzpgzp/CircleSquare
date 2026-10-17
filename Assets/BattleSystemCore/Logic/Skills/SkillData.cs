using System.Collections.Generic;
using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore.Logic.Skills
{
    /// <summary>
    /// 技能静态定义：id、名字与载荷轨道模板组。技能=时间轴效果组合（伤害/形状判定/击飞/位移/Buff），
    /// 施放时经TrackFactory按模板实例化，模板复用TrackTemplate词汇（技能与Buff触发共用）。
    /// 全局只读共享（与BuffData同定位）。
    /// </summary>
    public class SkillData
    {
        public uint skillId;
        public string name;

        /// <summary>技能载荷：轨道模板组（组内顺序即模板列表顺序）</summary>
        public List<TrackTemplate> payload = new List<TrackTemplate>();
    }
}
