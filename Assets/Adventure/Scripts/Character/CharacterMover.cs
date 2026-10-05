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

        [Header("旋转参数")]
        [Tooltip("旋转平滑速度，0表示瞬间旋转")]
        [SerializeField] private float rotationSpeed = 10f;

        /// <summary>是否正在移动</summary>
        public bool isMoving { get; private set; }

        /// <summary>移动状态改变事件</summary>
        public event System.Action<bool> onMovingStateChanged;

        /// <summary>当前目标点</summary>
        private Vector3 targetPosition;

        /// <summary>到达目标回调</summary>
        private System.Action onArrived;

        /// <summary>当前朝向角度</summary>
        private float currentAngle;

        /// <summary>目标朝向角度</summary>
        private float targetAngle;

        private void Awake()
        {
            // 初始化角度为0（朝下/正面）
            currentAngle = 0;
            targetAngle = 0;
        }

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
                SetMoving(false);
                onArrived?.Invoke();
                onArrived = null;
                return;
            }

            // 移动
            Vector3 move = dir.normalized * moveSpeed * Time.deltaTime;
            if (move.magnitude > distance)
            {
                transform.position = targetPosition;
                SetMoving(false);
                onArrived?.Invoke();
                onArrived = null;
            }
            else
            {
                transform.position += move;
            }

            // 更新朝向旋转
            UpdateFacingRotation(dir);
        }

        /// <summary>
        /// 移动到目标位置
        /// </summary>
        public void MoveTo(Vector3 target, System.Action onComplete = null)
        {
            targetPosition = new Vector3(target.x, target.y, transform.position.z);
            onArrived = onComplete;
            SetMoving(true);

            // 计算目标角度
            Vector3 dir = targetPosition - transform.position;
            targetAngle = CalculateTargetAngle(dir);
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
            SetMoving(false);
            onArrived = null;
        }

        /// <summary>
        /// 立即传送到目标位置
        /// </summary>
        public void TeleportTo(Vector3 position)
        {
            transform.position = new Vector3(position.x, position.y, transform.position.z);
            SetMoving(false);
        }

        /// <summary>
        /// 设置移动速度
        /// </summary>
        public void SetSpeed(float speed)
        {
            moveSpeed = speed;
        }

        /// <summary>
        /// 根据移动方向计算目标角度（固定四向）
        /// 向下=0°，向左=90°，向上=180°，向右=270°
        /// 绕Y轴旋转
        /// </summary>
        private float CalculateTargetAngle(Vector3 direction)
        {
            float absX = Mathf.Abs(direction.x);
            float absY = Mathf.Abs(direction.y);

            // 判断主要方向
            if (absX > absY)
            {
                // 水平方向为主
                if (direction.x > 0)
                    return 90f; // 向右
                else
                    return 270f;  // 向左
            }
            else
            {
                // 垂直方向为主
                if (direction.y > 0)
                    return 0f; // 向上
                else
                    return 180f;   // 向下
            }
        }

        /// <summary>
        /// 更新朝向旋转（固定四向，绕Y轴）
        /// </summary>
        private void UpdateFacingRotation(Vector3 direction)
        {
            // 计算目标角度（固定四向）
            targetAngle = CalculateTargetAngle(direction);

            // 当前Y轴旋转角度
            float currentY = transform.eulerAngles.y;

            // 平滑旋转
            if (rotationSpeed > 0)
            {
                float newY = Mathf.LerpAngle(currentY, targetAngle, Time.deltaTime * rotationSpeed);
                transform.rotation = Quaternion.Euler(0, newY, 0);
            }
            else
            {
                transform.rotation = Quaternion.Euler(0, targetAngle, 0);
            }
        }

        /// <summary>
        /// 设置移动状态并触发事件
        /// </summary>
        private void SetMoving(bool value)
        {
            if (isMoving == value) return;
            isMoving = value;
            onMovingStateChanged?.Invoke(isMoving);
        }
    }
}
