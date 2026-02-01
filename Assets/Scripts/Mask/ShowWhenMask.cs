using System;
using Tools.EventTool;
using UnityEngine;

namespace Mask
{
    public class ShowWhenMask : MonoBehaviour, MMEventListener<OnMaskCollectEvent>
    {
        public MaskWorldItem mask;
        public GameObject obj;
        public bool isActiveShow;
        private Color targetColor;
        
        private void OnEnable()
        {
            this.MMEventStartListening();
            targetColor = mask.targetColor;
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void OnMMEvent(OnMaskCollectEvent eventType)
        {
            bool show = false;
            if (isActiveShow)
            {
                show = eventType.turnToColor == targetColor;
            }
            else
            {
                show = eventType.turnToColor != targetColor;
            }

            obj.SetActive(show);
        }

        public void Show()
        {
            obj.SetActive(true);
        }
    }
}