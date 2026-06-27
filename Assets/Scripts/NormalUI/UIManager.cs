using System;
using Adventure.UI;
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
        
        [SerializeField] private RectTransform dialogTr;
        [SerializeField] private RectTransform panelTr;
        
        private DialogManager dialogManager;

        private void Awake()
        {
            Init();
        }

        public void Init()
        {
            dialogManager = new DialogManager();
            dialogManager.Init(dialogTr);
        }

        public void ShowStartPanel()
        {
            MyResourcesManager.Instance.LoadAndInstantiate(StartPanel,panelTr);
        }

        public void ShowGameTopPanel()
        {
            MyResourcesManager.Instance.LoadAndInstantiate(gameTopPanel,panelTr);
        }
    }
}