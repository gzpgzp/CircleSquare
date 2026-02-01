using System;
using DG.Tweening;
using UnityEngine;

namespace Mask
{
    public class MoveItem : MonoBehaviour
    {
        [SerializeField] private Transform startPos;
        [SerializeField] private Transform endPos;
        [SerializeField] private Transform moveObj;

        [SerializeField] private float moveSpeed = 3;
        [SerializeField] private float moveDuration = 1f;
        [SerializeField] private float waitTime = 0.5f;
        [SerializeField] private bool isTurn = false;
        
        protected Sequence seq;
        protected BoxCollider2D box;
        private Vector3 moveObjScale = Vector3.one;

        private void Awake()
        {
            box = GetComponent<BoxCollider2D>();
            seq = DOTween.Sequence();

            var dis = (endPos.position - moveObj.position).magnitude;
            var duration = dis / moveSpeed;
            
            var dis_2 = (endPos.position - startPos.position).magnitude;
            var duration_2 = dis_2 / moveSpeed;
            
            seq.Append(moveObj.DOMove(endPos.position, duration).SetEase(Ease.Linear))
                .AppendInterval(waitTime).OnComplete(() => {
                    if (isTurn)
                    {
                        moveObjScale.x *= -1;
                        moveObj.localScale = moveObjScale;
                    }
                })
                .Append(moveObj.DOMove(startPos.position, duration_2).SetEase(Ease.Linear))
                .AppendInterval(waitTime).OnComplete(() => {
                    if (isTurn)
                    {
                        moveObjScale.x *= -1;
                        moveObj.localScale = moveObjScale;
                    }
                })
                .SetLoops(-1); // 无限循环
            seq.Pause();
        }

        private void OnEnable()
        {
            moveObj.position = startPos.position;
            seq.Restart();
        }

        private void OnDisable()
        {
            seq?.Pause();
        }
    }
}