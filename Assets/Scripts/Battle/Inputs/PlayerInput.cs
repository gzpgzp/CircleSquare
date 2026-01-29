using System.Collections.Generic;
using UnityEngine;

namespace Battle.Inputs
{
    public enum InputPhase
    {
        None,
        Down,
        Hold,
        Up,
    }

    public class PlayerInput : BaseInput
    {
        public List<InputAction> inputs;
        

        public override float Horizontal
        {
            get => Input.GetAxisRaw("Horizontal");
        }

        public override float Vertical
        {
            get => Input.GetAxisRaw("Vertical");
        }

        public PlayerInput()
        {
            inputs = new List<InputAction>();
        }

        public InputAction AddInput(InputActionSO so)
        {
            var inputAction = new InputAction()
            {
                actionKey =  so.actionKey,
                inputPhase = so.inputPhase,
            };
            
            inputs.Add(inputAction);
            return inputAction;
        }

        public override void Update(float deltaTime)
        {
            foreach (var action in inputs)
            {
                if (Input.GetKeyDown(action.actionKey))
                {
                    action.InvokeDown();
                    action.inputPhase = InputPhase.Down;
                }

                if (Input.GetKey(action.actionKey))
                {
                    action.InvokeHold();
                    action.inputPhase = InputPhase.Hold;
                }

                if (Input.GetKeyUp(action.actionKey))
                {
                    action.InvokeUp();
                    action.inputPhase = InputPhase.Up;
                }
            }
        }
    }
}