using System;
using Battle.Character.Ability;
using UnityEngine;

namespace Battle.Inputs
{
    [CreateAssetMenu(menuName = "Input/Input Action")]
    public class InputActionSO : ScriptableObject
    {
        public AbilityEnum abilityEnum;
        public string actionName;
        public KeyCode actionKey;
        public InputPhase inputPhase;
    }

    public class InputAction
    {
        public KeyCode actionKey;
        public InputPhase inputPhase;
        
        private event Action onDown;
        private event Action onUp;
        private event Action onHold;

        public void RegisterDown(Action action)
        {
            onDown += action;
        }

        public void RegisterUp(Action action)
        {
            onUp += action;
        }

        public void RegisterHold(Action action)
        {
            onHold += action;
        }

        public void RemoveDown(Action action)
        {
            onDown -= action;
        }

        public void RemoveUp(Action action)
        {
            onUp -= action;
        }

        public void RemoveHold(Action action)
        {
            onHold -= action;
        }

        public void InvokeUp() => onUp?.Invoke();
        public void InvokeDown() => onDown?.Invoke();
        public void InvokeHold() => onHold?.Invoke();
    }
}