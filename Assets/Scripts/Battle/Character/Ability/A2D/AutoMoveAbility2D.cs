using UnityEngine;

namespace Battle.Character.Ability.A2D
{
    /// <summary>
    /// AI 自动向最近敌人移动（2D 俯视）。
    /// 适用于 Archer、DogDun 等需要保持追击/接近的角色。
    ///
    /// 行为：
    ///   - 每帧从 Blackboard 读取目标方向
    ///   - 目标存在且距离超过 stopRange → 朝目标匀速移动并翻转 Sprite
    ///   - 距离 ≤ stopRange → 停止移动（原地等待攻击）
    /// </summary>
    [Ability(AbilityEnum.AutoMove2D)]
    public class AutoMoveAbility2D : AIAbility2D
    {
        /// <summary>移动速度（单位/秒）</summary>
        public float moveSpeed = 4f;

        /// <summary>与目标的停止距离（进入此范围内停止追击）</summary>
        public float stopRange = 2f;

        public AutoMoveAbility2D()
        {
            OwnTag = AbilityTag.Movement;
            isEnabled = true;
        }

        protected override void OnTickUpdate(float deltaTime)
        {
            if (blackboard == null || !blackboard.HasTarget)
            {
                character2D.motor.SetVelocity(Vector2.zero);
                return;
            }

            float dist = blackboard.DistanceToTarget;

            if (dist <= stopRange)
            {
                character2D.motor.SetVelocity(Vector2.zero);
                return;
            }

            Vector2 dir = blackboard.DirectionToTarget;

            // 朝向目标翻转 Sprite（Sprite 默认面左）
            Vector3 scale = character2D.transform.localScale;
            scale.x = dir.x > 0f ? -Mathf.Abs(scale.x) : Mathf.Abs(scale.x);
            character2D.transform.localScale = scale;

            character2D.motor.SetVelocity(dir * moveSpeed);
        }
    }
}
