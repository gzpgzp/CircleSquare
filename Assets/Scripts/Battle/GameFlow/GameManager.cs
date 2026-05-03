using GameFramework;
using NormalUI;
using SquareBattle.BattleCore;
using UserSave;

namespace Battle.GameFlow
{
    /// <summary>
    /// 战斗模式的游戏管理器。
    /// 由 GameWorld 创建并持有，不继承 Singleton，避免 new + Instance 冲突。
    /// </summary>
    public class GameManager
    {
        public CharacterManager characterManager { get; private set; }
        public LevelManager levelManager { get; private set; }

        public void GameStart(GameContext ctx)
        {
            UIManager.Instance.ShowGameTopPanel();

            InitManagers();
            InitLevel(ctx);
        }

        public void StopGame()
        {

        }

        private void InitManagers()
        {
            characterManager = new CharacterManager();
            levelManager = new LevelManager();

            characterManager.Init();
        }

        private void InitLevel(GameContext ctx)
        {
            ILevelSetup setup = ctx.gameMode switch
            {
                GameMode.SquareBattle => new SquareBattleLevelSetup(),
                GameMode.NewBattle   => new NewBattleLevelSetup(),
                _                    => new NewBattleLevelSetup(),
            };
            levelManager.CreateLevel(ctx, characterManager, setup);
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