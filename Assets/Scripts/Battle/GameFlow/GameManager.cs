using GameFramework;
using Mask;
using NormalUI;
using Tools.Dialogs;
using Tools.EventTool;
using Tools.Singletons;
using UnityEngine;
using UserSave;

namespace Battle.GameFlow
{
    public struct LevelStartEvent
    {
        public int levelIndex;

        static LevelStartEvent e;

        public static void Trigger(int levelIndex)
        {
            e.levelIndex = levelIndex;
            MMEventManager.TriggerEvent(e);
        }
    }

    // 每个游戏自己的GameManager
    public class GameManager : Singleton<GameManager>, MMEventListener<GameRestartEvent>,
        MMEventListener<LevelOutItemCollectEvent>, MMEventListener<GameNextLevelEvent>, MMEventListener<LevelStartEvent>
    {
        public CharacterManager characterManager;
        public LevelManager levelManager;

        private bool isGaming = false;

        public void GameStart()
        {
            UIManager.Instance.ShowLevelSelectPanel();
            this.MMEventStartListening<GameRestartEvent>();
            this.MMEventStartListening<GameNextLevelEvent>();
            this.MMEventStartListening<LevelOutItemCollectEvent>();
            this.MMEventStartListening<GameNextLevelEvent>();
            this.MMEventStartListening<LevelStartEvent>();
        }

        public void StopGame()
        {
            this.MMEventStopListening<GameRestartEvent>();
            this.MMEventStopListening<GameNextLevelEvent>();
            this.MMEventStopListening<LevelOutItemCollectEvent>();
            this.MMEventStopListening<GameNextLevelEvent>();
            this.MMEventStopListening<LevelStartEvent>();
        }

        private void LevelStart(int levelIndex)
        {
            this.isGaming = true;
            UIManager.Instance.ShowGameTopPanel();
            InitManager();
            InitLevel(levelIndex);

            if (!SaveManager.Instance.saveData.playerContext.isGuided)
            {
                GuideManager.Instance.StartGuide();
            }
        }

        public void Restart()
        {
            levelManager.RestartLevel();
        }

        private void InitManager()
        {
            characterManager = new CharacterManager();
            levelManager = new LevelManager();

            characterManager.Init();
        }

        private void InitLevel(int levelIndex)
        {
            // 创建关卡
            levelManager.CreateLevel(levelIndex);
        }

        public void Tick(float dt)
        {
            if (!isGaming) return;
            characterManager?.Tick(dt);
            levelManager?.Tick(dt);
        }

        public void FixedUpdate(float dt)
        {
            if(!isGaming) return;
            characterManager?.FixedUpdate(dt);
        }

        public void OnMMEvent(GameRestartEvent eventType)
        {
            Restart();
        }

        public void OnMMEvent(LevelOutItemCollectEvent eventType)
        {
            levelManager.PauseLevel();
            isGaming = false;
            var success = levelManager.CheckNextLevel();

            OpenDialogEvent.Trigger("LevelCompleteDialog", new LevelCompleteDialogContext()
            {
                dialogName = "LevelCompleteDialog",
                isShowNextLevel = success
            });
        }

        public void OnMMEvent(GameNextLevelEvent eventType)
        {
            levelManager.EnterNextLevel();
        }

        public void OnMMEvent(LevelStartEvent eventType)
        {
            LevelStart(eventType.levelIndex);
        }
    }
}