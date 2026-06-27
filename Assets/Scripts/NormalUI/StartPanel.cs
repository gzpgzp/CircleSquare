using Battle.GameFlow;
using Define;
using GameFramework;
using UnityEngine;
using UnityEngine.UI;
using UserSave;

namespace NormalUI
{
    // 主界面
    public class StartPanel : MonoBehaviour
    {
        [SerializeField] private Button startButton;
        [SerializeField] private Button continueButton;
        [SerializeField] private Button quitButton;

        private void Start()
        {
            startButton.onClick.AddListener(OnStartButtonClicked);
            continueButton.onClick.AddListener(OnContinueButtonClicked);
            quitButton.onClick.AddListener(OnQuitButtonClicked);

            if (!SaveManager.Instance.HasSave())
            {
                continueButton.interactable = false;
            }
        }

        private void OnStartButtonClicked()
        {
            MySceneManager.Instance.LoadScene(GameScene.BattleScene,()=>OnGameStart(true));
            Destroy(gameObject);
        }

        private void OnContinueButtonClicked()
        {
            MySceneManager.Instance.LoadScene(GameScene.BattleScene,()=>OnGameStart(false));
            Destroy(gameObject);
        }

        private void OnQuitButtonClicked()
        {
#if UNITY_EDITOR
            UnityEditor.EditorApplication.isPlaying = false;
#else
            Application.Quit();
#endif
        }

        private void OnGameStart(bool isNewGame)
        {
            GameStartEvent.Trigger(new GameContext()
            {
                isNewGame = isNewGame
            });
        }
    }
}