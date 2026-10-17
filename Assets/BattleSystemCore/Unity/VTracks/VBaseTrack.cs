using System;
using System.Collections.Generic;
using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore.Unity.VTracks
{
    /// <summary>
    /// 表现层轨道基类：逻辑轨道实例的影子（第三层影子：Actor→BattleActorView /
    /// Projectile→ProjectileView / Track实例→VTrack）。生命周期与逻辑轨道同生共死
    /// （TrackViewStarted创建、TrackViewEnded销毁），OnSync每渲染帧调——只读逻辑数据
    /// （经SharedStaticData/SharedDynamicData与各Manager查询），永不回写（表现层禁回写纪律）。
    /// </summary>
    public abstract class VBaseTrack
    {
        /// <summary>对应逻辑轨道句柄（VTrackManager表的key）</summary>
        protected uint viewHandle;

        /// <summary>影子对应的逻辑轨道引用（只读；由manager经TrackViewRegistry取得，
        /// 注册表帧末才清，生命周期内引用始终有效）</summary>
        protected BaseTrack logicTrack;

        /// <summary>逻辑战斗实例（只读查询用）</summary>
        protected BattleSystem battle;

        public void Bind(uint handle, BaseTrack logic, BattleSystem logicBattle)
        {
            viewHandle = handle;
            logicTrack = logic;
            battle = logicBattle;
        }

        /// <summary>逻辑轨道Enter（TrackViewStarted派发时调用一次）</summary>
        public abstract void OnEnter();

        /// <summary>每渲染帧同步（持续型表现每帧读逻辑数据驱动）</summary>
        public virtual void OnSync()
        {
        }

        /// <summary>逻辑轨道Exit/打断（TrackViewEnded派发时调用一次）——销毁本轨道全部表现物</summary>
        public virtual void OnExit()
        {
        }
    }

    /// <summary>
    /// 表现层轨道工厂：逻辑轨道类型→VTrack构造（与逻辑TrackFactory对仗的注册表）。
    /// 未注册的逻辑轨道类型不产生表现层轨道（逻辑侧照样跑，只是无表现）。
    /// 新增轨道有表现需求时：逻辑侧工厂RegisterViewTrack+Enter/Exit发信号，此处注册一项。
    /// </summary>
    public static class VTrackFactory
    {
        private static readonly Dictionary<Type, Func<VBaseTrack>> factories = new Dictionary<Type, Func<VBaseTrack>>();

        static VTrackFactory()
        {
            // TriggerFx两形态共用同一VTrack（瞬时/持续行为按logicTrack.isDuration区分）
            factories[typeof(TriggerFxTrack)] = CreateFxV;
            factories[typeof(TriggerFxTick)] = CreateFxV;
            factories[typeof(ControlBulletTrack)] = CreateControlV;
            factories[typeof(AttackShapeTick)] = CreateShapeV;
        }

        private static VBaseTrack CreateFxV()
        {
            return new TriggerFxVTrack();
        }

        private static VBaseTrack CreateControlV()
        {
            return new ControlBulletVTrack();
        }

        private static VBaseTrack CreateShapeV()
        {
            return new AttackShapeVTrack();
        }

        /// <summary>按逻辑轨道精确类型建对应VTrack（未注册返回null）</summary>
        public static VBaseTrack Create(BaseTrack logicTrack)
        {
            return factories.TryGetValue(logicTrack.GetType(), out var create) ? create() : null;
        }
    }
}
