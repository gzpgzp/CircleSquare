using System;
using Battle.GameFlow;
using UnityEngine;
using UnityEngine.UI;

namespace Mask
{
    public class LevelListItem: MonoBehaviour
    {
        [SerializeField] private Button btn;
        [SerializeField] private Text text;
        [SerializeField] private GameObject lockImg;

        private int levelIndex;
        private Action OnClick;
        
        private void Start()
        {
            btn.onClick.AddListener(OnBtnClick);
        }

        private void OnDestroy()
        {
            btn.onClick.RemoveAllListeners();
        }

        private void OnBtnClick()
        {
            OnClick?.Invoke();
            LevelStartEvent.Trigger(levelIndex);
        }

        public void Init(int levelIndex, bool isShowLock, Action clickAction)
        {
            this.OnClick = clickAction;
            text.text = levelIndex.ToString();
            this.levelIndex = levelIndex;
            
            text.gameObject.SetActive(!isShowLock);
            lockImg.SetActive(isShowLock);
        }
    }
}