using System;
using Tools;
using Tools.Singletons;
using UnityEngine.SceneManagement;

namespace GameFramework
{
    public class MySceneManager : MMSingleton<MySceneManager>
    {
        public void LoadScene(string sceneName,Action completeCallBack = null)
        {
            SceneManager.LoadSceneAsync(sceneName, LoadSceneMode.Single).completed += operation =>
            {
                completeCallBack?.Invoke();
            };
        }
    }
}