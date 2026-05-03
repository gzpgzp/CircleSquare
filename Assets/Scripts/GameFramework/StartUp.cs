using Define;
using UnityEngine;

namespace GameFramework
{
    public class StartUp : MonoBehaviour
    {
        [Header("Game Mode")]
        [Tooltip("在 Inspector 里直接选择游戏模式，无需 UI")]
        [SerializeField] private GameMode gameMode = GameMode.SquareBattle;

        /// <summary>全局到 StartPanel 的传递通道</summary>
        public static GameMode SelectedMode { get; private set; } = GameMode.SquareBattle;

        private GameFrameWork gameGameFrameWork;

        public void Awake()
        {
            SelectedMode = gameMode;
            DontDestroyOnLoad(this);
            MySceneManager.Instance.LoadScene(GameScene.MainMenu, OnStartUp);
        }

        private void OnStartUp()
        {
            gameGameFrameWork = new GameFrameWork();
            gameGameFrameWork.StartUp();
        }
    }
}