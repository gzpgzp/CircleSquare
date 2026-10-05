using UnityEngine;

namespace Battle3D.Camera
{
    /// <summary>
    /// 摄像机目标 - 挂载在角色上，提供摄像机跟随的参考点
    /// 通常放在角色头顶偏上的位置
    /// </summary>
    public class CameraTarget : MonoBehaviour
    {
        [Header("偏移")]
        [SerializeField] private Vector3 localOffset = new Vector3(0f, 1.5f, 0f);

        [Header("平滑")]
        [SerializeField] private float smoothSpeed = 10f;

        private Transform parentTransform;
        private Vector3 currentPos;

        private void Awake()
        {
            parentTransform = transform.parent;
            currentPos = GetTargetPosition();
        }

        private void LateUpdate()
        {
            if (parentTransform == null) return;

            Vector3 targetPos = GetTargetPosition();
            currentPos = Vector3.Lerp(currentPos, targetPos, smoothSpeed * Time.deltaTime);
            transform.position = currentPos;
        }

        private Vector3 GetTargetPosition()
        {
            if (parentTransform == null) return transform.position;
            return parentTransform.position + localOffset;
        }

        public void SetOffset(Vector3 offset)
        {
            localOffset = offset;
        }
    }
}
