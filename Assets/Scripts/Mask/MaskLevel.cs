using System;
using System.Collections.Generic;
using DG.Tweening;
using Tools.EventTool;
using UnityEngine;

namespace Mask
{
    public class MaskLevel : MonoBehaviour, MMEventListener<OnMaskCollectEvent>
    {
        public Transform respawnTr;
        public List<MaskWorldItem> maskItems = new List<MaskWorldItem>();
        public List<WorldText> worldTexts = new List<WorldText>();
        public SpriteRenderer wall;
        public LevelOutItem item;
        public List<Police> polices = new List<Police>();
        public List<BrokenRoad> Roads = new List<BrokenRoad>();
        public List<ShowWhenMask> items = new List<ShowWhenMask>();

        public Color startColor = Color.white;
        public Color currColor = Color.white;

        private void OnEnable()
        {
            this.MMEventStartListening();
        }

        private void OnDisable()
        {
            this.MMEventStopListening();
        }

        public void StartLevel()
        {
            currColor = startColor;
            foreach (var mask in maskItems)
            {
                mask.Init(()=>currColor);
            }

            foreach (var worldText in worldTexts)
            {
                worldText.startColor = startColor;
            }

            CircleWallEffect.Instance.Reset(startColor);
        }

        public void Restart()
        {
            foreach (var maskWorldItem in maskItems)
            {
                maskWorldItem.Restart();
            }

            foreach (var world in worldTexts)
            {
                world.Restart();
            }

            foreach (var police in polices)
            {
                police.Restart();
            }

            foreach (var road in Roads)
            {
                road.Restart();
            }

            foreach (var item in items)
            {
                item.Show();
            }

            currColor = startColor;
            item.gameObject.SetActive(true);
            CircleWallEffect.Instance.Reset(startColor);
        }

        public void OnMMEvent(OnMaskCollectEvent eventType)
        {
            currColor = eventType.turnToColor;
        }
    }
}