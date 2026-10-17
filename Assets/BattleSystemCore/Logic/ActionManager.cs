using System.Collections.Generic;
using BattleSystemCore.Logic.Actions;

namespace BattleSystemCore.Logic
{
    /// <summary>
    /// 行动管理器：维护当前活跃的Action列表并按帧驱动它们。
    /// 帧内（Enter/Process/Exit执行期间）产生的增删请求进入缓冲队列，帧末统一生效，
    /// 保证遍历安全与确定性：帧内新增的Action从下一tick开始参与检查。
    /// 在AdvanceTick之外（如宿主处理输入阶段）调用AddAction则立即生效，当帧即可命中Enter。
    /// </summary>
    public class ActionManager
    {
        private readonly List<BaseAction> currActions = new List<BaseAction>();
        private readonly List<BaseAction> pendingAdd = new List<BaseAction>();
        private readonly List<BaseAction> pendingRemove = new List<BaseAction>();
        private bool isUpdating;

        /// <summary>当前活跃Action列表（只读视图）。遍历顺序即执行顺序，确定</summary>
        public IReadOnlyList<BaseAction> Actions => currActions;

        /// <summary>是否正处在AdvanceTick帧内（Enter/Process/Exit执行期间）。帧内AddAction走缓冲，下一tick才生效</summary>
        public bool IsUpdating => isUpdating;

        public void Init()
        {
            currActions.Clear();
            pendingAdd.Clear();
            pendingRemove.Clear();
            isUpdating = false;
        }

        public void AddAction(BaseAction action)
        {
            if (action == null)
            {
                return;
            }

            if (isUpdating)
            {
                pendingAdd.Add(action);
            }
            else
            {
                currActions.Add(action);
            }
        }

        /// <summary>
        /// 请求移除一个Action，视为打断：已Enter未Exit的轨道会被强制Exit。
        /// 帧内调用时延迟到帧末生效。
        /// </summary>
        public void RemoveAction(BaseAction action)
        {
            if (action == null)
            {
                return;
            }

            if (isUpdating)
            {
                if (!pendingRemove.Contains(action))
                {
                    pendingRemove.Add(action);
                }
            }
            else
            {
                action.Interrupt();
                currActions.Remove(action);
            }
        }

        /// <summary>前半帧：先处理所有轨道的Enter，再处理所有持续轨道的Process</summary>
        public void UpdateFrontPart(int currTick)
        {
            isUpdating = true;
            for (int i = 0; i < currActions.Count; i++)
            {
                currActions[i]?.UpdateFrontPart(currTick);
            }

            for (int i = 0; i < currActions.Count; i++)
            {
                currActions[i]?.Update(currTick);
            }
        }

        /// <summary>后半帧：处理所有轨道的Exit</summary>
        public void UpdateBackPart(int currTick)
        {
            for (int i = 0; i < currActions.Count; i++)
            {
                currActions[i]?.UpdateBackPart(currTick);
            }
        }

        /// <summary>帧末清理：移除已结束的Action、应用缓冲的增删请求。必须在Exit阶段之后调用</summary>
        public void LateUpdate(int currTick)
        {
            // 自然结束的Action（所有轨道已播完）
            for (int i = currActions.Count - 1; i >= 0; i--)
            {
                var action = currActions[i];
                if (action == null || action.IsFinished(currTick))
                {
                    currActions.RemoveAt(i);
                }
            }

            // 请求移除的Action（打断语义）
            for (int i = 0; i < pendingRemove.Count; i++)
            {
                var action = pendingRemove[i];
                if (currActions.Remove(action))
                {
                    action.Interrupt();
                }
            }
            pendingRemove.Clear();

            // 帧内新增的Action并入，下一tick开始参与检查
            for (int i = 0; i < pendingAdd.Count; i++)
            {
                currActions.Add(pendingAdd[i]);
            }
            pendingAdd.Clear();

            isUpdating = false;
        }
    }
}
