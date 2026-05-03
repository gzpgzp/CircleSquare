using UnityEngine;
using UnityEngine.UI;
using TMPro;

namespace SquareBattle.BattleCore.UI
{
    /// <summary>
    /// 单个血条 UI 单元，由 HealthBarManager 统一管理
    /// </summary>
    public class HealthBarItem : MonoBehaviour
    {
        [SerializeField] private Image fillImage;

        [Tooltip("显示血量数字的")]
        [SerializeField] private Text hpText;

        [Tooltip("满血颜色")]
        [SerializeField] private Color fullColor = Color.green;

        [Tooltip("空血颜色")]
        [SerializeField] private Color emptyColor = Color.red;

        // 跟随的世界空间目标
        private Transform _target;
        // 目标头顶偏移（世界单位）
        private Vector3 _worldOffset;
        // 渲染用的摄像机
        private Camera _camera;

        public void Init(Transform target, Vector3 worldOffset, Camera cam)
        {
            _target = target;
            _worldOffset = worldOffset;
            _camera = cam;
        }

        private void LateUpdate()
        {
            if (_target == null)
            {
                gameObject.SetActive(false);
                return;
            }

            // 世界坐标 → 屏幕坐标
            Vector3 worldPos = _target.position + _worldOffset;
            Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(_camera, worldPos);

            // 屏幕坐标 → Canvas 内局部坐标（必须传入同一个摄像机，不能传 null）
            RectTransform rt = transform as RectTransform;
            RectTransform parentRt = rt.parent as RectTransform;
            if (RectTransformUtility.ScreenPointToLocalPointInRectangle(
                    parentRt, screenPos, _camera, out Vector2 localPos))
            {
                rt.anchoredPosition = localPos;
            }
        }

        /// <summary>
        /// 更新血条填充、颜色与血量文字
        /// </summary>
        public void SetHealth(int current, int max)
        {
            if (fillImage == null)
            {
                Debug.LogWarning($"[HealthBarItem] {name} 的 fillImage 未赋值！", this);
                return;
            }

            float ratio = max > 0 ? Mathf.Clamp01((float)current / max) : 0f;
            fillImage.fillAmount = ratio;
            fillImage.color = Color.Lerp(emptyColor, fullColor, ratio);

            if (hpText != null)
                hpText.text = $"{current}";
        }

        public void SetTarget(Transform target) => _target = target;
    }
}
