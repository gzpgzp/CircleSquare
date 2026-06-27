using System.Collections.Generic;
using Adventure.Character;
using GameFramework;
using UnityEngine;

namespace Adventure
{
    /// <summary>
    /// 冒险模块总管理器 - 初始化任务系统和挂机战斗
    /// </summary>
    public class AdventureGameManager : MonoBehaviour
    {
        [Header("玩家角色ID")]
        [SerializeField] private string playerCharacterId = "hero_warrior";

        private AutoCombatSystem combatSystem;

        public static AdventureGameManager Instance { get; private set; }

        private void Awake()
        {
            Instance = this;
        }

        private void Start()
        {
            // 初始化任务系统
            QuestManager.Instance.Init();
            ConfigManager.Instance.Init();

            // 初始化挂机战斗
            InitCombat();
        }

        private void InitCombat()
        {
            var charInfo = ConfigManager.Instance.GetCharacterInfo(playerCharacterId);
            if (charInfo == null)
            {
                Debug.LogWarning($"[AdventureGameManager] Character config not found: {playerCharacterId}");
                return;
            }

            // 创建玩家角色
            var player = new CharacterEntity(charInfo, CharacterCamp.Player);
            var playerTeam = new List<CharacterEntity> { player };

            // 启动自动战斗
            combatSystem = new AutoCombatSystem();
            combatSystem.Init(playerTeam);
            combatSystem.StartCombat();

            Debug.Log($"[AdventureGameManager] Combat initialized. Player: {player.characterName} Lv.{player.levelSystem.level}");
        }

        private void Update()
        {
            combatSystem?.Tick(Time.deltaTime);
        }

        private void OnDestroy()
        {
            combatSystem?.Destroy();
        }

        /// <summary>
        /// 获取战斗系统引用（供UI查询）
        /// </summary>
        public AutoCombatSystem GetCombatSystem() => combatSystem;
    }
}