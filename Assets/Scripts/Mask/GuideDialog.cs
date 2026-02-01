using System;
using System.Collections.Generic;
using DG.Tweening;
using TMPro;
using Tools.Dialogs;
using Tools.EventTool;
using UnityEngine;
using UnityEngine.UI;

namespace Mask
{
    public struct EndGuideEvent
    {
        static EndGuideEvent e;

        public static void Trigger()
        {
            MMEventManager.TriggerEvent(e);
        }
    }

    public class GuideDialogContext : BaseUIDialogContext
    {
        public List<string> text;
    }

    public class GuideDialog : BaseUIDialog<GuideDialogContext>
    {
        [SerializeField] private TMP_Text text;
        [SerializeField] private Button bgButton;
        [SerializeField] private float duration = 1f;

        private Tweener textTweener;
        private List<String> texts;
        private int currIndex;

        private void Start()
        {
            bgButton.onClick.AddListener(OnBgButtonClick);
        }

        private void OnDestroy()
        {
            bgButton.onClick.RemoveAllListeners();
        }

        protected override void OnShowTyped(GuideDialogContext context)
        {
            texts = context.text;
            currIndex = 0;
            ShowText(currIndex++);
        }

        private void ShowText(int index)
        {
            text.text = texts[index];
            text.ForceMeshUpdate();
            int totalChars = text.textInfo.characterCount;

            text.maxVisibleCharacters = 0;

            textTweener = DOTween.To(
                () => text.maxVisibleCharacters,
                x => text.maxVisibleCharacters = x,
                totalChars,
                duration
            ).SetEase(Ease.Linear);
        }

        private void OnBgButtonClick()
        {
            if (textTweener.IsPlaying())
            {
                textTweener.Complete();
            }
            else if (currIndex > texts.Count - 1)
            {
                Close();
                EndGuideEvent.Trigger();
            }
            else
            {
                ShowText(currIndex++);
            }
        }
    }
}