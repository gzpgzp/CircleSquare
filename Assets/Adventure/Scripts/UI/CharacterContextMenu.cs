using Adventure.Character;
using Cameras;
using Tools.Dialogs;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

namespace Adventure.UI
{
    /// <summary>
    /// 角色快捷菜单上下文
    /// </summary>
    public class CharacterContextMenuContext : BaseUIDialogContext
    {
        /// <summary>角色屏幕坐标（MainCamera转换后的）</summary>
        public Vector3 characterScreenPos;
        /// <summary>角色Mover引用（用于复位操作）</summary>
        public CharacterMover characterMover;
        /// <summary>角色ID</summary>
        public string characterId;
    }

    /// <summary>
    /// 角色快捷菜单弹窗 - 左键点击角色时弹出，包含复位和信息两个按钮
    /// 智能定位：默认左下角，出屏幕时自动切换到其他角
    /// 预制体路径: Resources/Prefabs/UI/Dialogs/CharacterContextMenu
    /// </summary>
    public class CharacterContextMenu : BaseUIDialog<CharacterContextMenuContext>
    {
        [Header("按钮")]
        [SerializeField] private Button resetBtn;
        [SerializeField] private Button infoBtn;
        [SerializeField] private Button closeBtn;

        private RectTransform rectTransform;

        protected override void OnShowTyped(CharacterContextMenuContext context)
        {
            rectTransform = GetComponent<RectTransform>();

            // 将屏幕坐标转换为Canvas局部坐标
            Vector2 canvasPos = ScreenToCanvasPosition(context.characterScreenPos);

            // 智能定位
            SmartPosition(canvasPos);

            // 绑定按钮事件
            if (resetBtn != null)
                resetBtn.onClick.AddListener(OnResetClick);

            if (infoBtn != null)
                infoBtn.onClick.AddListener(OnInfoClick);

            if (closeBtn != null)
                closeBtn.onClick.AddListener(OnCloseClick);
        }

        protected override void OnClose()
        {
            if (resetBtn != null) resetBtn.onClick.RemoveAllListeners();
            if (infoBtn != null) infoBtn.onClick.RemoveAllListeners();
            if (closeBtn != null) closeBtn.onClick.RemoveAllListeners();
            base.OnClose();
        }

        private void Update()
        {
            // 检测鼠标左键点击其他地方
            if (Input.GetMouseButtonDown(0))
            {
                // 检查是否点击在UI上
                if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
                {
                    // 点击在UI上，不关闭
                    return;
                }
                
                // 点击在非UI区域，关闭菜单
                Close();
            }
        }

        /// <summary>
        /// 复位按钮：重置角色的旋转和缩放
        /// </summary>
        private void OnResetClick()
        {
            if (TypedContext.characterMover != null)
            {
                var transform = TypedContext.characterMover.transform;
                // 重置旋转
                transform.rotation = Quaternion.identity;
                // 重置缩放
                transform.localScale = Vector3.one;
                Debug.Log("[CharacterContextMenu] 角色已复位");
            }
            Close();
        }

        /// <summary>
        /// 信息按钮：打开角色详细信息弹窗
        /// </summary>
        private void OnInfoClick()
        {
            OpenDialogEvent.Trigger("CharacterInfoDialog", new CharacterInfoDialogContext
            {
                characterId = TypedContext.characterId
            });
            // 不调用Close()，让DialogPipe自动隐藏
            // 关闭CharacterInfoDialog后会自动恢复显示CharacterContextMenu
        }

        private void OnCloseClick()
        {
            Close();
        }

        /// <summary>
        /// 将屏幕坐标转换为Canvas局部坐标
        /// </summary>
        private Vector2 ScreenToCanvasPosition(Vector3 screenPos)
        {
            // 获取自己所在的Canvas
            Canvas uiCanvas = GetComponentInParent<Canvas>();
            if (uiCanvas == null)
            {
                Debug.LogWarning("[CharacterContextMenu] UI Canvas not found!");
                return Vector2.zero;
            }

            // 转换为Canvas局部坐标
            RectTransform canvasRect = uiCanvas.rootCanvas.transform as RectTransform;
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPos,
                uiCanvas.worldCamera, // Screen Space Camera模式需要传UI相机
                out Vector2 localPos
            );

            Debug.Log($"[CharacterContextMenu] 屏幕坐标:{screenPos} -> Canvas局部坐标:{localPos}, Canvas尺寸:{canvasRect.rect.size}");
            return localPos;
        }

        /// <summary>
        /// 智能定位：默认右下角，出屏幕时尝试其他角
        /// </summary>
        private void SmartPosition(Vector2 charCanvasPos)
        {
            // 确保RectTransform已更新
            Canvas.ForceUpdateCanvases();

            float dialogWidth = rectTransform.rect.width;
            float dialogHeight = rectTransform.rect.height;
            float padding = 10f;

            // 重置锚点到中心
            rectTransform.anchorMin = new Vector2(0.5f, 0.5f);
            rectTransform.anchorMax = new Vector2(0.5f, 0.5f);
            rectTransform.pivot = new Vector2(0.5f, 0.5f);

            // 四个候选位置：以角色位置为中心，弹窗的四个角对齐
            Vector2[] candidates = new Vector2[4];
            string[] anchorNames = new string[4] { "右下", "左下", "右上", "左上" };

            // 右下角（默认）：弹窗在角色右下方
            candidates[0] = new Vector2(charCanvasPos.x + dialogWidth / 2 + padding, charCanvasPos.y - dialogHeight / 2 - padding);
            // 左下角：弹窗在角色左下方
            candidates[1] = new Vector2(charCanvasPos.x - dialogWidth / 2 - padding, charCanvasPos.y - dialogHeight / 2 - padding);
            // 右上角：弹窗在角色右上方
            candidates[2] = new Vector2(charCanvasPos.x + dialogWidth / 2 + padding, charCanvasPos.y + dialogHeight / 2 + padding);
            // 左上角：弹窗在角色左上方
            candidates[3] = new Vector2(charCanvasPos.x - dialogWidth / 2 - padding, charCanvasPos.y + dialogHeight / 2 + padding);

            // 获取Canvas尺寸
            RectTransform canvasRect = rectTransform.parent as RectTransform;
            float canvasWidth = canvasRect != null ? canvasRect.rect.width : Screen.width;
            float canvasHeight = canvasRect != null ? canvasRect.rect.height : Screen.height;

            // 遍历候选位置，找第一个完全在Canvas内的
            for (int i = 0; i < candidates.Length; i++)
            {
                Vector2 pos = candidates[i];

                // 计算弹窗边界（锚点在中心）
                float minX = pos.x - dialogWidth / 2;
                float maxX = pos.x + dialogWidth / 2;
                float minY = pos.y - dialogHeight / 2;
                float maxY = pos.y + dialogHeight / 2;

                // 检查是否在Canvas内
                if (minX >= -canvasWidth / 2 && maxX <= canvasWidth / 2 && 
                    minY >= -canvasHeight / 2 && maxY <= canvasHeight / 2)
                {
                    rectTransform.anchoredPosition = pos;
                    Debug.Log($"[CharacterContextMenu] 弹窗定位: {anchorNames[i]}, 角色Canvas坐标: {charCanvasPos}");
                    return;
                }
            }

            // 所有位置都出屏幕，强制居中
            rectTransform.anchoredPosition = Vector2.zero;
            Debug.LogWarning("[CharacterContextMenu] 屏幕空间不足，弹窗居中显示");
        }
    }
}
