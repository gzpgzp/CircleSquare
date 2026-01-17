using System;
using Define;
using NormalUI;
using UnityEngine;
using UnityEngine.Serialization;

namespace GameFramework
{
    public class StartUp : MonoBehaviour
    {
        private GameFrameWork gameGameFrameWork;
        
        public void Awake()
        {
            DontDestroyOnLoad(this);
            MySceneManager.Instance.LoadScene(GameScene.MainMenu,OnStartUp);
        }

        private void OnStartUp()
        {
            gameGameFrameWork = new GameFrameWork();
            gameGameFrameWork.StartUp();
        }
    }
}