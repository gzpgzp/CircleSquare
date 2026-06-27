using Adventure.Character;
using Adventure.UI;
using Cameras;
using Tools.Dialogs;
using UnityEngine;

namespace Adventure.Interaction
{
    /// <summary>
    /// 角色交互处理器 - 继承自 InteractiveObjectHandler
    /// 功能：悬停检测、长按拖动、短按打开菜单、右键旋转、滚轮缩放
    /// </summary>
    public class CharacterInteractionHandler : InteractiveObjectHandler
    {
        [Header("角色交互参数")]
        [SerializeField] private float rotationSpeed = 360f;
        [SerializeField] private float zoomSpeed = 1.5f;
        [SerializeField] private float minScale = 0.5f;
        [SerializeField] private float maxScale = 3f;

        // 旋转状态
        private CharacterMover rotatingCharacter;
        private Vector2 lastMousePos;

        protected override void Update()
        {
            base.Update();
            HandleRightClickRotate();
            HandleMouseWheelZoom();
        }

        // ========== 实现抽象方法 ==========

        protected override MonoBehaviour GetInteractiveObjectFromHit(RaycastHit hit)
        {
            return hit.collider.GetComponent<CharacterMover>();
        }

        protected override Vector3 GetObjectPosition(MonoBehaviour obj)
        {
            if (obj is CharacterMover mover)
                return mover.transform.position;
            return Vector3.zero;
        }

        // ========== 重写虚拟方法 ==========

        protected override void OnShortClick(MonoBehaviour obj)
        {
            if (obj is CharacterMover character)
            {
                OpenCharacterContextMenu(character);
            }
        }

        protected override void OnDragging(MonoBehaviour obj, Vector3 newPosition)
        {
            if (obj is CharacterMover character)
            {
                newPosition.z = character.transform.position.z; // 保持Z不变
                character.TeleportTo(newPosition);
            }
        }

        protected override void OnRightClick(MonoBehaviour obj)
        {
            // 右键旋转由 HandleRightClickRotate 处理
        }

        // ========== 角色专属功能 ==========

        /// <summary>
        /// 右键旋转 - 鼠标在角色上按住右键，左右移动旋转Y轴，上下移动旋转X轴
        /// </summary>
        private void HandleRightClickRotate()
        {
            // 右键按下
            if (Input.GetMouseButtonDown(1) && hoveredObject is CharacterMover hoveredChar)
            {
                rotatingCharacter = hoveredChar;
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
            if (Mathf.Abs(scroll) < 0.001f || hoveredObject == null) return;

            // 正在右键旋转时不缩放
            if (rotatingCharacter != null) return;

            if (hoveredObject is CharacterMover character)
            {
                Vector3 scale = character.transform.localScale;
                float currentScale = scale.x; // 假设XYZ等比缩放

                // 滚轮向上放大，向下缩小
                float newScale = currentScale + scroll * zoomSpeed;
                newScale = Mathf.Clamp(newScale, minScale, maxScale);

                // 等比缩放
                character.transform.localScale = new Vector3(newScale, newScale, newScale);
            }
        }

        /// <summary>
        /// 打开角色快捷菜单
        /// </summary>
        private void OpenCharacterContextMenu(CharacterMover character)
        {
            if (character == null || mainCamera == null) return;

            // 用主相机将角色世界坐标转换为屏幕坐标
            Vector3 screenPos = mainCamera.WorldToScreenPoint(character.transform.position);

            OpenDialogEvent.Trigger("CharacterContextMenu", new CharacterContextMenuContext
            {
                characterScreenPos = screenPos,
                characterMover = character,
                characterId = character.name
            });
        }
    }
}
