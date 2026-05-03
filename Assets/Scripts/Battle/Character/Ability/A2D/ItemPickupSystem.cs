using System;
using UnityEngine;

namespace Battle.Character.Ability.A2D
{
    /// <summary>
    /// 道具拾取系统：处理角色拾取道具的逻辑
    /// </summary>
    public class ItemPickupSystem : MonoBehaviour
    {
        [Header("Item Pickup")]
        [Tooltip("道具拾取检测半径")]
        [SerializeField] private float pickupRadius = 1.5f;

        [Tooltip("道具拾取特效")]
        [SerializeField] private string pickupEffectKey = "Prefabs/SquareBattle/BalloonPopExplosion";

        private BaseCharacter owner;

        private void Awake()
        {
            owner = GetComponent<BaseCharacter>();
        }

        private void OnTriggerEnter2D(Collider2D other)
        {
            if (other.CompareTag("Item"))
            {
                // 拾取道具
                PickupItem(other);
            }
        }

        /// <summary>
        /// 拾取道具
        /// </summary>
        private void PickupItem(Collider2D itemCollider)
        {
            // 播放拾取特效
            PlayPickupEffect();

            // 应用道具效果
            ApplyItemEffect(itemCollider);

            // 销毁道具
            Destroy(itemCollider.gameObject);
        }

        /// <summary>
        /// 播放拾取特效
        /// </summary>
        private void PlayPickupEffect()
        {
            if (!string.IsNullOrEmpty(pickupEffectKey))
            {
                var fx = Resources.Load<GameObject>(pickupEffectKey);
                if (fx != null)
                {
                    Instantiate(fx, transform.position, Quaternion.identity);
                }
            }
        }

        /// <summary>
        /// 应用道具效果
        /// </summary>
        private void ApplyItemEffect(Collider2D itemCollider)
        {
            // 获取道具数据
            var itemData = itemCollider.GetComponent<ItemData>();
            if (itemData == null) return;

            // 根据道具类型应用不同效果
            switch (itemData.itemType)
            {
                case ItemType.CriticalBoost:
                    ApplyCriticalBoost(itemData.duration, itemData.value);
                    break;
                case ItemType.AttackRangeBoost:
                    ApplyAttackRangeBoost(itemData.duration, itemData.value);
                    break;
                case ItemType.ComboCountBoost:
                    ApplyComboCountBoost(itemData.duration, itemData.value);
                    break;
            }
        }

        /// <summary>
        /// 应用暴击增强效果
        /// </summary>
        private void ApplyCriticalBoost(float duration, float value)
        {
            // // 查找现有的暴击修饰器
            // var criticalModifier = GetComponent<CriticalHitModifier>();
            //
            // if (criticalModifier == null)
            // {
            //     // 创建新的暴击修饰器
            //     criticalModifier = gameObject.AddComponent<CriticalHitModifier>();
            // }
            //
            // // 启用暴击效果
            // criticalModifier.EnableEffect(duration);
        }

        /// <summary>
        /// 应用攻击范围增强效果
        /// </summary>
        private void ApplyAttackRangeBoost(float duration, float value)
        {
            // 这里可以实现攻击范围增强逻辑
        }

        /// <summary>
        /// 应用连段数增强效果
        /// </summary>
        private void ApplyComboCountBoost(float duration, float value)
        {
            // 这里可以实现连段数增强逻辑
        }
    }

    /// <summary>
    /// 道具数据
    /// </summary>
    public class ItemData : MonoBehaviour
    {
        public ItemType itemType;
        public float duration;
        public float value;
    }

    /// <summary>
    /// 道具类型枚举
    /// </summary>
    public enum ItemType
    {
        None,
        CriticalBoost,
        AttackRangeBoost,
        ComboCountBoost
    }
}