using System.Collections.Generic;
using Battle3D.Character;
using Battle3D.Character.Ability;
using Battle3D.Core;
using UnityEngine;
using UnityEngine.UI;

namespace Battle3D.UI
{
    /// <summary>
    /// 技能HUD - 显示技能图标、冷却时间、能量条
    /// 主界面底部的技能操作区域
    /// </summary>
    public class SkillHUD : MonoBehaviour
    {
        [Header("技能槽位")]
        [SerializeField] private SkillSlotUI basicAttackSlot;
        [SerializeField] private SkillSlotUI dodgeSlot;
        [SerializeField] private SkillSlotUI skill1Slot;
        [SerializeField] private SkillSlotUI skill2Slot;
        [SerializeField] private SkillSlotUI ultimateSlot;

        [Header("能量条")]
        [SerializeField] private Slider energySlider;
        [SerializeField] private Image energyFill;
        [SerializeField] private Text energyText;

        [Header("血量")]
        [SerializeField] private HealthBar playerHealthBar;

        private BaseCharacter player;
        private Dictionary<AbilitySlot, SkillSlotUI> slotUIs = new Dictionary<AbilitySlot, SkillSlotUI>();

        private void Start()
        {
            // 注册槽位映射
            slotUIs[AbilitySlot.BasicAttack] = basicAttackSlot;
            slotUIs[AbilitySlot.Dodge] = dodgeSlot;
            slotUIs[AbilitySlot.Skill1] = skill1Slot;
            slotUIs[AbilitySlot.Skill2] = skill2Slot;
            slotUIs[AbilitySlot.Ultimate] = ultimateSlot;

            // 订阅能量变化事件
            GameEventSystem.Subscribe<EnergyChangedData>(GameEvents.OnEnergyChanged, OnEnergyChanged);
        }

        /// <summary>
        /// 绑定玩家角色
        /// </summary>
        public void BindPlayer(BaseCharacter playerCharacter)
        {
            player = playerCharacter;

            // 绑定血条
            playerHealthBar?.Bind(player);

            // 初始化能量显示
            OnEnergyChanged(new EnergyChangedData
            {
                current = player.AbilitySystemComponent?.CurrentEnergy ?? 0f,
                max = player.AbilitySystemComponent?.MaxEnergy ?? 100f,
            });
        }

        private void Update()
        {
            if (player == null) return;

            UpdateSkillSlots();
        }

        /// <summary>
        /// 更新所有技能槽位的冷却显示
        /// </summary>
        private void UpdateSkillSlots()
        {
            var abilitySystem = player.AbilitySystemComponent;
            if (abilitySystem == null) return;

            foreach (var kvp in slotUIs)
            {
                var slotUI = kvp.Value;
                if (slotUI == null) continue;

                var ability = abilitySystem.GetAbility(kvp.Key);
                if (ability != null)
                {
                    slotUI.UpdateCooldown(ability.CooldownNormalized, ability.CooldownRemaining);
                    slotUI.SetEnabled(ability.IsEnabled && !ability.IsOnCooldown);
                }
            }
        }

        private void OnEnergyChanged(EnergyChangedData data)
        {
            if (energySlider != null)
            {
                float normalized = data.max > 0f ? data.current / data.max : 0f;
                energySlider.value = normalized;
            }

            if (energyText != null)
            {
                energyText.text = $"{(int)data.current}/{(int)data.max}";
            }
        }

        private void OnDestroy()
        {
            GameEventSystem.Unsubscribe<EnergyChangedData>(GameEvents.OnEnergyChanged, OnEnergyChanged);
        }
    }

    /// <summary>
    /// 单个技能槽位的UI
    /// </summary>
    public class SkillSlotUI : MonoBehaviour
    {
        [SerializeField] private Image icon;
        [SerializeField] private Image cooldownOverlay;
        [SerializeField] private Text cooldownText;
        [SerializeField] private GameObject readyIndicator;
        [SerializeField] private KeyCode shortcutKey;

        [Header("快捷键显示")]
        [SerializeField] private Text shortcutText;

        private void Awake()
        {
            if (shortcutText != null && shortcutKey != KeyCode.None)
            {
                shortcutText.text = shortcutKey.ToString();
            }
        }

        /// <summary>
        /// 更新冷却显示
        /// </summary>
        public void UpdateCooldown(float normalizedCooldown, float remainingSeconds)
        {
            if (cooldownOverlay != null)
            {
                cooldownOverlay.enabled = normalizedCooldown > 0f;
                cooldownOverlay.fillAmount = normalizedCooldown;
            }

            if (cooldownText != null)
            {
                if (remainingSeconds > 0f)
                {
                    cooldownText.text = remainingSeconds.ToString("F1");
                    cooldownText.enabled = true;
                }
                else
                {
                    cooldownText.enabled = false;
                }
            }

            if (readyIndicator != null)
            {
                readyIndicator.SetActive(normalizedCooldown <= 0f);
            }
        }

        /// <summary>
        /// 设置槽位启用状态
        /// </summary>
        public void SetEnabled(bool enabled)
        {
            if (icon != null)
            {
                icon.color = enabled ? Color.white : new Color(0.5f, 0.5f, 0.5f, 0.8f);
            }
        }

        /// <summary>
        /// 设置图标
        /// </summary>
        public void SetIcon(Sprite sprite)
        {
            if (icon != null && sprite != null)
            {
                icon.sprite = sprite;
            }
        }

        /// <summary>
        /// 播放激活特效
        /// </summary>
        public void PlayActivateEffect()
        {
            // TODO: 播放技能释放的UI动画
        }
    }
}
