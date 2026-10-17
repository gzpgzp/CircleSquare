using System.Collections.Generic;

namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>
    /// 活跃表现轨道注册表：viewHandle→逻辑轨道引用。表现层轨道按句柄取逻辑轨道
    /// 引用读数据容器（staticData/dynamicData）——事件只发纯信号，数据走引用。
    /// 工厂实例化有表现需求的轨道时注册，轨道Exit请求移除，移除延迟到帧末事件
    /// Flush之后统一生效（照帧内请求帧末生效惯例）——瞬时轨道同帧Enter+Exit，
    /// 事件派发时数据必须仍可读。纯表现结构，不进状态哈希。
    /// </summary>
    public class TrackViewRegistry
    {
        private readonly Dictionary<uint, BaseTrack> active = new Dictionary<uint, BaseTrack>();
        private readonly List<uint> pendingRemove = new List<uint>();

        /// <summary>注册（工厂分配句柄后调用；handle=0为无表现轨道，忽略）</summary>
        public void Register(uint handle, BaseTrack track)
        {
            if (handle != 0)
            {
                active[handle] = track;
            }
        }

        /// <summary>按句柄取逻辑轨道引用（不存在/无效句柄返回null）</summary>
        public BaseTrack Get(uint handle)
        {
            return handle != 0 && active.TryGetValue(handle, out var track) ? track : null;
        }

        /// <summary>请求移除（轨道Exit时）：帧末事件Flush之后统一生效</summary>
        public void RequestRemove(uint handle)
        {
            if (handle != 0 && active.ContainsKey(handle) && !pendingRemove.Contains(handle))
            {
                pendingRemove.Add(handle);
            }
        }

        /// <summary>帧末事件派发完毕后调用（BattleSystem.LateUpdate内Flush之后）：真正清表</summary>
        public void LateFlushRemove()
        {
            for (int i = 0; i < pendingRemove.Count; i++)
            {
                active.Remove(pendingRemove[i]);
            }

            pendingRemove.Clear();
        }

        public void Init()
        {
            active.Clear();
            pendingRemove.Clear();
        }
    }
}
