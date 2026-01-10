using System;
using NormalUI;
using Tools.Dialogs;
using Tools.ResourcesTool;
using UnityEngine;

namespace GameFramework
{
    public class FrameWork : MonoBehaviour
    {
        [SerializeField] private UIManager uiManager;
        
        private MyResourcesManager myResourcesManager;
        
        
        public void Start()
        {
            StartUp();
        }

        private void StartUp()
        {
            uiManager.Init();   
         
            myResourcesManager.Init();
            ConfigManager.Instance.Init();
        }
    }
}