using UnityEngine;

namespace Adventure.Interaction
{
    /// <summary>
    /// 可交互对象基类 - 提供通用的射线检测、悬停、拖动、点击逻辑
    /// 继承此类的脚本需要实现具体的交互行为
    /// </summary>
    public abstract class InteractiveObjectHandler : MonoBehaviour
    {
        [Header("基础引用")]
        [SerializeField] protected Camera mainCamera;
        [SerializeField] protected LayerMask interactiveLayer;

        [Header("交互参数")]
        [SerializeField] protected float longPressThreshold = 0.2f;
        [SerializeField] protected float dragDeadZone = 5f;

        // 悬停状态
        protected MonoBehaviour hoveredObject;

        // 拖动状态
        protected MonoBehaviour pressedObject;
        protected float pressTime;
        protected Vector3 pressMousePos;
        protected bool isDragging;
        protected Vector3 dragOffset;
        protected float dragPlaneZ;

        protected virtual void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        protected virtual void Update()
        {
            HandleHover();
            HandlePress();
            HandleRightClick();
        }

        /// <summary>
        /// 悬停检测 - 需要子类提供射线命中时的对象获取逻辑
        /// </summary>
        private void HandleHover()
        {
            if (isDragging) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactiveLayer))
            {
                var obj = GetInteractiveObjectFromHit(hit);
                if (obj != null)
                {
                    if (hoveredObject != obj)
                    {
                        OnHoverExit(hoveredObject);
                        hoveredObject = obj;
                        OnHoverEnter(hoveredObject);
                    }
                    return;
                }
            }

            if (hoveredObject != null)
            {
                OnHoverExit(hoveredObject);
                hoveredObject = null;
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
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactiveLayer))
                {
                    pressedObject = GetInteractiveObjectFromHit(hit);
                    if (pressedObject != null)
                    {
                        pressTime = Time.time;
                        pressMousePos = Input.mousePosition;
                        isDragging = false;

                        // 记录拖动平面信息
                        dragPlaneZ = mainCamera.WorldToScreenPoint(GetObjectPosition(pressedObject)).z;
                        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                            new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragPlaneZ));
                        dragOffset = GetObjectPosition(pressedObject) - worldPos;

                        OnPressStart(pressedObject);
                    }
                }
                else
                {
                    pressedObject = null;
                }
            }

            // 持续按住
            if (Input.GetMouseButton(0) && pressedObject != null)
            {
                float holdDuration = Time.time - pressTime;

                // 进入拖动模式：必须长按超过阈值
                if (!isDragging && holdDuration >= longPressThreshold)
                {
                    isDragging = true;
                    OnDragStart(pressedObject);
                }

                // 拖动中
                if (isDragging)
                {
                    Vector3 screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragPlaneZ);
                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
                    Vector3 newPos = worldPos + dragOffset;
                    OnDragging(pressedObject, newPos);
                }
            }

            // 抬起
            if (Input.GetMouseButtonUp(0) && pressedObject != null)
            {
                if (!isDragging)
                {
                    // 没有进入拖动 = 短按
                    OnShortClick(pressedObject);
                }
                else
                {
                    OnDragEnd(pressedObject);
                }

                OnPressEnd(pressedObject);
                isDragging = false;
                pressedObject = null;
            }
        }

        /// <summary>
        /// 右键点击检测
        /// </summary>
        private void HandleRightClick()
        {
            if (!Input.GetMouseButtonDown(1)) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, interactiveLayer))
            {
                var obj = GetInteractiveObjectFromHit(hit);
                if (obj != null)
                {
                    OnRightClick(obj);
                }
            }
        }

        // ========== 抽象方法 - 子类必须实现 ==========

        /// <summary>
        /// 从射线命中结果中获取交互对象
        /// </summary>
        protected abstract MonoBehaviour GetInteractiveObjectFromHit(RaycastHit hit);

        /// <summary>
        /// 获取对象的世界坐标位置
        /// </summary>
        protected abstract Vector3 GetObjectPosition(MonoBehaviour obj);

        // ========== 虚拟方法 - 子类可选重写 ==========

        /// <summary>
        /// 悬停进入
        /// </summary>
        protected virtual void OnHoverEnter(MonoBehaviour obj) { }

        /// <summary>
        /// 悬停退出
        /// </summary>
        protected virtual void OnHoverExit(MonoBehaviour obj) { }

        /// <summary>
        /// 按下开始
        /// </summary>
        protected virtual void OnPressStart(MonoBehaviour obj) { }

        /// <summary>
        /// 按下结束
        /// </summary>
        protected virtual void OnPressEnd(MonoBehaviour obj) { }

        /// <summary>
        /// 短按点击
        /// </summary>
        protected virtual void OnShortClick(MonoBehaviour obj) { }

        /// <summary>
        /// 拖动开始
        /// </summary>
        protected virtual void OnDragStart(MonoBehaviour obj) { }

        /// <summary>
        /// 拖动中
        /// </summary>
        protected virtual void OnDragging(MonoBehaviour obj, Vector3 newPosition) { }

        /// <summary>
        /// 拖动结束
        /// </summary>
        protected virtual void OnDragEnd(MonoBehaviour obj) { }

        /// <summary>
        /// 右键点击
        /// </summary>
        protected virtual void OnRightClick(MonoBehaviour obj) { }
    }
}
