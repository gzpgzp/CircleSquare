using System.Collections.Generic;
using BattleSystemCore.Logic.Actions;

namespace BattleSystemCore.Logic.Tracks
{
    public class BaseTrack
    {
        private List<BaseAction> actions = new List<BaseAction>();

        public void Update()
        {
            for (int i = 0; i < actions.Count; i++)
            {
                actions[i].Update();
            }
        }
    }
}