using Battle.GameFlow;
using Cameras;
using Tools.Singletons;
using UnityEngine;

namespace GameFramework
{
    public class GameContext
    {
        public bool isNewGame;
    }

    // 所有游戏的mono入口
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