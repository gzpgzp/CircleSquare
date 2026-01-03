using UnityEngine;

namespace Tools.Dialogs
{
    public class BaseUIDialogContext
    {
        public string dialogName;
    }

    public class BaseUIDialog<T> : MonoBehaviour where T : BaseUIDialogContext
    {
        private T context;
        
        // 主动展示
        public void OnShow(T ctx)
        {
            ctx.dialogName = gameObject.name;
            context = ctx;
        }

        // 主动关闭
        public void OnClose()
        {
            
        }

        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);   
        }

        public string DialogName
        {
            get { return context.dialogName; }
        }
    }
}