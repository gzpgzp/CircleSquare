

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Battle.Character.Ability;

namespace Battle.Character
{
    public static class CharacterInitializer
    {
        private static Dictionary<AbilityEnum, Type> types;

        // 
        public static void Initializer()
        {
            types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => t.IsSubclassOf(typeof(BaseAbility)))
                .Select(t => new
                {
                    Type = t,
                    Attr = t.GetCustomAttribute<AbilityAttribute>()
                })
                .Where(x => x.Attr != null)
                .ToDictionary(x => x.Attr.Id, x => x.Type);
        }

        public static BaseAbility CreateAbility(int abilityId)
        {
            return null;
        }

        public static BaseAbility CreateAbility(AbilityEnum abilityEnum)
        {
            if (!types.TryGetValue(abilityEnum, out var type))
                throw new Exception($"Ability {abilityEnum} not registered!");
        
            return (BaseAbility)Activator.CreateInstance(type);
        }
    }
}