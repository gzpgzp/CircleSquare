using Tools.EventTool;
using UnityEngine;

namespace Tools.Dialogs
{
    public class DialogManager : MMEventListener<OpenDialogEvent>, MMEventListener<CloseDialogEvent>
    {
        private Transform dialogParent;
        
        private DialogPipe dialogPipe;
        private DialogCreator dialogCreator;

        public void Init(Transform dialogParent)
        {
            this.dialogParent = dialogParent;
            dialogPipe = new DialogPipe();
        }

        private void OpenDialog(string dialogName, BaseUIDialogContext ctx)
        {
            var dialog = dialogCreator.CreateDialog(dialogName);
            if (dialog != null)
            {
                dialog.OnShow(ctx);
                dialogPipe.OnDialogShow(dialog);
            }
        }

        private void CloseDialog(string dialogName)
        {
            dialogPipe.OnDialogClose(dialogName);
        }

        public void OnMMEvent(OpenDialogEvent evt)
        {
            OpenDialog(evt.dialogName, evt.ctx);
        }

        public void OnMMEvent(CloseDialogEvent evt)
        {
            CloseDialog(evt.dialogName);
        }
    }

    public struct OpenDialogEvent
    {
        public string dialogName;
        public BaseUIDialogContext ctx;

        public OpenDialogEvent(string dialogName, BaseUIDialogContext ctx)
        {
            this.dialogName = dialogName;
            this.ctx = ctx;
        }

        static OpenDialogEvent e;

        public static void Trigger(string dialogName, BaseUIDialogContext ctx)
        {
            e.dialogName = dialogName;
            e.ctx = ctx;
            MMEventManager.TriggerEvent(e);
        }
    }

    public struct CloseDialogEvent
    {
        public string dialogName;

        public CloseDialogEvent(string dialogName)
        {
            this.dialogName = dialogName;
        }

        static CloseDialogEvent e;

        public static void Trigger(string dialogName)
        {
            e.dialogName = dialogName;
            MMEventManager.TriggerEvent(e);
        }
    }
}