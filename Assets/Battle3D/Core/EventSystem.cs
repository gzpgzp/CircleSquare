using System;
using System.Collections.Generic;
using UnityEngine;

namespace Battle3D.Core
{
    /// <summary>
    /// 全局事件总线 - 解耦各系统间的通信
    /// 采用发布-订阅模式，避免系统间直接引用
    /// </summary>
    public static class GameEventSystem
    {
        private static Dictionary<string, Delegate> eventTable = new Dictionary<string, Delegate>();

        #region 无参事件

        public static void Subscribe(string eventName, Action handler)
        {
            if (eventTable.TryGetValue(eventName, out var existing))
                eventTable[eventName] = Delegate.Combine(existing, handler);
            else
                eventTable[eventName] = handler;
        }

        public static void Unsubscribe(string eventName, Action handler)
        {
            if (eventTable.TryGetValue(eventName, out var existing))
            {
                var result = Delegate.Remove(existing, handler);
                if (result == null)
                    eventTable.Remove(eventName);
                else
                    eventTable[eventName] = result;
            }
        }

        public static void Publish(string eventName)
        {
            if (eventTable.TryGetValue(eventName, out var existing))
            {
                (existing as Action)?.Invoke();
            }
        }

        #endregion

        #region 单参事件

        public static void Subscribe<T>(string eventName, Action<T> handler)
        {
            if (eventTable.TryGetValue(eventName, out var existing))
                eventTable[eventName] = Delegate.Combine(existing, handler);
            else
                eventTable[eventName] = handler;
        }

        public static void Unsubscribe<T>(string eventName, Action<T> handler)
        {
            if (eventTable.TryGetValue(eventName, out var existing))
            {
                var result = Delegate.Remove(existing, handler);
                if (result == null)
                    eventTable.Remove(eventName);
                else
                    eventTable[eventName] = result;
            }
        }

        public static void Publish<T>(string eventName, T arg)
        {
            if (eventTable.TryGetValue(eventName, out var existing))
            {
                (existing as Action<T>)?.Invoke(arg);
            }
        }

        #endregion

        #region 双参事件

        public static void Subscribe<T1, T2>(string eventName, Action<T1, T2> handler)
        {
            if (eventTable.TryGetValue(eventName, out var existing))
                eventTable[eventName] = Delegate.Combine(existing, handler);
            else
                eventTable[eventName] = handler;
        }

        public static void Unsubscribe<T1, T2>(string eventName, Action<T1, T2> handler)
        {
            if (eventTable.TryGetValue(eventName, out var existing))
            {
                var result = Delegate.Remove(existing, handler);
                if (result == null)
                    eventTable.Remove(eventName);
                else
                    eventTable[eventName] = result;
            }
        }

        public static void Publish<T1, T2>(string eventName, T1 arg1, T2 arg2)
        {
            if (eventTable.TryGetValue(eventName, out var existing))
            {
                (existing as Action<T1, T2>)?.Invoke(arg1, arg2);
            }
        }

        #endregion

        /// <summary>
        /// 清空所有事件（场景切换时调用）
        /// </summary>
        public static void Clear()
        {
            eventTable.Clear();
        }
    }

    /// <summary>
    /// 事件名称常量，避免硬编码字符串
    /// </summary>
    public static class GameEvents
    {
        // 战斗相关
        public const string OnDamageDealt = "OnDamageDealt";
        public const string OnCharacterDeath = "OnCharacterDeath";
        public const string OnCharacterHit = "OnCharacterHit";

        // 技能相关
        public const string OnAbilityActivated = "OnAbilityActivated";
        public const string OnAbilityDeactivated = "OnAbilityDeactivated";
        public const string OnAbilityCooldownEnd = "OnAbilityCooldownEnd";

        // 能量相关
        public const string OnEnergyChanged = "OnEnergyChanged";

        // 状态相关
        public const string OnCharacterStateChanged = "OnCharacterStateChanged";
        public const string OnLockOnTarget = "OnLockOnTarget";
        public const string OnUnlockTarget = "OnUnlockTarget";

        // 游戏流程
        public const string OnGameStart = "OnGameStart";
        public const string OnGamePause = "OnGamePause";
        public const string OnGameResume = "OnGameResume";
        public const string OnGameOver = "OnGameOver";
    }
}
