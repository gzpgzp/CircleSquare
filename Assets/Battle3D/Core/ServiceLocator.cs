using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle3D.Core
{
    /// <summary>
    /// 简易服务定位器 - 管理全局可访问的服务/管理器
    /// 轻量级替代方案，避免过度依赖单例模式
    /// </summary>
    public static class ServiceLocator
    {
        private static Dictionary<Type, object> services = new Dictionary<Type, object>();

        /// <summary>
        /// 注册服务实例
        /// </summary>
        public static void Register<T>(T service) where T : class
        {
            services[typeof(T)] = service;
        }

        /// <summary>
        /// 获取已注册的服务
        /// </summary>
        public static T Get<T>() where T : class
        {
            if (services.TryGetValue(typeof(T), out var service))
                return service as T;
            
            Debug.LogWarning($"[ServiceLocator] Service {typeof(T).Name} not registered!");
            return null;
        }

        /// <summary>
        /// 尝试获取服务，不输出警告
        /// </summary>
        public static bool TryGet<T>(out T service) where T : class
        {
            if (services.TryGetValue(typeof(T), out var obj))
            {
                service = obj as T;
                return service != null;
            }
            service = null;
            return false;
        }

        /// <summary>
        /// 注销服务
        /// </summary>
        public static void Unregister<T>() where T : class
        {
            services.Remove(typeof(T));
        }

        /// <summary>
        /// 清空所有服务（场景切换时调用）
        /// </summary>
        public static void Clear()
        {
            services.Clear();
        }
    }
}
