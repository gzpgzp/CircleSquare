using System;
using System.Collections.Generic;
using Battle.AI;
using UnityEngine;

namespace Battle.Character
{
    public class Enemy : BaseCharacter
    {
        [SerializeField] private List<StateEnum> states = new List<StateEnum>();
        private StateMachine machine;

        public void Start()
        {
            machine = new StateMachine();
            foreach (var id in states)
            {
                var state = StateRegistry.Create(id, this, machine);
                if (state != null)
                    machine.AddState(state);
            }

            machine.ChangeState(states[0]);
        }

        private void Update()
        {
            machine.Update(Time.deltaTime);
        }
    }
}