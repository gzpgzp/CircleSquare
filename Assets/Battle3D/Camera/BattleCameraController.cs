using UnityEngine;
using Battle3D.Core;
using Battle3D.Character;

namespace Battle3D.Camera
{
    /// <summary>
    /// 战斗摄像机控制器 - 第三人称跟随摄像机
    /// 支持：鼠标旋转、锁定目标、碰撞检测、自动对准
    /// 推荐搭配 Cinemachine 使用，此类作为逻辑控制层
    /// </summary>
    public class BattleCameraController : MonoBehaviour
    {
        [Header("跟随目标")]
        [SerializeField] private Transform followTarget;

        [Header("跟随参数")]
        [SerializeField] private Vector3 offset = new Vector3(0f, 2f, -5f);
        [SerializeField] private float followSpeed = 8f;

        [Header("旋转参数")]
        [SerializeField] private float rotationSpeed = 3f;
        [SerializeField] private float minVerticalAngle = -30f;
        [SerializeField] private float maxVerticalAngle = 60f;

        [Header("锁定目标")]
        [SerializeField] private float lockOnRotationSpeed = 8f;
        [SerializeField] private float lockOnRange = 20f;

        [Header("碰撞")]
        [SerializeField] private float collisionRadius = 0.3f;
        [SerializeField] private LayerMask collisionLayer;

        // 旋转状态
        private float currentYaw;
        private float currentPitch = 15f;

        // 锁定目标
        private Transform lockOnTarget;
        private bool isLockedOn;

        // 摄像机Transform
        private Transform camTransform;

        public Transform FollowTarget => followTarget;
        public Transform LockOnTarget => lockOnTarget;
        public bool IsLockedOn => isLockedOn;
        public float CurrentYaw => currentYaw;
        public float CurrentPitch => currentPitch;

        private void Awake()
        {
            camTransform = UnityEngine.Camera.main?.transform;
            if (camTransform == null)
            {
                Debug.LogWarning("[BattleCamera] No main camera found!");
            }
        }

        private void Start()
        {
            if (followTarget != null)
            {
                currentYaw = followTarget.eulerAngles.y;
            }

            // 订阅事件
            GameEventSystem.Subscribe<BaseCharacter>(GameEvents.OnLockOnTarget, OnLockOnTargetEvent);
            GameEventSystem.Subscribe(GameEvents.OnUnlockTarget, OnUnlockTargetEvent);
        }

        private void LateUpdate()
        {
            if (followTarget == null || camTransform == null) return;

            if (isLockedOn && lockOnTarget != null)
            {
                UpdateLockOnCamera();
            }
            else
            {
                UpdateFreeCamera();
            }
        }

        #region 自由摄像机

        private void UpdateFreeCamera()
        {
            // 使用鼠标输入旋转（简化版，正式版通过 Input 系统）
            float mouseX = UnityEngine.Input.GetAxis("Mouse X");
            float mouseY = UnityEngine.Input.GetAxis("Mouse Y");

            currentYaw += mouseX * rotationSpeed;
            currentPitch -= mouseY * rotationSpeed;
            currentPitch = Mathf.Clamp(currentPitch, minVerticalAngle, maxVerticalAngle);

            // 计算目标位置
            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 desiredPosition = followTarget.position + rotation * offset;

            // 碰撞检测
            desiredPosition = HandleCollision(followTarget.position, desiredPosition);

            // 平滑跟随
            camTransform.position = Vector3.Lerp(camTransform.position, desiredPosition, followSpeed * Time.deltaTime);

            // 看向目标
            Vector3 lookAtPoint = followTarget.position + Vector3.up * offset.y * 0.5f;
            camTransform.LookAt(lookAtPoint);
        }

        #endregion

        #region 锁定目标摄像机

        private void UpdateLockOnCamera()
        {
            // 计算目标方向
            Vector3 directionToTarget = lockOnTarget.position - followTarget.position;
            directionToTarget.y = 0f;

            if (directionToTarget.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(directionToTarget.normalized);
                float targetYaw = targetRotation.eulerAngles.y;

                // 平滑转向目标
                currentYaw = Mathf.LerpAngle(currentYaw, targetYaw, lockOnRotationSpeed * Time.deltaTime);
            }

            currentPitch = Mathf.Lerp(currentPitch, 15f, 2f * Time.deltaTime);

            // 计算摄像机位置
            Quaternion rotation = Quaternion.Euler(currentPitch, currentYaw, 0f);
            Vector3 desiredPosition = followTarget.position + rotation * offset;
            desiredPosition = HandleCollision(followTarget.position, desiredPosition);

            camTransform.position = Vector3.Lerp(camTransform.position, desiredPosition, followSpeed * Time.deltaTime);

            // 看向角色和目标之间的中点
            Vector3 midPoint = (followTarget.position + lockOnTarget.position) * 0.5f + Vector3.up;
            camTransform.LookAt(midPoint);
        }

        #endregion

        #region 碰撞处理

        private Vector3 HandleCollision(Vector3 targetPos, Vector3 desiredPos)
        {
            Vector3 direction = desiredPos - targetPos;
            float distance = direction.magnitude;

            if (Physics.SphereCast(targetPos, collisionRadius, direction.normalized,
                out RaycastHit hit, distance, collisionLayer))
            {
                desiredPos = targetPos + direction.normalized * (hit.distance - collisionRadius);
            }

            return desiredPos;
        }

        #endregion

        #region 公共接口

        /// <summary>设置跟随目标</summary>
        public void SetFollowTarget(Transform target)
        {
            followTarget = target;
            if (target != null)
                currentYaw = target.eulerAngles.y;
        }

        /// <summary>锁定目标</summary>
        public void LockOn(Transform target)
        {
            lockOnTarget = target;
            isLockedOn = true;
        }

        /// <summary>取消锁定</summary>
        public void Unlock()
        {
            lockOnTarget = null;
            isLockedOn = false;
        }

        /// <summary>自动寻找最近的敌人并锁定</summary>
        public void AutoLockOn(LayerMask enemyLayer)
        {
            if (followTarget == null) return;

            Collider[] colliders = Physics.OverlapSphere(followTarget.position, lockOnRange, enemyLayer);
            float closestDist = float.MaxValue;
            Transform closest = null;

            foreach (var col in colliders)
            {
                var character = col.GetComponent<BaseCharacter>();
                if (character == null || character.IsPlayer) continue;
                if (!character.IsAlive) continue;

                float dist = Vector3.Distance(followTarget.position, col.transform.position);
                if (dist < closestDist)
                {
                    closestDist = dist;
                    closest = col.transform;
                }
            }

            if (closest != null)
                LockOn(closest);
        }

        #endregion

        #region 事件回调

        private void OnLockOnTargetEvent(BaseCharacter target)
        {
            if (target != null)
                LockOn(target.transform);
        }

        private void OnUnlockTargetEvent()
        {
            Unlock();
        }

        #endregion

        private void OnDestroy()
        {
            GameEventSystem.Unsubscribe<BaseCharacter>(GameEvents.OnLockOnTarget, OnLockOnTargetEvent);
            GameEventSystem.Unsubscribe(GameEvents.OnUnlockTarget, OnUnlockTargetEvent);
        }
    }
}
