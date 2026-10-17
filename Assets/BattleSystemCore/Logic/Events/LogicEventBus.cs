using System;
using System.Collections.Generic;

namespace BattleSystemCore.Logic.Events
{
    /// <summary>
    /// 逻辑层事件总线：Post立即同步派发（区别于表现层BattleEventQueue的帧末批量派发）。
    /// 用途：逻辑内部信号（普攻命中等）广播给Buff触发器等回写型订阅者。
    /// 确定性：订阅顺序=注册顺序（Actor创建顺序确定），派发按订阅顺序遍历。
    /// 纪律：handler内禁止(退)订阅本总线（遍历中变更列表）；可安全AddAction等（走pending缓冲）。
    /// </summary>
    public sealed class LogicEventBus
    {
        private readonly Dictionary<Type, List<Delegate>> handlers = new Dictionary<Type, List<Delegate>>();

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, ILogicEvent
        {
            if (handler == null)
            {
                return;
            }

            var type = typeof(TEvent);
            if (!handlers.TryGetValue(type, out var list))
            {
                list = new List<Delegate>();
                handlers.Add(type, list);
            }

            // 同一handler重复订阅只保留一份
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], handler))
                {
                    return;
                }
            }

            list.Add(handler);
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, ILogicEvent
        {
            if (handler == null || !handlers.TryGetValue(typeof(TEvent), out var list))
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i], handler))
                {
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>清空全部订阅（战斗实例销毁/重置时使用）</summary>
        public void Clear()
        {
            handlers.Clear();
        }

        /// <summary>立即同步派发（按订阅顺序）。handler内不得(退)订阅本总线</summary>
        public void Post<TEvent>(TEvent evt) where TEvent : struct, ILogicEvent
        {
            if (!handlers.TryGetValue(typeof(TEvent), out var list))
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                ((Action<TEvent>)list[i])(evt);
            }
        }
    }
}
