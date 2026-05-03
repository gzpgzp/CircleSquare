using System.Collections.Generic;
using Battle.Character;
using Battle.Character.Ability;
using GameFramework;
using Tools.ResourcesTool;
using UnityEngine;

namespace Battle.GameFlow
{
    /// <summary>
    /// 新系统（Ability 组合）模式的关卡初始化策略。
    /// 负责加载地图、创建玩家和 AI 敌人。
    /// </summary>
    public class NewBattleLevelSetup : ILevelSetup
    {
        // ──────────────────────── 配置 ────────────────────────
        private const string MapPath = "Prefabs/Maps/Map_1";

        // 玩家出生位置
        private static readonly Vector3 PlayerSpawn = new Vector3(0f, 0f, 0f);

        // AI 敌人出生点列表（可自由扩展）
        private static readonly (Vector3 pos, string name)[] EnemySpawns =
        {
            (new Vector3( 3f, 0f, 0f), "Enemy_Melee_1"),
            (new Vector3(-3f, 0f, 0f), "Enemy_Melee_2"),
        };

        // ──────────────────────── ILevelSetup ────────────────────────

        public void CreateLevel(GameContext ctx, CharacterManager charMgr)
        {
            MyResourcesManager.Instance.LoadAndInstantiate(MapPath);

            CreatePlayer(charMgr);

            foreach (var (pos, name) in EnemySpawns)
                CreateMeleeEnemy(charMgr, pos, name);
        }

        public void Tick(float dt) { }

        public void Restart() { }

        // ──────────────────────── 工厂方法 ────────────────────────

        private static void CreatePlayer(CharacterManager charMgr)
        {
            var ctx = new CharacterContext
            {
                characterModelId = 1,
                isAI             = false,
                spawnPosition    = PlayerSpawn,
                characterName    = "Player",
                abilities        = new List<int>
                {
                    (int)AbilityEnum.HorizontalMove2D,
                    (int)AbilityEnum.Dash,
                    (int)AbilityEnum.Jump,
                },
            };
            charMgr.CreatePlayer(ctx);
        }

        private static void CreateMeleeEnemy(CharacterManager charMgr, Vector3 spawnPos, string enemyName)
        {
            var ctx = new CharacterContext
            {
                characterModelId = 1,
                isAI             = true,
                spawnPosition    = spawnPos,
                characterName    = enemyName,
                abilities        = new List<int>
                {
                    (int)AbilityEnum.AutoMove2D,
                    (int)AbilityEnum.MeleeAttack2D,
                },
            };
            charMgr.CreateEnemy(ctx);
        }
    }
}
