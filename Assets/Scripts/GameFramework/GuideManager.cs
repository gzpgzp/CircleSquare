using System;
using Mask;
using Tools.Dialogs;
using Tools.EventTool;
using Tools.Singletons;
using UserSave;

namespace GameFramework
{
    public class GuideManager : Singleton<GuideManager>, MMEventListener<EndGuideEvent>
    {
        public bool isInGuide = false;

        public void Init()
        {
            this.MMEventStartListening();
        }

        public void OnDestroy()
        {
            this.MMEventStopListening();
        }

        public void StartGuide()
        {
            isInGuide = true;                
            OpenDialogEvent.Trigger("GuideDialog", new GuideDialogContext()
            {
                dialogName = "GuideDialog",
                text = ConfigManager.Instance.GetGuideTexts()
            });
        }

        private void EndGuide()
        {
            isInGuide = false;
            SaveManager.Instance.saveData.playerContext.isGuided = true;
            SaveManager.Instance.Save();
            CloseDialogEvent.Trigger("GuideDialog");
        }

        public void OnMMEvent(EndGuideEvent eventType)
        {
            EndGuide();
        }
    }
}