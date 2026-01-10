using Tools.Dialogs;
using Tools.Singletons;
using UnityEngine;

namespace NormalUI
{
    public class UIManager : MMSingleton<UIManager>
    {
        [SerializeField] private Transform dialogTr;
        [SerializeField] private Transform topPanel;
        
        private DialogManager dialogManager;
        
        public void Init()
        {
            dialogManager.Init(dialogTr);
        }
    }
}