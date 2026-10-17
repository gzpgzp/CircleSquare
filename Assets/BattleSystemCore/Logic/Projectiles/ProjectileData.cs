using System.Collections.Generic;
using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore.Logic.Projectiles
{
    /// <summary>
    /// 投射物静态定义（照SkillData模式）：弹体参数+命中载荷组。
    /// 弹=会飞的载荷载体：命中时把payload交给TrackPayloadRunner结算，
    /// 载荷可用全部轨道词汇（伤害/上Buff/形状攻击=爆炸AoE/击飞/再发射=弹中弹）。
    /// 配置化链路：Luban Projectile表→Loader→ProjectileLibrary。
    /// </summary>
    public class ProjectileData
    {
        public uint projectileId;

        public string name;

        /// <summary>飞行速度（milli/tick，正值。方向由发射时刻的origin→target决定）</summary>
        public int speedMilliPerTick;

        /// <summary>命中判定半径（milli，与目标bodyRadius相加后做水平圆判定）</summary>
        public int hitRadiusMilli;

        /// <summary>最大射程（milli）：累计飞行距离超出即失效自毁（防永动弹）</summary>
        public int maxRangeMilli;

        /// <summary>命中载荷（TrackTemplate组）：命中者=target，发射者=caster/owner</summary>
        public List<TrackTemplate> payload = new List<TrackTemplate>();
    }
}
