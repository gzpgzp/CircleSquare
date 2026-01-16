using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Battle.Character;
using UnityEngine;

namespace Battle.AI
{
    public static class StateRegistry
    {
        private static Dictionary<StateEnum,Type> stateTypes = new Dictionary<StateEnum,Type>();

        static StateRegistry()
        {
            stateTypes = new Dictionary<StateEnum, Type>();

            var baseType = typeof(BaseState);

            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t));

            foreach (var type in types)
            {
                var temp = (BaseState)Activator.CreateInstance(
                    type,
                    new object[] { null, null }
                );

                stateTypes[temp.Id] = type;
            }
        }
        
        public static BaseState Create(
            StateEnum id,
            BaseCharacter character,
            StateMachine machine)
        {
            if (!stateTypes.TryGetValue(id, out var type))
            {
                Debug.LogError($"State {id} not registered");
                return null;
            }

            return (BaseState)Activator.CreateInstance(
                type,
                character,
                machine
            );
        }
    }
}