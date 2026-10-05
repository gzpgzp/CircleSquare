using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEngine;

namespace Battle3D.Character.Ability
{
    /// <summary>
    /// 能力属性标记 - 用于反射自动注册
    /// 标记在 BaseAbility 子类上，指定其唯一标识
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public class AbilityIdAttribute : Attribute
    {
        public string Id { get; }

        public AbilityIdAttribute(string id)
        {
            Id = id;
        }
    }

    /// <summary>
    /// 能力初始化器 - 使用反射自动发现并注册所有 BaseAbility 子类
    /// 参考现有项目 AbilityInitializer 的设计
    /// </summary>
    public static class AbilityInitializer
    {
        private static Dictionary<string, Type> abilityTypes;
        private static bool isInitialized = false;

        /// <summary>
        /// 初始化 - 扫描程序集注册所有能力类型
        /// </summary>
        public static void Initialize()
        {
            if (isInitialized) return;

            abilityTypes = new Dictionary<string, Type>();

            var baseType = typeof(BaseAbility);
            var types = Assembly.GetExecutingAssembly()
                .GetTypes()
                .Where(t => !t.IsAbstract && baseType.IsAssignableFrom(t));

            foreach (var type in types)
            {
                var attr = type.GetCustomAttribute<AbilityIdAttribute>();
                if (attr != null)
                {
                    abilityTypes[attr.Id] = type;
                    Debug.Log($"[AbilityInitializer] Registered ability: {attr.Id} -> {type.Name}");
                }
            }

            isInitialized = true;
            Debug.Log($"[AbilityInitializer] Total {abilityTypes.Count} abilities registered.");
        }

        /// <summary>
        /// 根据ID创建能力实例
        /// </summary>
        public static BaseAbility CreateAbility(string abilityId)
        {
            if (!isInitialized) Initialize();

            if (!abilityTypes.TryGetValue(abilityId, out var type))
            {
                Debug.LogError($"[AbilityInitializer] Ability '{abilityId}' not registered!");
                return null;
            }

            return (BaseAbility)Activator.CreateInstance(type);
        }

        /// <summary>
        /// 批量创建能力
        /// </summary>
        public static List<BaseAbility> CreateAbilities(List<string> abilityIds)
        {
            var abilities = new List<BaseAbility>();
            foreach (var id in abilityIds)
            {
                var ability = CreateAbility(id);
                if (ability != null)
                    abilities.Add(ability);
            }
            return abilities;
        }

        /// <summary>
        /// 获取所有已注册的能力ID
        /// </summary>
        public static IReadOnlyList<string> GetAllAbilityIds()
        {
            if (!isInitialized) Initialize();
            return abilityTypes.Keys.ToList();
        }

        /// <summary>
        /// 检查能力ID是否已注册
        /// </summary>
        public static bool HasAbility(string abilityId)
        {
            if (!isInitialized) Initialize();
            return abilityTypes.ContainsKey(abilityId);
        }
    }
}
