using System;
using Tools.EventTool;
using Tools.ResourcesTool;
using UnityEngine;
using UnityEngine.Serialization;

namespace Mask
{
    public class MaskWorldItem : WorldTriggerItem
    {
        public GameObject mask;
        public Color targetColor;

        public Func<Color> GetCurrColor;
        public bool showHideArea = true;

        public void Init(Func<Color> GetCurrColor)
        {
            this.GetCurrColor = GetCurrColor;
            mask.GetComponent<SpriteRenderer>().color = targetColor;
        }

        protected override void OnTrigger(GameObject go)
        {
            go.GetComponent<WearMask>().Wear(targetColor);
            gameObject.SetActive(false);
            
            CircleWallEffect.Instance.StartEffect(transform.position,Color.black, targetColor, GetCurrColor(), showHideArea);
            OnMaskCollectEvent.Trigger(targetColor);
        }

        public void Restart()
        {
            gameObject.SetActive(true);
        }
    }
}