using System;
using Battle.GameFlow;
using Cameras;
using Tools.Singletons;
using UnityEngine;
using UserSave;

namespace GameFramework
{
    /// <summary>游戏模式枚举，决定进入哪套关卡逻辑</summary>
    public enum GameMode
    {
        SquareBattle = 0,  // 旧系统：Archer / Car / DogDun 自治角色
        NewBattle    = 1,  // 新系统：Ability 组合角色
    }

    public class GameContext
    {
        public bool isNewGame;
        public GameMode gameMode;
    }

    public class GameWorld : MMSingleton<GameWorld>
    {
        public GameManager gameManager;
        
        // 一些世界信息
        public Vector3 mouseWorldPos = Vector3.zero;
        public Camera worldCamera => CameraManager.Instance.GetCamera(MyCameraType.Main);
        
        public void Init(GameContext ctx)
        {
            gameManager = new GameManager();
            gameManager.GameStart(ctx);
        }

        public void StopGame()
        {
            gameManager.StopGame();
        }

        private void Update()
        {
            gameManager.Tick(Time.deltaTime);
        }

        private void FixedUpdate()
        {
            gameManager.FixedUpdate(Time.fixedDeltaTime);
        }

        public Vector3 ComputeMouse(Vector3 fromPos)
        {
            mouseWorldPos = worldCamera.ScreenToWorldPoint(Input.mousePosition);
            mouseWorldPos.z = 0;
            
            return mouseWorldPos - fromPos;
        }
    }
}