using System;
using System.Collections.Generic;

namespace BattleSystemCore.Logic.Events
{
    /// <summary>
    /// 逻辑事件队列：帧内Post只收集，帧末Flush统一派发。
    /// 事件按Post顺序派发（同帧相对顺序确定）；handler内再Post的事件进入下一轮批处理，直到清空。
    /// 派发发生在帧末清理之后、下一帧逻辑之前，属逻辑与表现的安全边界，handler内不得回写战斗状态。
    /// </summary>
    public sealed class BattleEventQueue
    {
        private List<IBattleEvent> queued = new List<IBattleEvent>();
        private List<IBattleEvent> dispatching = new List<IBattleEvent>();
        private readonly Dictionary<Type, List<HandlerEntry>> handlers = new Dictionary<Type, List<HandlerEntry>>();

        private struct HandlerEntry
        {
            public object rawHandler;               // 原始Action<TEvent>，退订时按引用匹配
            public Action<IBattleEvent> invoke;     // 统一调用包装
        }

        /// <summary>帧内任意时刻投递事件，帧末Flush时派发</summary>
        public void Post<TEvent>(TEvent evt) where TEvent : struct, IBattleEvent
        {
            queued.Add(evt);
        }

        public void Subscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IBattleEvent
        {
            if (handler == null)
            {
                return;
            }

            var type = typeof(TEvent);
            if (!handlers.TryGetValue(type, out var list))
            {
                list = new List<HandlerEntry>();
                handlers.Add(type, list);
            }

            // 同一handler重复订阅只保留一份
            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i].rawHandler, handler))
                {
                    return;
                }
            }

            list.Add(new HandlerEntry
            {
                rawHandler = handler,
                invoke = e => handler((TEvent)e),
            });
        }

        public void Unsubscribe<TEvent>(Action<TEvent> handler) where TEvent : struct, IBattleEvent
        {
            if (handler == null || !handlers.TryGetValue(typeof(TEvent), out var list))
            {
                return;
            }

            for (int i = 0; i < list.Count; i++)
            {
                if (ReferenceEquals(list[i].rawHandler, handler))
                {
                    list.RemoveAt(i);
                    return;
                }
            }
        }

        /// <summary>帧末派发所有积压事件。由BattleSystem在LateUpdate末尾调用，宿主层不应直接调用</summary>
        public void Flush()
        {
            while (queued.Count > 0)
            {
                // 交换缓冲：handler内新Post的事件进入下一轮，本轮列表安全遍历
                (queued, dispatching) = (dispatching, queued);

                for (int i = 0; i < dispatching.Count; i++)
                {
                    var evt = dispatching[i];
                    if (handlers.TryGetValue(evt.GetType(), out var list))
                    {
                        for (int j = 0; j < list.Count; j++)
                        {
                            list[j].invoke(evt);
                        }
                    }
                }

                dispatching.Clear();
            }
        }

        /// <summary>清空积压与全部订阅（战斗重置时使用）</summary>
        public void Clear()
        {
            queued.Clear();
            dispatching.Clear();
            handlers.Clear();
        }
    }
}
