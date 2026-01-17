using NormalUI;

namespace Battle.GameFlow
{
    // 每个游戏自己的GameManager
    public class GameManager
    {
        private CharacterManager characterManager;
        
        public void GameStart(bool isNewGame)
        {
            UIManager.Instance.ShowGameTopPanel();
        }

        public void StopGame()
        {
            
        }
    }
}