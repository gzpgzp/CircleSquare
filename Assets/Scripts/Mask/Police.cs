using System;
using DG.Tweening;
using Tools.EventTool;
using UnityEngine;

namespace Mask
{
    public class Police : MoveItem, MMEventListener<OnMaskCollectEvent>, MMEventListener<LevelOutItemCollectEvent>
    {
        [SerializeField] private DeathAnimation death;
        [SerializeField] private MaskWorldItem mask;
        [SerializeField] private SpriteRenderer sprite;
        private bool isBeAttackAble = false;

        private Color targetColor;

        private void Start()
        {
            if (mask != null)
            {
                targetColor = mask.targetColor;
            }
            
            this.MMEventStartListening<OnMaskCollectEvent>();
            this.MMEventStartListening<LevelOutItemCollectEvent>();
        }

        private void OnDestroy()
        {
            this.MMEventStopListening<OnMaskCollectEvent>();
            this.MMEventStopListening<LevelOutItemCollectEvent>();
        }

        private void OnCollisionEnter2D(Collision2D other)
        {
            if (other.gameObject.CompareTag("Player"))
            {
                var player = other.gameObject.GetComponent<PlayerController2D>();
                if (isBeAttackAble)
                {
                    Die();
                }
                else
                {
                    player.Die();
                }
            }
        }

        public void Restart()
        {
            sprite.color = Color.red;
            isBeAttackAble = false;
            box.enabled = true;
            gameObject.SetActive(true);
        }

        private void Die()
        {
            death.Die();
            seq.Pause();
        }

        public void OnWakeUp()
        {
            seq.Play();
            sprite.color = Color.red;
        }

        public void OnSleep()
        {
            seq.Pause();
            sprite.color = Color.gray;
        }

        public void OnMMEvent(OnMaskCollectEvent eventType)
        {
            if (mask != null)
            {
                isBeAttackAble = targetColor == eventType.turnToColor;
                if (isBeAttackAble)
                {
                    OnSleep();
                }
                else
                {
                    OnWakeUp();
                }
            }
        }

        public void OnMMEvent(LevelOutItemCollectEvent eventType)
        {
            seq.Pause();
        }
    }
}