using System;
using DG.Tweening;
using Tools.EventTool;
using UnityEngine;
using UnityTimer;

namespace Mask
{
    public struct LevelOutItemCollectEvent
    {
        static LevelOutItemCollectEvent e;

        public static void Trigger()
        {
            MMEventManager.TriggerEvent(e);
        }
    }
    
    public class LevelOutItem : WorldTriggerItem
    {
        [SerializeField] private Transform rotateItem;

        private Tweener tweener;
        
        private void OnEnable()
        {
            tweener = rotateItem.DORotate(new Vector3(0, 360, 0), 2f, RotateMode.FastBeyond360)
                .SetLoops(-1, LoopType.Restart)
                .SetEase(Ease.Linear);
        }

        private void OnDisable()
        {
            tweener.Kill();
        }

        protected override void OnTrigger(GameObject go)
        {
            gameObject.SetActive(false);
            Timer.Register(0.5f, () => LevelOutItemCollectEvent.Trigger());
        }
    }
}