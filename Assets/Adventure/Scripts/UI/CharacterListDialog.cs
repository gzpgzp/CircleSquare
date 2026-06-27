using System.Collections.Generic;
using Adventure.Character;
using GameFramework;
using TMPro;
using Tools.Dialogs;
using UnityEngine;
using UnityEngine.UI;

namespace Adventure.UI
{
    /// <summary>
    /// 角色列表弹窗 - 展示玩家拥有的所有角色信息
    /// 预制体路径: Resources/Prefabs/UI/Dialogs/CharacterListDialog
    /// </summary>
    public class CharacterListDialog : BaseUIDialog<BaseUIDialogContext>
    {
        [Header("UI引用")]
        [SerializeField] private Transform characterListContent;
        [SerializeField] private GameObject characterItemPrefab;
        [SerializeField] private Button closeBtn;
        [SerializeField] private TMP_Text titleText;

        private List<GameObject> itemInstances = new List<GameObject>();

        protected override void OnShowTyped(BaseUIDialogContext context)
        {
            if (titleText != null)
                titleText.text = "冒险者公会";

            closeBtn.onClick.AddListener(OnCloseClick);
            RefreshList();
        }

        protected override void OnClose()
        {
            closeBtn.onClick.RemoveAllListeners();
            ClearList();
            base.OnClose();
        }

        private void RefreshList()
        {
            ClearList();

            // 获取所有玩家角色（非enemy_开头的）
            var allChars = ConfigManager.Instance.GetAllCharacterInfos();

            foreach (var charInfo in allChars)
            {
                if (charInfo.CharacterId.StartsWith("enemy_")) continue;

                var go = Instantiate(characterItemPrefab, characterListContent);
                var itemUI = go.GetComponent<CharacterItemUI>();
                if (itemUI != null)
                {
                    itemUI.Setup(charInfo);
                }
                itemInstances.Add(go);
            }
        }

        private void ClearList()
        {
            foreach (var go in itemInstances)
            {
                if (go != null) Destroy(go);
            }
            itemInstances.Clear();
        }

        private void OnCloseClick()
        {
            Close();
        }
    }
}
