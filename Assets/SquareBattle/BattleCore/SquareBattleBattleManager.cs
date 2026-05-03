
using Battle.Character;
using Battle.Character.Ability;
using Battle.Character.Ability.A2D;
using Battle.GameFlow;
using SquareBattle.BattleCore;
using SquareBattle.BattleCore.Characters;
using UnityEngine;

namespace SquareBattle.BattleCore
{
    /// <summary>
    /// SquareBattle 专用战斗管理器（Resources 硬编码版）
    /// 所有预制体路径均通过 Resources.Load 加载，无需 Inspector 拖拽。
    /// </summary>
    public class SquareBattleBattleManager : MonoBehaviour
    {
        // ── 单例模式 ──────────────────────────────────────────────────────────
        public static SquareBattleBattleManager Instance { get; private set; }

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
        }

        private void Start()
        {
            InitializeBattleSystem();
        }

        /// <summary>
        /// 初始化战斗系统（可扩展为加载配置表等）
        /// </summary>
        private void InitializeBattleSystem()
        {
            Debug.Log("[SquareBattleBattleManager] 战斗系统初始化完成（Resources 模式）");
        }

        /// <summary>
        /// 创建 Archer 角色（从 Resources 加载）
        /// </summary>
        public GameObject CreateArcher(Battle.GameFlow.CharacterManager charMgr, Vector3 position, Camp camp, int maxHp = 1000)
        {
            const string prefabPath = "Prefabs/SquareBattle/SquareBattleArcher";
            var prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[SquareBattleBattleManager] ❌ 找不到预制体：{prefabPath}\n请确认路径正确且预制体已放入 Assets/Resources/Prefabs/SquareBattle/");
                return null;
            }

            var go = Instantiate(prefab, position, Quaternion.identity);
            var archer = go.GetComponent<SquareBattleArcher>();
            if (archer != null)
            {
                // 可在此处运行时设置参数（如 bulletCount、angleStep）
                // archer.bulletCount = 3;
                // archer.angleStep = 10;
            }
            return go;
        }

        /// <summary>
        /// 创建 Car 角色（从 Resources 加载）
        /// </summary>
        public GameObject CreateCar(Battle.GameFlow.CharacterManager charMgr, Vector3 position, Camp camp, int maxHp = 1200)
        {
            const string prefabPath = "Prefabs/SquareBattle/SquareBattleCar";
            var prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[SquareBattleBattleManager] ❌ 找不到预制体：{prefabPath}\n请确认路径正确且预制体已放入 Assets/Resources/Prefabs/SquareBattle/");
                return null;
            }

            var go = Instantiate(prefab, position, Quaternion.identity);
            var car = go.GetComponent<SquareBattleCar>();
            if (car != null)
            {
                // 运行时参数设置（示例）
                // car.minDriftAcceleration = 1.0f;
            }
            return go;
        }

        /// <summary>
        /// 创建 MeleeCharacter 角色（从 Resources 加载）
        /// </summary>
        public GameObject CreateMeleeCharacter(Battle.GameFlow.CharacterManager charMgr, Vector3 position, Camp camp, int maxHp = 800)
        {
            const string prefabPath = "Prefabs/SquareBattle/SquareBattleMelee";
            var prefab = Resources.Load<GameObject>(prefabPath);
            if (prefab == null)
            {
                Debug.LogError($"[SquareBattleBattleManager] ❌ 找不到预制体：{prefabPath}\n请确认路径正确且预制体已放入 Assets/Resources/Prefabs/SquareBattle/");
                return null;
            }

            var go = Instantiate(prefab, position, Quaternion.identity);
            var melee = go.GetComponent<SquareBattleMeleeCharacter>();
            if (melee != null)
            {
                // 运行时参数设置（示例）
                // melee.comboCount = 2;
            }
            return go;
        }

        /// <summary>
        /// 清理所有 SquareBattle 角色（可选）
        /// </summary>
        public void ClearAllCharacters()
        {
            Debug.Log("[SquareBattleBattleManager] 🧹 清理所有 SquareBattle 角色（待实现）");
        }
    }
}