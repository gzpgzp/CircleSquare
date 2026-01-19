using System;
using UnityEngine;

namespace Battle.Inputs
{
    public class KeyboardInput : BaseInput
    {
        protected string keyBoard = "";

        public KeyboardInput()
        {
            inputType = InputType.Key;
        }

        public void Update()
        {
            if (keyBoard != "" && Input.GetKey(keyBoard))
            {
                OnTrigger();
            }
        }
    }
}