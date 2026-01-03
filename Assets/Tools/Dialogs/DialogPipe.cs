using System.Collections.Generic;

namespace Tools.Dialogs
{
    public class DialogPipe
    {
        private Stack<BaseUIDialog<BaseUIDialogContext>> dialogsStack = new Stack<BaseUIDialog<BaseUIDialogContext>>();

        public void OnDialogShow(BaseUIDialog<BaseUIDialogContext> dialog)
        {
            var peekDialog = dialogsStack.Peek();
            peekDialog.SetVisible(false);
            
            dialogsStack.Push(dialog);
        }

        public void OnDialogClose(string dialogName)
        {
            if (dialogsStack.Count <= 0) return;

            if (dialogsStack.Peek().DialogName != dialogName) return;

            dialogsStack.Pop();
            var peekDialog = dialogsStack.Peek();
            peekDialog.SetVisible(true);
        }
    }
}