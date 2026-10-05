using System.Collections.Generic;
using Battle3D.AI;
using Battle3D.Character;
using Battle3D.Character.Brain;
using Battle3D.Combat;
using Battle3D.Core;
using UnityEngine;

namespace Battle3D.GameFlow
{
    /// <summary>
    /// 战斗管理器 - 管理战斗流程和状态
    /// 负责：战斗开始/结束判定、目标管理、战斗数据统计
    /// </summary>
    public class CombatManager
    {
        private bool isCombatActive;
        private float combatTimer;

        // 战斗统计
        private int totalDamageDealt;
        private int totalDamageTaken;
        private int killCount;

        public bool IsCombatActive => isCombatActive;
        public float CombatTimer => combatTimer;

        public void Init()
        {
            // 订阅事件
            GameEventSystem.Subscribe<HitData>(GameEvents.OnCharacterHit, OnCharacterHit);
            GameEventSystem.Subscribe<DeathData>(GameEvents.OnCharacterDeath, OnCharacterDeath);

            Debug.Log("[CombatManager] Initialized.");
        }

        public void Tick(float dt)
        {
            if (!isCombatActive) return;

            combatTimer += dt;

            // 检查战斗是否结束
            CheckCombatEnd();
        }

        #region 战斗流程

        /// <summary>
        /// 开始战斗
        /// </summary>
        public void StartCombat()
        {
            isCombatActive = true;
            combatTimer = 0f;
            totalDamageDealt = 0;
            totalDamageTaken = 0;
            killCount = 0;

            Debug.Log("[CombatManager] Combat started.");
        }

        /// <summary>
        /// 结束战斗
        /// </summary>
        public void EndCombat(bool playerWon)
        {
            isCombatActive = false;

            Debug.Log($"[CombatManager] Combat ended. Player won: {playerWon}");
            Debug.Log($"[CombatManager] Stats - Time: {combatTimer:F1}s, Damage dealt: {totalDamageDealt}, " +
                      $"Damage taken: {totalDamageTaken}, Kills: {killCount}");
        }

        /// <summary>
        /// 检查战斗结束条件
        /// </summary>
        private void CheckCombatEnd()
        {
            var charManager = ServiceLocator.Get<CharacterManager>();
            if (charManager == null) return;

            var player = charManager.PlayerCharacter;
            if (player == null || !player.IsAlive)
            {
                EndCombat(false);
                return;
            }

            var enemies = charManager.GetAliveEnemies();
            if (enemies.Count == 0)
            {
                EndCombat(true);
            }
        }

        #endregion

        #region 事件处理

        private void OnCharacterHit(HitData data)
        {
            if (!isCombatActive) return;

            if (data.attacker != null && data.attacker.IsPlayer)
            {
                totalDamageDealt += (int)data.damage;
            }
            else if (data.target != null && data.target.IsPlayer)
            {
                totalDamageTaken += (int)data.damage;
            }

            // 通知AI进入战斗状态
            NotifyAIOnHit(data);
        }

        private void OnCharacterDeath(DeathData data)
        {
            if (!isCombatActive) return;

            if (data.character != null && !data.character.IsPlayer)
            {
                killCount++;
            }
        }

        /// <summary>
        /// 受击时通知附近的AI进入战斗
        /// </summary>
        private void NotifyAIOnHit(HitData data)
        {
            var charManager = ServiceLocator.Get<CharacterManager>();
            if (charManager == null) return;

            // 如果是AI被攻击，通知其进入追击状态
            if (data.target != null && !data.target.IsPlayer)
            {
                // 通过AIBrain设置目标
                // 这里简化处理，实际项目中应该通过事件或直接引用
            }
        }

        #endregion

        #region 目标管理

        /// <summary>
        /// 为所有AI设置共同目标
        /// </summary>
        public void SetAllAITargets(BaseCharacter target)
        {
            var charManager = ServiceLocator.Get<CharacterManager>();
            if (charManager == null) return;

            foreach (var kvp in charManager.AllCharacters)
            {
                var character = kvp.Value;
                if (character == null || character.IsPlayer) continue;

                // 尝试获取AIBrain并设置目标
                // 这里简化处理
            }
        }

        #endregion
    }
}
