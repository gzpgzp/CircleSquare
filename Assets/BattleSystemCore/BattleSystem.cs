using System;
using System.Collections;
using System.Collections.Generic;
using BattleSystemCore.Logic;
using UnityEngine;

namespace BattleSystemCore
{
    public class BattleSystem : MonoBehaviour
    {
        private ActionManager actionManager;
        private ActorManager actorManager;

        private void Init()
        {
            actionManager.Init();
            actorManager.Init();
        }

        private void Update()
        {
            actionManager.Update();
            actorManager.Update();
        }
    }
}
