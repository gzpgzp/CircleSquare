using Define;
using NormalUI;
using Tools.EventTool;
using Tools.ResourcesTool;
using UserSave;

namespace GameFramework
{
    public struct GameStartEvent
    {
        public GameContext ctx;
        
        public GameStartEvent(GameContext ctx)
        {
            this.ctx = ctx;
        }

        static GameStartEvent e;

        public static void Trigger(GameContext ctx)
        {
            e.ctx = ctx;
            MMEventManager.TriggerEvent(e);
        }
    }

    public struct GameStopEvent
    {
        static GameStopEvent e;

        public static void Trigger()
        {
            MMEventManager.TriggerEvent(e);
        }
    }

    public class GameFrameWork : MMEventListener<GameStartEvent>, MMEventListener<GameStopEvent>
    {
        private GameWorld gameWorld;
        
        public void StartUp()
        {
            // 资源等加载
            ConfigManager.Instance.Init();
            MyResourcesManager.Instance.Init();
            
            // 游戏内容初始化
            UIManager.Instance.Init();
            
            InitGameStartScene();
            this.MMEventStartListening<GameStartEvent>();
            this.MMEventStartListening<GameStopEvent>();
        }

        public void OnDestroy()
        {
            this.MMEventStopListening<GameStartEvent>();
            this.MMEventStopListening<GameStopEvent>();
        }

        private void InitGameStartScene()
        {
            // 初始化游戏菜单
            UIManager.Instance.ShowStartPanel();
        }

        private void OnGameStart(GameContext ctx)
        {
            if (ctx.isNewGame)
            {
                SaveManager.Instance.Delete();
            }
            gameWorld = MyResourcesManager.Instance.LoadAndInstantiate("Prefabs/GameWorld").GetComponent<GameWorld>();
            gameWorld.Init(ctx);
        }

        public void OnMMEvent(GameStartEvent e)
        {
            OnGameStart(e.ctx);
        }

        // 返回主菜单
        public void OnMMEvent(GameStopEvent eventType)
        {
            gameWorld.StopGame();
            MySceneManager.Instance.LoadScene(GameScene.MainMenu,InitGameStartScene);
        }
    }
}