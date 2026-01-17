using System.Text;
using Tools.ResourcesTool;
using Unity.VisualScripting;
using UnityEngine;

namespace Tools.Dialogs
{
    public static class DialogCreator
    {
        private const string DialogPrePath = "Prefabs/UI/Dialogs/";

        public static BaseUIDialog CreateDialog(string dialogName, Transform transform)
        {
            var sb = new StringBuilder(DialogPrePath);
            sb.Append(dialogName);
            
            var go = MyResourcesManager.Instance.LoadAndInstantiate(sb.ToString(),transform);
            
            go.TryGetComponent<BaseUIDialog>(out var dialog);
            return dialog;
        }
    }
}