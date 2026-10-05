using System;
using UnityEngine;

namespace Battle3D.Character.Ability
{
    /// <summary>
    /// 能力上下文 - 存储能力的运行时数据和配置
    /// 通过 ScriptableObject 配置，运行时实例化
    /// </summary>
    [Serializable]
    public class AbilityContext
    {
        [Header("基础信息")]
        public string abilityName;
        public string description;
        public AbilitySlot slot;

        [Header("冷却与消耗")]
        public float cooldown = 0f;
        public float energyCost = 0f;

        [Header("伤害")]
        public float damageMultiplier = 1f;
        public float knockbackForce = 0f;

        [Header("时间")]
        public float windupTime = 0f;
        public float activeTime = 0.3f;
        public float recoveryTime = 0.2f;

        [Header("标签")]
        public AbilityTag ownTag = AbilityTag.Action;
        public AbilityTag blockTags = AbilityTag.None;

        [Header("条件")]
        public bool requiresGround = false;
        public bool canCancelByDodge = true;
        public bool canMoveWhileActive = false;

        [Header("连招（仅普攻）")]
        public int comboCount = 1;
        public float comboWindow = 0.5f;
    }

    /// <summary>
    /// 能力配置资源 - 用于数据驱动的技能配置
    /// </summary>
    [CreateAssetMenu(fileName = "NewAbility", menuName = "Battle3D/Ability Config")]
    public class AbilityConfigSO : ScriptableObject
    {
        public AbilityContext context;
        public AnimationClip animationClip;
        public AudioClip activateSound;
        public AudioClip hitSound;
        public GameObject vfxPrefab;
    }
}
