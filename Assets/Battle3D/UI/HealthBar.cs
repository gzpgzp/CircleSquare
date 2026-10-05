using Battle3D.Character;
using Battle3D.Combat;
using Battle3D.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Battle3D.UI
{
    /// <summary>
    /// 血条UI - 显示在角色头顶或主界面
    /// 支持世界空间（World Space）和屏幕空间（Screen Space）两种模式
    /// </summary>
    public class HealthBar : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Text healthText;
        [SerializeField] private Image fillImage;

        [Header("配置")]
        [SerializeField] private bool showDamageNumber = true;
        [SerializeField] private float damageNumberDuration = 1f;
        [SerializeField] private GameObject damageNumberPrefab;

        [Header("颜色")]
        [SerializeField] private Color fullHealthColor = Color.green;
        [SerializeField] private Color midHealthColor = Color.yellow;
        [SerializeField] private Color lowHealthColor = Color.red;

        private BaseCharacter targetCharacter;

        /// <summary>
        /// 绑定目标角色
        /// </summary>
        public void Bind(BaseCharacter character)
        {
            targetCharacter = character;
            UpdateHealthBar();
        }

        private void Update()
        {
            if (targetCharacter == null) return;
            UpdateHealthBar();
        }

        private void UpdateHealthBar()
        {
            if (healthSlider != null)
            {
                float normalized = targetCharacter.HealthNormalized;
                healthSlider.value = normalized;

                // 颜色渐变
                if (fillImage != null)
                {
                    fillImage.color = GetHealthColor(normalized);
                }
            }

            if (healthText != null)
            {
                healthText.text = $"{(int)targetCharacter.CurrentHealth}/{(int)targetCharacter.MaxHealth}";
            }
        }

        private Color GetHealthColor(float normalized)
        {
            if (normalized > 0.6f)
                return Color.Lerp(midHealthColor, fullHealthColor, (normalized - 0.6f) / 0.4f);
            if (normalized > 0.3f)
                return Color.Lerp(lowHealthColor, midHealthColor, (normalized - 0.3f) / 0.3f);
            return lowHealthColor;
        }

        #region 伤害数字

        /// <summary>
        /// 显示伤害数字（世界空间）
        /// </summary>
        public static void ShowDamageNumber(DamageResult result)
        {
            if (result.hitPoint == Vector3.zero) return;

            // 创建伤害数字UI
            // 在实际项目中，这里应该从对象池获取并设置位置和数值
            // 当前输出到控制台
            string prefix = result.isCritical ? "CRIT! " : "";
            Debug.Log($"[Damage] {prefix}{result.finalDamage:F0} -> {result.target?.CharacterId}");
        }

        #endregion
    }

    /// <summary>
    /// 世界空间血条 - 挂载在角色头顶
    /// 始终面向摄像机
    /// </summary>
    public class WorldHealthBar : MonoBehaviour
    {
        [SerializeField] private Slider healthSlider;
        [SerializeField] private Image fillImage;

        private BaseCharacter owner;
        private Transform cameraTransform;

        public void Init(BaseCharacter character)
        {
            owner = character;
            cameraTransform = UnityEngine.Camera.main?.transform;
        }

        private void LateUpdate()
        {
            if (owner == null) return;

            // 面向摄像机
            if (cameraTransform != null)
            {
                transform.LookAt(transform.position + cameraTransform.rotation * Vector3.forward,
                    cameraTransform.rotation * Vector3.up);
            }

            // 更新血量
            if (healthSlider != null)
            {
                healthSlider.value = owner.HealthNormalized;
            }

            // 低血量时隐藏（敌人）或始终显示（玩家）
            gameObject.SetActive(owner.IsAlive);
        }
    }
}
