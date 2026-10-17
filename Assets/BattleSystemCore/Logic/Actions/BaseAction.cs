using System.Collections.Generic;
using BattleSystemCore.Logic.Tracks;

namespace BattleSystemCore.Logic.Actions
{
    /// <summary>
    /// 行动：一次完整的技能/行为单元，由多条轨道按各自的时间区间并行编排而成。
    /// 所有轨道Exit完毕后由ActionManager在帧末自动移除；也可通过RemoveAction打断。
    /// </summary>
    public class BaseAction
    {
        public int actionId;
        public string name;
        public List<BaseTrack> tracks = new List<BaseTrack>();

        /// <summary>Action整体结束tick：所有启用轨道中最晚的endTick</summary>
        public int EndTick
        {
            get
            {
                int max = int.MinValue;
                for (int i = 0; i < tracks.Count; i++)
                {
                    var track = tracks[i];
                    if (track != null && track.enable && track.endTick > max)
                    {
                        max = track.endTick;
                    }
                }

                return max;
            }
        }

        /// <summary>所有轨道是否已播完（无轨道视为已结束）</summary>
        public bool IsFinished(int currTick)
        {
            return tracks.Count == 0 || currTick >= EndTick;
        }

        /// <summary>打断：对所有已Enter未Exit的轨道强制Exit（用于眩晕、死亡等中断场景）</summary>
        public void Interrupt()
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                if (track == null)
                {
                    continue;
                }

                if (track.entered && !track.exited)
                {
                    track.exited = true;
                    track.Exit();
                }
            }
        }

        /// <summary>前半帧：startTick到达的轨道进入</summary>
        public void UpdateFrontPart(int currTick)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                if (track == null || !track.enable)
                {
                    continue;
                }

                if (track.startTick == currTick)
                {
                    track.entered = true;
                    track.Enter();
                }
            }
        }

        /// <summary>前半帧：区间覆盖currTick的持续轨道每帧执行</summary>
        public void Update(int currTick)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                if (track == null || !track.enable)
                {
                    continue;
                }

                if (track.isDuration && track.startTick <= currTick && track.endTick >= currTick)
                {
                    (track as DurationTrack)?.Process();
                }
            }
        }

        /// <summary>后半帧：endTick到达的轨道退出。Exit不受enable拦截——已进入的轨道必须保证退出</summary>
        public void UpdateBackPart(int currTick)
        {
            for (int i = 0; i < tracks.Count; i++)
            {
                var track = tracks[i];
                if (track == null)
                {
                    continue;
                }

                if (track.endTick == currTick)
                {
                    track.exited = true;
                    track.Exit();
                }
            }
        }
    }
}
