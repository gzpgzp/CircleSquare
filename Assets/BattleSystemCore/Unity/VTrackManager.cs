using System.Collections.Generic;
using BattleSystemCore.Logic;
using BattleSystemCore.Logic.Events;
using BattleSystemCore.Unity.VTracks;
using UnityEngine;

namespace BattleSystemCore.Unity
{
    /// <summary>
    /// 表现层轨道管理器：订阅TrackViewStarted/Ended，按句柄经TrackViewRegistry取逻辑
    /// 轨道引用，VTrackFactory按逻辑轨道类型建对应表现层轨道（一一对应），统一驱动
    /// OnEnter/OnSync/OnExit。VTrack与逻辑轨道实例同生共死（Enter建、Exit/打断销），
    /// 期间只读逻辑数据——永不回写（表现层禁回写纪律）。
    /// </summary>
    public class VTrackManager : MonoBehaviour
    {
        private readonly Dictionary<uint, VBaseTrack> tracks = new Dictionary<uint, VBaseTrack>();
        private BattleSystem battle;

        /// <summary>绑定逻辑战斗实例（事件转发自BattleDemo，其持有battle引用）</summary>
        public void Init(BattleSystem logicBattle)
        {
            battle = logicBattle;
        }

        /// <summary>TrackViewStartedEvent处理：按逻辑轨道类型建对应VTrack并OnEnter
        /// （查不到注册表引用/未注册类型则跳过）</summary>
        public void StartTrack(TrackViewStartedEvent evt)
        {
            if (battle == null || tracks.ContainsKey(evt.viewHandle))
            {
                return;
            }

            var logic = battle.trackViewRegistry.Get(evt.viewHandle);
            if (logic == null)
            {
                return; // 理论不可达：注册表帧末才清，Flush派发时必在
            }

            var v = VTrackFactory.Create(logic);
            if (v == null)
            {
                return;
            }

            v.Bind(evt.viewHandle, logic, battle);
            v.OnEnter();
            tracks[evt.viewHandle] = v;
        }

        /// <summary>TrackViewEndedEvent处理：销毁对应VTrack（逻辑轨道播完/打断）</summary>
        public void EndTrack(TrackViewEndedEvent evt)
        {
            if (tracks.Remove(evt.viewHandle, out var v))
            {
                v.OnExit();
            }
        }

        /// <summary>清空全部VTrack（战斗重置用；Ended漏发时的兜底）</summary>
        public void Clear()
        {
            foreach (var pair in tracks)
            {
                pair.Value.OnExit();
            }

            tracks.Clear();
        }

        private void Update()
        {
            // 遍历字典值（OnSync内部不增删），逐轨道同步表现
            foreach (var pair in tracks)
            {
                pair.Value.OnSync();
            }
        }
    }
}
