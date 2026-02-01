using System;
using TMPro;
using Tools.EventTool;
using UnityEngine;

namespace Mask
{
    public struct OnMaskCollectEvent
    {
        public Color turnToColor;

        public OnMaskCollectEvent(Color turnToColor)
        {
            this.turnToColor = turnToColor;
        }

        static OnMaskCollectEvent e;

        public static void Trigger(Color turnToColor)
        {
            e.turnToColor = turnToColor;
            MMEventManager.TriggerEvent(e);
        }
    }
    
    public class WorldText : MonoBehaviour, MMEventListener<OnMaskCollectEvent>
    {
        [SerializeField] private TextMeshPro changeText;
        [SerializeField] private MaskWorldItem mask;
        public Color showColor;
        public Color startColor;
        
        public void Start()
        {
            if (mask != null)
            {
                showColor = mask.targetColor;
            }
            UnActiveText();
            this.MMEventStartListening();
        }

        public void OnDestroy()
        {
            this.MMEventStopListening();
        }


        private void ActiveText()
        {
            changeText.gameObject.SetActive(true);
            changeText.color = startColor;
        }

        private void UnActiveText()
        {
            changeText.gameObject.SetActive(false);
            changeText.color = showColor;
        }

        public void Restart()
        {
            UnActiveText();
        }

        public void OnMMEvent(OnMaskCollectEvent eventType)
        {
            if (showColor == eventType.turnToColor)
            {
                ActiveText();
            }
            else
            {
                UnActiveText();
            }
        }
    }
}