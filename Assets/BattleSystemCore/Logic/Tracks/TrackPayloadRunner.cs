using System.Collections.Generic;
using BattleSystemCore.Logic.Actions;

namespace BattleSystemCore.Logic.Tracks
{
    /// <summary>
    /// 触发载荷执行器：在触发时刻执行一组轨道模板，按执行时序分流——
    /// ①触发当帧的瞬时模板（relTick=0的TickTrack）：同帧Enter结算，与触发同一调用栈，
    ///   保住"第N次普攻追加伤害"的同帧语义（晚一帧就变成两次独立攻击）；
    /// ②延迟/持续模板：实例化为BaseAction进入时间轴。帧内触发时入轨Action要到下一tick才参与
    ///   UpdateFrontPart检查（且其按startTick==currTick精确匹配），startTick早于currTick+1的
    ///   轨道整体顺延一帧起跳（时长不变）；帧外（如输入层）触发当帧即可命中，不顺延。
    /// context.damageSource由调用方按触发性质注入（Buff触发=BuffEffect、技能=Skill），防触发递归。
    /// </summary>
    public static class TrackPayloadRunner
    {
        /// <summary>触发载荷Action的actionId（与Checker/Demo手写Action的1/2/3错开，便于日志与哈希区分）</summary>
        public const int PayloadActionId = 4;

        /// <summary>
        /// 执行载荷模板组。baseTick为触发基准帧（模板relTick的锚点，一般传battle.currTick）。
        /// </summary>
        public static void Run(List<TrackTemplate> payload, TrackContext context, int baseTick)
        {
            if (payload == null || payload.Count == 0)
            {
                return;
            }

            var battle = BattleSystem.Current;
            if (battle == null)
            {
                return;
            }

            BaseAction action = null;
            bool inFrame = battle.actionManager.IsUpdating;
            int minStartTick = battle.currTick + 1;
            for (int i = 0; i < payload.Count; i++)
            {
                // 轨道索引随循环填入上下文（TriggerFx等信号轨道透传给表现层做定位）。
                // context为struct值拷贝参数，改的是本方法内副本，调用方原件不受影响
                context.trackIndex = i;
                var track = TrackFactory.Create(payload[i], context, baseTick);
            
                if (track.startTick == baseTick && !track.isDuration)
                {
                    // 当帧瞬时轨道：直接结算，不进时间轴（同帧语义）
                    track.entered = true;
                    track.Enter();
                    track.exited = true;
                    track.Exit();
                    continue;
                }
            
                // 入轨轨道：帧内触发时（缓冲入轨、下一tick才参与检查）startTick早于
                // currTick+1的轨道整体顺延一帧起跳（时长不变）；帧外触发当帧即可命中，不顺延
                if (inFrame)
                {
                    int shift = minStartTick - track.startTick;
                    if (shift > 0)
                    {
                        track.startTick += shift;
                        track.endTick += shift;
                    }
                }

                if (action == null)
                {
                    action = new BaseAction
                    {
                        actionId = PayloadActionId,
                        name = "TriggerPayload",
                    };
                }

                action.tracks.Add(track);
            }

            if (action != null)
            {
                battle.actionManager.AddAction(action);
            }
        }
    }
}
