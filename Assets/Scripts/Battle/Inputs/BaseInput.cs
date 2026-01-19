using System;
using UnityEngine;

namespace Battle.Inputs
{
    public enum InputType
    {
        None,
        Key,
    }

    public class BaseInput : MonoBehaviour
    {
        protected InputType inputType;
        private Action inputAction;
        
        public InputType InputType
        {
            get => inputType;
        }

        public void InitAction(Action inputAction)
        {
            this.inputAction = inputAction;
        }

        protected void OnTrigger()
        {
            inputAction?.Invoke();
        }
    }
}