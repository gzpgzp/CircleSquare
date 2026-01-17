using System.Collections.Generic;

namespace Tools.Dialogs
{
    public class DialogPipe
    {
        private Stack<BaseUIDialog> dialogsStack = new Stack<BaseUIDialog>();

        public void OnDialogShow(BaseUIDialog dialog)
        {
            if (dialogsStack.Count > 0)
            {
                dialogsStack.Peek().SetVisible(false);
            }
            
            dialogsStack.Push(dialog);
        }

        public void OnDialogClose(string dialogName)
        {
            if (dialogsStack.Count <= 0) return;

            if (dialogsStack.Peek().DialogName != dialogName) return;

            var dialog = dialogsStack.Pop();

            if (dialogsStack.Count > 0)
            {
                dialogsStack.Peek().SetVisible(true);
            }
        }
    }
}