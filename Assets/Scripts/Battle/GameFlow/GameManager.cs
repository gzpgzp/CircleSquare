using GameFramework;
using NormalUI;
using Tools.Singletons;
using UserSave;

namespace Battle.GameFlow
{
    // 每个游戏自己的GameManager
    public class GameManager : Singleton<GameManager>
    {
        public CharacterManager characterManager;
        public LevelManager levelManager;
        
        public void GameStart(GameContext ctx)
        {
            UIManager.Instance.ShowGameTopPanel();
            
            InitManager();
            InitLevel();
        }

        public void StopGame()
        {
            
        }

        private void InitManager()
        {
            characterManager = new CharacterManager();
            levelManager = new LevelManager();
            
            characterManager.Init();
        }

        private void InitLevel()
        {
            // 创建关卡
            levelManager.CreateLevel();
        }

        public void Tick(float dt)
        {
            characterManager.Tick(dt);
            levelManager.Tick(dt);
        }

        public void FixedUpdate(float dt)
        {
            characterManager.FixedUpdate(dt);
        }
    }
}