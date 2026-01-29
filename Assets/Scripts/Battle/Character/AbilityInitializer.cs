

using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Battle.Character.Ability;
using Battle.Inputs;
using GameFramework;
using Tools.ResourcesTool;

namespace Battle.Character
{
    public static class AbilityInitializer
    {
        private static Dictionary<AbilityEnum, Type> types;
        private static Dictionary<AbilityEnum, InputActionSO> abilityToConfig;

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

            abilityToConfig = new Dictionary<AbilityEnum, InputActionSO>();
            var so = MyResourcesManager.Instance.LoadAll<InputActionSO>("Inputs");
            for (int i = 0; i < so.Length; i++)
            {
                abilityToConfig[so[i].abilityEnum] = so[i];
            }
        }

        public static BaseAbility CreateAbility(int abilityId)
        {
            return CreateAbility(ConfigManager.Instance.GetAbilityById(abilityId));
        }

        public static BaseAbility CreateAbility(AbilityEnum abilityEnum)
        {
            if (!types.TryGetValue(abilityEnum, out var type))
                throw new Exception($"Ability {abilityEnum} not registered!");
        
            return (BaseAbility)Activator.CreateInstance(type);
        }

        public static InputActionSO GetAbilityInputConfig(int abilityId)
        {
            if (abilityToConfig.TryGetValue(ConfigManager.Instance.GetAbilityById(abilityId), out var config))
            {
                return config;
            }
            return null;
        }
    }
}