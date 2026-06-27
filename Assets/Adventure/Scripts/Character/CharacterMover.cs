using UnityEngine;

namespace Adventure.Character
{
    /// <summary>
    /// 3D角色XY平面移动 - 从当前位置移动到目标位置
    /// 无碰撞检测，纯位移插值
    /// </summary>
    public class CharacterMover : MonoBehaviour
    {
        [Header("移动参数")]
        [SerializeField] private float moveSpeed = 3f;
        [SerializeField] private float arriveThreshold = 0.1f;

        /// <summary>是否正在移动</summary>
        public bool isMoving { get; private set; }

        /// <summary>当前目标点</summary>
        private Vector3 targetPosition;

        /// <summary>到达目标回调</summary>
        private System.Action onArrived;

        /// <summary>角色朝向（true=面朝右）</summary>
        public bool facingRight { get; private set; } = true;

        private void Update()
        {
            if (!isMoving) return;

            Vector3 pos = transform.position;
            Vector3 dir = targetPosition - pos;
            dir.z = 0; // 锁定Z轴

            float distance = dir.magnitude;

            if (distance <= arriveThreshold)
            {
                // 到达
                transform.position = targetPosition;
                isMoving = false;
                onArrived?.Invoke();
                onArrived = null;
                return;
            }

            // 移动
            Vector3 move = dir.normalized * moveSpeed * Time.deltaTime;
            if (move.magnitude > distance)
            {
                transform.position = targetPosition;
                isMoving = false;
                onArrived?.Invoke();
                onArrived = null;
            }
            else
            {
                transform.position += move;
            }

            // 朝向
            UpdateFacing(dir.x);
        }

        /// <summary>
        /// 移动到目标位置
        /// </summary>
        public void MoveTo(Vector3 target, System.Action onComplete = null)
        {
            targetPosition = new Vector3(target.x, target.y, transform.position.z);
            onArrived = onComplete;
            isMoving = true;

            // 更新朝向
            float dirX = targetPosition.x - transform.position.x;
            UpdateFacing(dirX);
        }

        /// <summary>
        /// 移动到指定XY坐标
        /// </summary>
        public void MoveTo(float x, float y, System.Action onComplete = null)
        {
            MoveTo(new Vector3(x, y, transform.position.z), onComplete);
        }

        /// <summary>
        /// 立即停止移动
        /// </summary>
        public void Stop()
        {
            isMoving = false;
            onArrived = null;
        }

        /// <summary>
        /// 立即传送到目标位置
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            isMoving = false;
        }

        /// <summary>
        /// 设置移动速度
        /// </summary>
        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
        }

        private void UpdateFacing(float dirX)
        {
            if (Mathf.Abs(dirX) < 0.01f) return;

            bool shouldFaceRight = dirX > 0;
            if (shouldFaceRight != facingRight)
            {
                facingRight = shouldFaceRight;
                // 通过翻转X轴scale实现朝向
                Vector3 scale = transform.localScale;
                scale.x = facingRight ? Mathf.Abs(scale.x) : -Mathf.Abs(scale.x);
                transform.localScale = scale;
            }
        }
    }
}
