using Battle3D.GameFlow;
using UnityEngine;

namespace Battle3D.Core
{
    /// <summary>
    /// 3D战斗游戏总管理器
    /// 职责：初始化子系统、驱动主循环、管理服务定位器
    /// </summary>
    public class GameManager : MonoBehaviour
    {
        public static GameManager Instance { get; private set; }

        public CharacterManager CharacterManager { get; private set; }
        public LevelManager LevelManager { get; private set; }
        public CombatManager CombatManager { get; private set; }

        [Header("游戏状态")]
        [SerializeField] private bool isPaused;

        private void Awake()
        {
            if (Instance != null && Instance != this)
            {
                Destroy(gameObject);
                return;
            }
            Instance = this;
            DontDestroyOnLoad(gameObject);
        }

        private void Start()
        {
            GameStart();
        }

        public void GameStart()
        {
            InitManagers();
            InitLevel();

            ServiceLocator.Register(this);
            ServiceLocator.Register(CharacterManager);
            ServiceLocator.Register(LevelManager);
            ServiceLocator.Register(CombatManager);

            GameEventSystem.Publish(GameEvents.OnGameStart);
            Debug.Log("[GameManager] Game started.");
        }

        private void InitManagers()
        {
            CharacterManager = new CharacterManager();
            LevelManager = new LevelManager();
            CombatManager = new CombatManager();

            CharacterManager.Init();
            CombatManager.Init();
        }

        private void InitLevel()
        {
            LevelManager.CreateLevel();
        }

        private void Update()
        {
            if (isPaused) return;

            float dt = Time.deltaTime;
            Tick(dt);
        }

        private void FixedUpdate()
        {
            if (isPaused) return;

            float dt = Time.fixedDeltaTime;
            FixedTick(dt);
        }

        private void Tick(float dt)
        {
            CharacterManager?.Tick(dt);
            LevelManager?.Tick(dt);
            CombatManager?.Tick(dt);
        }

        private void FixedTick(float dt)
        {
            CharacterManager?.FixedTick(dt);
        }

        public void PauseGame()
        {
            isPaused = true;
            Time.timeScale = 0f;
            GameEventSystem.Publish(GameEvents.OnGamePause);
        }

        public void ResumeGame()
        {
            isPaused = false;
            Time.timeScale = 1f;
            GameEventSystem.Publish(GameEvents.OnGameResume);
        }

        public void StopGame()
        {
            GameEventSystem.Publish(GameEvents.OnGameOver);
            GameEventSystem.Clear();
            ServiceLocator.Clear();
            Debug.Log("[GameManager] Game stopped.");
        }

        private void OnDestroy()
        {
            if (Instance == this)
            {
                GameEventSystem.Clear();
                ServiceLocator.Clear();
            }
        }
    }
}
