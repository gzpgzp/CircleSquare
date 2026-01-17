using UnityEngine;

namespace Tools.Dialogs
{

    public class BaseUIDialogContext
    {
        public string dialogName;
    }

    public abstract class BaseUIDialog: MonoBehaviour
    {
        protected BaseUIDialogContext context;
        
        // 主动展示
        public void Show(BaseUIDialogContext ctx)
        {
            ctx.dialogName = gameObject.name;
            context = ctx;
            OnShow(ctx);
        }

        public void Close()
        {
            CloseDialogEvent.Trigger(context.dialogName);
            OnClose();
        }
        
        public void SetVisible(bool isVisible)
        {
            gameObject.SetActive(isVisible);   
        }

        protected virtual void OnShow(BaseUIDialogContext ctx)
        {
            
        }

        protected virtual void OnClose()
        {
            Destroy(gameObject);
        }

        public string DialogName
        {
            get { return context.dialogName; }
        }
    }
    
    // 泛型
    public abstract class BaseUIDialog<T> : BaseUIDialog
        where T : BaseUIDialogContext
    {
        protected T TypedContext;

        protected override void OnShow(BaseUIDialogContext ctx)
        {
            TypedContext = (T)ctx;
            OnShowTyped(TypedContext);
        }

        protected abstract void OnShowTyped(T context);
    }
}