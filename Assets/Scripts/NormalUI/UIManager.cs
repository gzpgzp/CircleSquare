using System;
using Tools.Dialogs;
using Tools.ResourcesTool;
using Tools.Singletons;
using UnityEngine;
using UnityEngine.Serialization;

namespace NormalUI
{
    public class UIManager : MMSingleton<UIManager>
    {
        private const string PanelPre = "Prefabs/UI/Panels/";
        private const string StartPanel = PanelPre + "StartPanel";
        private const string gameTopPanel = PanelPre + "GameTopPanel";
        private const string levelSelectPanel = PanelPre + "LevelSelectPanel";
        
        [SerializeField] private RectTransform dialogTr;
        [SerializeField] private RectTransform panelTr;
        
        private DialogManager dialogManager;
        
        public void Init()
        {
            dialogManager = new DialogManager();
            dialogManager.Init(dialogTr);
        }

        public void OnDestroy()
        {
            
        }

        public void ShowStartPanel()
        {
            MyResourcesManager.Instance.LoadAndInstantiate(StartPanel,panelTr);
        }

        public void ShowGameTopPanel()
        {
            MyResourcesManager.Instance.LoadAndInstantiate(gameTopPanel,panelTr);
        }

        public void ShowLevelSelectPanel()
        {
            MyResourcesManager.Instance.LoadAndInstantiate(levelSelectPanel,panelTr);
        }
    }
}