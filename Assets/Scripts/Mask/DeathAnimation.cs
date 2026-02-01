using System;
using UnityEngine;
using DG.Tweening;
using Sequence = DG.Tweening.Sequence;

namespace Mask
{
    public class DeathAnimation : MonoBehaviour
    {
        [Header("Jump")] public float jumpHeight = 0.6f;
        public float jumpDuration = 0.25f;

        [Header("Fall")] public float fallDistance = 8f;
        public float fallDuration = 0.6f;
        public Ease fallEase = Ease.InQuad;

        [Header("Optional")] public float rotateSpeed = 360f; // 0 = 不旋转
        public bool fadeOut = false;

        Collider2D col;
        SpriteRenderer sr;
        bool isDead = false;
        private Action onDeath;

        void Awake()
        {
            col = GetComponent<Collider2D>();
            sr = GetComponent<SpriteRenderer>();
        }

        public void Init(Action onDeath)
        {
            this.onDeath = onDeath;
        }

        public void Die()
        {
            if (isDead) return;
            isDead = true;

            // 1️⃣ 无视碰撞
            if (col) col.enabled = false;

            transform.DOKill();

            Vector3 startPos = transform.position;

            Sequence seq = DOTween.Sequence();

            // 2️⃣ 向上跳一下
            seq.Append(
                transform.DOMoveY(startPos.y + jumpHeight, jumpDuration)
                    .SetEase(Ease.OutQuad)
            );

            // 3️⃣ 往下掉
            seq.Append(
                transform.DOMoveY(startPos.y - fallDistance, fallDuration)
                    .SetEase(fallEase)
            );

            // 4️⃣ 可选旋转
            if (rotateSpeed > 0f)
            {
                transform.DORotate(
                    new Vector3(0, 0, rotateSpeed),
                    jumpDuration + fallDuration,
                    RotateMode.FastBeyond360
                );
            }

            // 5️⃣ 可选渐隐
            if (fadeOut && sr != null)
            {
                sr.DOFade(0f, jumpDuration + fallDuration);
            }

            // 6️⃣ 完成销毁
            seq.OnComplete(() => { onDeath?.Invoke(); });
        }
    }
}