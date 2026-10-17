using System.Collections.Generic;
using BattleSystemCore.Logic.Actors;

namespace BattleSystemCore.Logic
{
    /// <summary>
    /// 参与者管理器。确定性遍历依赖actors列表的插入顺序，actorMap仅用于按id点查。
    /// 移除请求统一延迟到帧末生效，避免帧内遍历时增删。
    /// </summary>
    public class ActorManager
    {
        private uint nextUId = 1;
        private readonly List<Actor> actors = new List<Actor>();
        private readonly Dictionary<uint, Actor> actorMap = new Dictionary<uint, Actor>();
        private readonly List<uint> pendingRemove = new List<uint>();

        /// <summary>全部参与者（只读视图）。遍历顺序即创建顺序，确定</summary>
        public IReadOnlyList<Actor> Actors => actors;

        public void Init()
        {
            nextUId = 1;
            actors.Clear();
            actorMap.Clear();
            pendingRemove.Clear();
        }

        /// <summary>创建参与者并分配确定性自增id。id从1开始，0保留为无效值</summary>
        public Actor CreateActor()
        {
            var actor = new Actor();
            actor.OnCreate(nextUId++);
            actors.Add(actor);
            actorMap.Add(actor.actorContext.UID, actor);
            actor.Init();
            return actor;
        }

        public Actor GetActor(uint id)
        {
            return actorMap.TryGetValue(id, out var actor) ? actor : null;
        }

        /// <summary>请求移除参与者，帧末统一生效</summary>
        public void RemoveActor(uint id)
        {
            if (!pendingRemove.Contains(id))
            {
                pendingRemove.Add(id);
            }
        }

        public void Update(int currTick)
        {
            // 按创建顺序遍历（确定性），驱动Actor级更新（速度积分等）
            for (int i = 0; i < actors.Count; i++)
            {
                actors[i]?.Update(currTick);
            }
        }

        /// <summary>帧末清理：应用延迟移除请求</summary>
        public void LateUpdate(int currTick)
        {
            for (int i = 0; i < pendingRemove.Count; i++)
            {
                var id = pendingRemove[i];
                if (actorMap.TryGetValue(id, out var actor))
                {
                    actorMap.Remove(id);
                    actors.Remove(actor);
                }
            }
            pendingRemove.Clear();
        }
    }
}
