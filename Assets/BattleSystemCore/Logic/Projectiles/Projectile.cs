using System.Collections.Generic;
using BattleSystemCore.Logic.Actors;
using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore.Logic.Projectiles
{
    /// <summary>
    /// 投射物运行时实体（非Actor：无属性/Buff/推挤语义，不参与AoE目标遍历——
    /// 弹只被命中判定不被打）。每tick pos+=velocity；
    /// 命中（水平圆判定，发射者豁免）或超程自毁，销毁延迟到帧末统一生效。
    /// 飞行参数为生成时解析值（模板值+SpawnBullet内联覆盖合成），不再持有模板引用——
    /// 覆盖参数不污染共享模板；payload仍为模板共享引用。
    /// velocity可被ControlBullet轨道重设（只改向不改速），traveled按speed累计。
    /// </summary>
    public class Projectile
    {
        /// <summary>投射物实例id（ProjectileManager独立分配，与Actor的uid空间互不相干）</summary>
        public uint uid { get; internal set; }

        /// <summary>发射者（命中豁免自身；载荷结算时的caster/owner；弹句柄的owner键）</summary>
        public uint ownerActorId { get; internal set; }

        /// <summary>命中载荷（TrackTemplate组，来自模板共享引用）：命中者=target，发射者=caster/owner</summary>
        public List<TrackTemplate> payload { get; internal set; }

        /// <summary>载荷伤害来源（技能弹=Skill，不广播普攻信号防递归）</summary>
        public DamageSource source { get; internal set; }

        /// <summary>弹句柄（SpawnBullet生成时写入，0=无名）：ControlBullet按(ownerActorId,label)操控</summary>
        public int label { get; internal set; }

        /// <summary>飞行速度（milli/tick，生成时解析。方向由velocity表达，操控只改向不改速）</summary>
        public int speedMilliPerTick { get; internal set; }

        /// <summary>命中判定半径（milli，与目标bodyRadius相加后做水平圆判定）</summary>
        public int hitRadiusMilli { get; internal set; }

        /// <summary>最大射程（milli）：累计飞行距离超出即失效自毁（防永动弹）</summary>
        public int maxRangeMilli { get; internal set; }

        /// <summary>当前位置（千分单位）。发射当帧起每tick移动一格velocity</summary>
        public Vec3 pos;

        /// <summary>每tick位移向量（milli/tick，方向与速率组装，ControlBullet可重设方向）</summary>
        public Vec3 velocity;

        /// <summary>累计飞行距离（milli）。超过maxRangeMilli即失效（操控改向不影响射程累计）</summary>
        public long traveledMilli;

        /// <summary>存活标记：命中/超程置false，帧末统一移除</summary>
        public bool alive = true;
    }
}
