using UnityEngine;

namespace Adventure.Location
{
    /// <summary>
    /// 地点点击检测 - 点按打开功能，长按拖动建筑
    /// </summary>
    public class LocationClickHandler : MonoBehaviour
    {
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask locationLayer;

        [Header("拖动参数")]
        [SerializeField] private float longPressThreshold = 0.3f;
        [SerializeField] private float dragDeadZone = 5f;

        private LocationPoint currentHover;

        // 拖动状态
        private LocationPoint pressedLocation;
        private float pressTime;
        private Vector3 pressMousePos;
        private bool isDragging;
        private Vector3 dragOffset;
        private float dragPlaneZ;

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleHover();
            HandlePress();
            HandleRightClick();
        }

        /// <summary>
        /// 鼠标悬停检测 - 高亮建筑
        /// </summary>
        private void HandleHover()
        {
            if (isDragging) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, locationLayer))
            {
                var location = hit.collider.GetComponent<LocationPoint>();
                if (location != null)
                {
                    if (currentHover != location)
                    {
                        currentHover?.SetHighlight(false);
                        currentHover = location;
                        currentHover.SetHighlight(true);
                    }
                    return;
                }
            }

            if (currentHover != null)
            {
                currentHover.SetHighlight(false);
                currentHover = null;
            }
        }

        /// <summary>
        /// 统一处理按下/长按/拖动/抬起
        /// </summary>
        private void HandlePress()
        {
            // 按下
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, locationLayer))
                {
                    pressedLocation = hit.collider.GetComponent<LocationPoint>();
                    if (pressedLocation != null)
                    {
                        pressTime = Time.time;
                        pressMousePos = Input.mousePosition;
                        isDragging = false;

                        // 记录拖动平面信息
                        dragPlaneZ = mainCamera.WorldToScreenPoint(pressedLocation.transform.position).z;
                        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                            new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragPlaneZ));
                        dragOffset = pressedLocation.transform.position - worldPos;
                    }
                }
                else
                {
                    pressedLocation = null;
                }
            }

            // 持续按住
            if (Input.GetMouseButton(0) && pressedLocation != null)
            {
                float holdDuration = Time.time - pressTime;
                float mouseDelta = (Input.mousePosition - pressMousePos).magnitude;

                // 进入拖动模式：长按超过阈值 或 移动超过死区
                if (!isDragging && (holdDuration >= longPressThreshold || mouseDelta > dragDeadZone))
                {
                    // 只有长按才拖动，快速滑动不触发
                    if (holdDuration >= longPressThreshold)
                    {
                        isDragging = true;
                    }
                }

                // 拖动中：更新建筑位置
                if (isDragging)
                {
                    Vector3 screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragPlaneZ);
                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
                    Vector3 newPos = worldPos + dragOffset;
                    newPos.z = pressedLocation.transform.position.z; // 保持Z不变
                    pressedLocation.transform.position = newPos;
                }
            }

            // 抬起
            if (Input.GetMouseButtonUp(0) && pressedLocation != null)
            {
                if (!isDragging)
                {
                    // 没有拖动 = 点击，打开功能
                    pressedLocation.OnClicked();
                }

                // 重置状态
                isDragging = false;
                pressedLocation = null;
            }
        }

        /// <summary>
        /// 右键点击 - 触发建筑的右键菜单/额外功能
        /// </summary>
        private void HandleRightClick()
        {
            if (!Input.GetMouseButtonDown(1)) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);

            if (Physics.Raycast(ray, out RaycastHit hit, 100f, locationLayer))
            {
                var location = hit.collider.GetComponent<LocationPoint>();
                if (location != null)
                {
                    location.OnRightClicked();
                }
            }
        }
    }
}
