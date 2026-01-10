using System.Text;
using Tools.ResourcesTool;
using Unity.VisualScripting;

namespace Tools.Dialogs
{
    public class DialogCreator
    {
        private const string DialogPrePath = "Prefabs/Dialogs/";

        public BaseUIDialog<BaseUIDialogContext> CreateDialog(string dialogName)
        {
            var sb = new StringBuilder(DialogPrePath);
            sb.Append(dialogName);
            MyResourcesManager.Instance.LoadAndInstantiate(sb.ToString()).TryGetComponent<BaseUIDialog<BaseUIDialogContext>>(out var dialog);
            return dialog;
        }
    }
}