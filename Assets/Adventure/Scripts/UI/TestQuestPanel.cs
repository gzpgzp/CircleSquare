using Tools.Dialogs;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.UI
{
    /// <summary>
    /// 测试用面板 - 提供打开任务列表弹窗的入口按钮
    /// 挂载到场景中的测试 Panel 上即可
    /// </summary>
    public class TestQuestPanel : MonoBehaviour
    {
        [SerializeField] private Button openQuestListBtn;

        private void Start()
        {
            if (openQuestListBtn != null)
                openQuestListBtn.onClick.AddListener(OnOpenQuestList);
        }

        private void OnDestroy()
        {
            if (openQuestListBtn != null)
                openQuestListBtn.onClick.RemoveListener(OnOpenQuestList);
        }

        private void OnOpenQuestList()
        {
            OpenDialogEvent.Trigger("QuestListDialog", new QuestListDialogContext());
        }
    }
}
