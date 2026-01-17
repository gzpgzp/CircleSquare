using System;
using Battle.GameFlow;
using Define;
using NormalUI;
using Tools.Dialogs;
using Tools.EventTool;
using Tools.ResourcesTool;
using UnityEngine;

namespace GameFramework
{
    public struct GameStartEvent
    {
        public bool isNewGame;
        
        public GameStartEvent(bool isNewGame)
        {
            this.isNewGame = isNewGame;
        }

        static GameStartEvent e;

        public static void Trigger(bool isNewGame)
        {
            e.isNewGame = isNewGame;
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
        private GameManager gameManager;
        
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

        private void OnGameStart(bool isNewGame)
        {
            gameManager = new GameManager();
            gameManager.GameStart(isNewGame);
        }

        public void OnMMEvent(GameStartEvent e)
        {
            OnGameStart(e.isNewGame);
        }

        // 返回主菜单
        public void OnMMEvent(GameStopEvent eventType)
        {
            gameManager.StopGame();
            MySceneManager.Instance.LoadScene(GameScene.MainMenu,InitGameStartScene);
        }
    }
}