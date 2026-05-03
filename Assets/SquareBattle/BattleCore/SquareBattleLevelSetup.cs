using Battle.GameFlow;
using GameFramework;
using SquareBattle.BattleCore;
using SquareBattle.BattleCore.Characters;
using Tools.ResourcesTool;
using UnityEngine;

namespace SquareBattle.BattleCore
{
    /// <summary>
    /// SquareBattle 模式的关卡初始化策略。
    /// Camp1：Archer（左侧）+ Melee1（左下）
    /// Camp2：DogDun（右侧）+ Melee1（右下）
    /// </summary>
    public class SquareBattleLevelSetup : ILevelSetup
    {
        // ── 地图 ──────────────────────────────────────────────
        private const string MapPath     = "Prefabs/SquareBattle/Enviroment";

        // ── 角色预制体路径 ──────────────────────────────────────
        private const string ArcherPath  = "Prefabs/SquareBattle/Archer";
        private const string CarPath     = "Prefabs/SquareBattle/Car";
        private const string DogDunPath  = "Prefabs/SquareBattle/DogDun";
        private const string Melee1Path  = "Prefabs/SquareBattle/Melee1";
        private const string SwapperPath  = "Prefabs/SquareBattle/Swapper";
        private const string SquareBattleCarPath     = "Prefabs/SquareBattle/SquareBattleCar";
        private const string SquareBattleArcherPath  = "Prefabs/SquareBattle/SquareBattleArcher";
        private const string SquareBattleMeleePath  = "Prefabs/SquareBattle/SquareBattleMelee";

        // ── 出生位置 ──────────────────────────────────────────
        // Camp1
        private static readonly Vector3 CampOneSpawn  = new Vector3(-4f,  0f, 0f);
        // Camp2
        private static readonly Vector3 CampTwoSpawn1  = new Vector3( 4f, -2f, 0f);
        private static readonly Vector3 CampTwoSpawn2  = new Vector3( 4f,  2f, 0f);

        public void CreateLevel(GameContext ctx, CharacterManager charMgr)
        {
            MyResourcesManager.Instance.LoadAndInstantiate(MapPath);

            // Camp1
            Spawn(ArcherPath, CampOneSpawn,
                new CharacterSpawnData().WithCamp(Camp.Camp1).WithMaxHp(1000));
            // Camp2
            Spawn(Melee1Path, CampTwoSpawn1,
                new CharacterSpawnData().WithCamp(Camp.Camp2).WithMaxHp(500));
            Spawn(SwapperPath, CampTwoSpawn2,
                new CharacterSpawnData().WithCamp(Camp.Camp2).WithMaxHp(500));
            
            // 新增 SquareBattle 角色（可选）
            // Spawn(SquareBattleCarPath, CampOneSpawn + new Vector3(0f, 2f, 0f),
            //     new CharacterSpawnData().WithCamp(Camp.Camp1).WithMaxHp(1200));
            // Spawn(SquareBattleArcherPath, CampTwoSpawn1 + new Vector3(0f, 2f, 0f),
            //     new CharacterSpawnData().WithCamp(Camp.Camp2).WithMaxHp(800));
            // Spawn(SquareBattleMeleePath, CampTwoSpawn2 + new Vector3(0f, 2f, 0f),
            //     new CharacterSpawnData().WithCamp(Camp.Camp2).WithMaxHp(600));
        }

        public void Tick(float dt) { }

        public void Restart() { }

        // ───────────────────────────────────────────────────
        private static void Spawn(string path, Vector3 pos, CharacterSpawnData data)
        {
            var prefab = Resources.Load<GameObject>(path);
            if (prefab == null)
            {
                Debug.LogWarning($"[SquareBattleLevelSetup] 找不到预制体：{path}\n请将预制体放在对应的 Resources 目录下。");
                return;
            }
            var go = Object.Instantiate(prefab, pos, Quaternion.identity);
            go.GetComponent<CharacterBase>()?.SetSpawnData(data);
        }
    }
}