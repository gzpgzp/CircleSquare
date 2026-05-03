using SquareBattle.BattleCore;
using SquareBattle.BattleCore.Characters;
using UnityEngine;

namespace Battle.Character.Ability.A2D
{
    /// <summary>
    /// DogDun 盾牌绕轨能力。
    ///
    /// 行为：
    ///   - BindCharacter 时实例化 shieldPrefab，调用旧 Shield.Init()
    ///   - 每帧无需额外操作（Shield 自己在 Update 中旋转）
    ///   - 角色死亡（OnUnactive）时销毁盾牌
    ///
    /// 依赖旧系统 Shield.cs，通过直接 Instantiate 创建盾牌实例。
    /// </summary>
    [Ability(AbilityEnum.ShieldOrbit2D)]
    public class ShieldOrbitAbility2D : AIAbility2D
    {
        /// <summary>盾牌预制体（需挂 Shield.cs，Collider2D Tag=Wall 或 Trigger）</summary>
        public GameObject shieldPrefab;

        /// <summary>旋转速度（度/秒）</summary>
        public float rotateSpeed = 180f;

        /// <summary>起始角度（度）</summary>
        public float startAngle = 0f;

        private Shield _shield;

        public ShieldOrbitAbility2D()
        {
            // 盾牌是被动能力，不属于 Action 也不属于 Movement，单独标记为 None
            OwnTag = AbilityTag.None;
            isEnabled = true;
        }

        public override void BindCharacter(BaseCharacter owner)
        {
            base.BindCharacter(owner);
            SpawnShield();
        }

        protected override void OnUnactive()
        {
            // Ability 被禁用（角色死亡等）时销毁盾牌
            if (_shield != null)
            {
                Object.Destroy(_shield.gameObject);
                _shield = null;
            }
        }

        protected override void OnTickUpdate(float deltaTime)
        {
            // Shield MonoBehaviour 自己在 Update 里处理旋转，这里不需要额外逻辑
        }

        // ──────────────────────── 辅助 ────────────────────────

        private void SpawnShield()
        {
            if (shieldPrefab == null)
            {
                Debug.LogWarning("[ShieldOrbitAbility2D] shieldPrefab 未赋值，无法生成盾牌！");
                return;
            }

            var go = Object.Instantiate(
                shieldPrefab,
                character2D.transform.position,
                Quaternion.identity
            );

            _shield = go.GetComponent<Shield>();
            if (_shield == null)
            {
                Debug.LogWarning("[ShieldOrbitAbility2D] shieldPrefab 上没有 Shield 组件！");
                Object.Destroy(go);
                return;
            }

            // 旧系统 Shield 需要 Camp，新系统没有 Camp 概念
            // 默认传 Camp.Camp1；若后续新系统引入 Camp，在此替换
            _shield.Init(character2D.transform, Camp.Camp1, startAngle, rotateSpeed);
        }
    }
}
