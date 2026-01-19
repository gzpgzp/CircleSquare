using NormalUI;
using UserSave;

namespace Battle.GameFlow
{
    // 每个游戏自己的GameManager
    public class GameManager
    {
        private CharacterManager characterManager;
        private LevelManager levelManager;
        
        public void GameStart(bool isNewGame)
        {
            if (isNewGame)
            {
                SaveManager.Instance.Delete();
            }
            var playerContext = SaveManager.Instance.GetPlayerData();
            
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
            characterManager.CreateCharacterObj();
        }
    }
}