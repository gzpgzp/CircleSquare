using Adventure.UI;
using Cameras;
using Tools.Dialogs;
using UnityEngine;
using UnityEngine.Events;

namespace Adventure.Character
{
    /// <summary>
    /// 角色拖拽控制器 - 点击角色，长按拖动，右键旋转，短按弹出菜单
    /// 挂载到场景空物体上即可
    /// </summary>
    public class CharacterDragHandler : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Camera mainCamera;
        [SerializeField] private LayerMask characterLayer;

        [Header("拖动参数")]
        [SerializeField] private float longPressThreshold = 0.2f;
        [SerializeField] private float dragDeadZone = 5f;

        [Header("旋转参数")]
        [SerializeField] private float rotationSpeed = 360f; // 旋转灵敏度

        [Header("缩放参数")]
        [SerializeField] private float zoomSpeed = 1.5f; // 滚轮缩放速度
        [SerializeField] private float minScale = 0.5f; // 最小缩放
        [SerializeField] private float maxScale = 3f; // 最大缩放
        
        [Header("事件")]
        [SerializeField] private UnityEvent<CharacterMover> onCharacterShortClick;

        // 拖动状态
        private CharacterMover pressedCharacter;
        private float pressTime;
        private Vector3 pressMousePos;
        private bool isDragging;
        private Vector3 dragOffset;
        private float dragPlaneZ;

        // 旋转状态
        private CharacterMover hoveredCharacter;
        private CharacterMover rotatingCharacter;
        private Vector2 lastMousePos;

        // 左键短按检测
        private bool wasShortClick;
        private float releaseTime;

        private void Start()
        {
            if (mainCamera == null)
                mainCamera = Camera.main;
        }

        private void Update()
        {
            HandleHover();
            HandlePress();
            HandleRightClickRotate();
            HandleMouseWheelZoom();
        }

        /// <summary>
        /// 悬停检测 - 记录鼠标下的角色
        /// </summary>
        private void HandleHover()
        {
            if (isDragging) return;

            Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
            if (Physics.Raycast(ray, out RaycastHit hit, 100f, characterLayer))
            {
                var mover = hit.collider.GetComponent<CharacterMover>();
                if (mover != null)
                {
                    hoveredCharacter = mover;
                    return;
                }
            }
            hoveredCharacter = null;
        }

        private void HandlePress()
        {
            // 按下
            if (Input.GetMouseButtonDown(0))
            {
                Ray ray = mainCamera.ScreenPointToRay(Input.mousePosition);
                if (Physics.Raycast(ray, out RaycastHit hit, 100f, characterLayer))
                {
                    pressedCharacter = hit.collider.GetComponent<CharacterMover>();
                    if (pressedCharacter != null)
                    {
                        pressTime = Time.time;
                        pressMousePos = Input.mousePosition;
                        isDragging = false;

                        // 记录拖动平面信息
                        dragPlaneZ = mainCamera.WorldToScreenPoint(pressedCharacter.transform.position).z;
                        Vector3 worldPos = mainCamera.ScreenToWorldPoint(
                            new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragPlaneZ));
                        dragOffset = pressedCharacter.transform.position - worldPos;
                    }
                }
                else
                {
                    pressedCharacter = null;
                }
            }

            // 持续按住
            if (Input.GetMouseButton(0) && pressedCharacter != null)
            {
                float holdDuration = Time.time - pressTime;
                float mouseDelta = (Input.mousePosition - pressMousePos).magnitude;

                // 进入拖动模式：必须长按超过阈值
                if (!isDragging && holdDuration >= longPressThreshold)
                {
                    isDragging = true;
                }

                // 拖动中：更新角色位置
                if (isDragging)
                {
                    Vector3 screenPos = new Vector3(Input.mousePosition.x, Input.mousePosition.y, dragPlaneZ);
                    Vector3 worldPos = mainCamera.ScreenToWorldPoint(screenPos);
                    Vector3 newPos = worldPos + dragOffset;
                    newPos.z = pressedCharacter.transform.position.z; // 保持Z不变
                    pressedCharacter.TeleportTo(newPos);
                }
            }

            // 抬起
            if (Input.GetMouseButtonUp(0) && pressedCharacter != null)
            {
                if (!isDragging)
                {
                    // 没有进入拖动 = 短按，触发事件
                    wasShortClick = true;
                    releaseTime = Time.time;
                }

                isDragging = false;
                pressedCharacter = null;
            }

            // 短按延迟一帧触发（确保没有后续长按误判）
            if (wasShortClick && Time.time - releaseTime > 0.01f)
            {
                OpenCharacterContextMenu(hoveredCharacter);
                onCharacterShortClick?.Invoke(hoveredCharacter);
                wasShortClick = false;
            }
        }

        /// <summary>
        /// 打开角色快捷菜单，传递角色世界坐标
        /// </summary>
        private void OpenCharacterContextMenu(CharacterMover character)
        {
            if (character == null || mainCamera == null) return;

            // 用主相机将角色世界坐标转换为屏幕坐标
            Vector3 screenPos = mainCamera.WorldToScreenPoint(character.transform.position);

            Debug.Log($"[CharacterDragHandler] 角色世界坐标:{character.transform.position}, 屏幕坐标:{screenPos}");

            OpenDialogEvent.Trigger("CharacterContextMenu", new CharacterContextMenuContext
            {
                characterScreenPos = screenPos,
                characterMover = character,
                characterId = character.name
            });
        }

        /// <summary>
        /// 右键旋转 - 鼠标在角色上按住右键，左右移动旋转Y轴，上下移动旋转X轴
        /// </summary>
        private void HandleRightClickRotate()
        {
            // 右键按下
            if (Input.GetMouseButtonDown(1) && hoveredCharacter != null)
            {
                rotatingCharacter = hoveredCharacter;
                lastMousePos = Input.mousePosition;
            }

            // 右键持续按住
            if (Input.GetMouseButton(1) && rotatingCharacter != null)
            {
                Vector2 delta = (Vector2)Input.mousePosition - lastMousePos;
                lastMousePos = Input.mousePosition;

                // 鼠标水平移动 -> 绕Y轴旋转（左右转）
                // 鼠标垂直移动 -> 绕X轴旋转（上下转）
                float rotY = delta.x * rotationSpeed * Time.deltaTime;
                float rotX = -delta.y * rotationSpeed * Time.deltaTime;

                rotatingCharacter.transform.Rotate(rotX, rotY, 0, Space.Self);
            }

            // 右键抬起
            if (Input.GetMouseButtonUp(1))
            {
                rotatingCharacter = null;
            }
        }

        /// <summary>
        /// 鼠标滚轮缩放 - 鼠标悬停在角色上时，滚轮放大缩小角色
        /// </summary>
        private void HandleMouseWheelZoom()
        {
            float scroll = Input.GetAxis("Mouse ScrollWheel");
            if (Mathf.Abs(scroll) < 0.001f || hoveredCharacter == null) return;

            // 正在右键旋转时不缩放
            if (rotatingCharacter != null) return;

            Vector3 scale = hoveredCharacter.transform.localScale;
            float currentScale = scale.x; // 假设XYZ等比缩放

            // 滚轮向上放大，向下缩小
            float newScale = currentScale + scroll * zoomSpeed;
            newScale = Mathf.Clamp(newScale, minScale, maxScale);

            // 等比缩放
            hoveredCharacter.transform.localScale = new Vector3(newScale, newScale, newScale);
        }
    }
}
