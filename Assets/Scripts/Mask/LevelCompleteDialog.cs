using System;
using GameFramework;
using NormalUI;
using Tools.Dialogs;
using UnityEngine;
using UnityEngine.UI;

namespace Mask
{    
    public class LevelCompleteDialogContext : BaseUIDialogContext
    {
        public bool isShowNextLevel;
    }
    
    public class LevelCompleteDialog : BaseUIDialog<LevelCompleteDialogContext>
    {
        [SerializeField] private Button nextlevel;
        [SerializeField] private Button restart;
        [SerializeField] private Button mainMenu;

        private void Start()
        {
            nextlevel.onClick.AddListener(OnNextLevelClicked);  
            restart.onClick.AddListener(OnRestartClicked);
            mainMenu.onClick.AddListener(OnMainMenuClicked);
        }

        private void OnDestroy()
        {
            nextlevel.onClick.RemoveAllListeners();
            restart.onClick.RemoveAllListeners();
            mainMenu.onClick.RemoveAllListeners();
        }

        private void OnNextLevelClicked()
        {
            Close();
            GameNextLevelEvent.Trigger();
        }

        private void OnRestartClicked()
        {
            Close();
            GameRestartEvent.Trigger();
        }

        private void OnMainMenuClicked()
        {
            Close();
            GameStopEvent.Trigger();
        }
        
        protected override void OnShowTyped(LevelCompleteDialogContext context)
        {
             nextlevel.gameObject.SetActive(context.isShowNextLevel);
        }
    }
}