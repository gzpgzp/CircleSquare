using System.Collections.Generic;
using Battle3D.Character;
using Battle3D.Core;
using UnityEngine;

namespace Battle3D.GameFlow
{
    /// <summary>
    /// 关卡管理器 - 初始化关卡场景和角色
    /// 负责创建玩家和敌人的配置
    /// </summary>
    public class LevelManager
    {
        public void CreateLevel()
        {
            Debug.Log("[LevelManager] Creating level...");

            var gameManager = GameManager.Instance;
            if (gameManager == null)
            {
                Debug.LogError("[LevelManager] GameManager not found!");
                return;
            }

            var charManager = gameManager.CharacterManager;

            // 创建玩家
            var playerCtx = CreatePlayerContext();
            var player = charManager.CreatePlayer(playerCtx);
            if (player != null)
            {
                player.SetPosition(Vector3.zero);
            }

            // 创建测试敌人
            CreateTestEnemies(charManager);

            Debug.Log("[LevelManager] Level created.");
        }

        private CharacterContext CreatePlayerContext()
        {
            return new CharacterContext
            {
                characterName = "Player",
                characterId = "player_001",
                maxHealth = 200f,
                moveSpeed = 6f,
                maxEnergy = 100f,
                isPlayer = true,
                abilityIds = new List<string>
                {
                    "basic_attack",
                    "dodge",
                    "skill_slash",
                    "skill_projectile",
                    "ultimate",
                },
            };
        }

        private void CreateTestEnemies(CharacterManager charManager)
        {
            // 创建几个测试敌人
            var positions = new Vector3[]
            {
                new Vector3(5f, 0f, 5f),
                new Vector3(-5f, 0f, 3f),
                new Vector3(3f, 0f, -5f),
            };

            for (int i = 0; i < positions.Length; i++)
            {
                var enemyCtx = new CharacterContext
                {
                    characterName = $"Enemy_{i}",
                    characterId = $"enemy_{i:D3}",
                    maxHealth = 100f,
                    moveSpeed = 3.5f,
                    maxEnergy = 50f,
                    isPlayer = false,
                    abilityIds = new List<string>
                    {
                        "basic_attack",
                    },
                    aiDetectRange = 12f,
                    aiAttackRange = 2.5f,
                };

                var enemy = charManager.CreateEnemy(enemyCtx);
                if (enemy != null)
                {
                    enemy.SetPosition(positions[i]);
                }
            }
        }

        public void RestartLevel()
        {
            Debug.Log("[LevelManager] Restarting level...");
            // TODO: 清除所有角色，重新创建
        }

        public void Tick(float dt)
        {
            // 关卡级别的更新逻辑（如计时器、环境效果等）
        }
    }
}
