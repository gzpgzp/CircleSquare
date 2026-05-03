using UnityEngine;

namespace Battle.Character.CharacterGlue
{
    /// <summary>
    /// AIBrain 与所有 AI Ability 之间共享的感知数据。
    /// 由 AIBrain 每帧写入，AI Ability 只读取，实现数据与逻辑解耦。
    /// </summary>
    public class AIBlackboard
    {
        /// <summary>当前锁定的攻击目标（null = 无目标）</summary>
        public BaseCharacter Target { get; private set; }

        /// <summary>目标是否有效（存在且未死亡）</summary>
        public bool HasTarget => Target != null && !Target.IsDead();

        /// <summary>自身到目标的距离（无目标时为 float.MaxValue）</summary>
        public float DistanceToTarget { get; private set; }

        /// <summary>自身到目标的方向（无目标时为 Vector2.zero）</summary>
        public Vector2 DirectionToTarget { get; private set; }

        /// <summary>
        /// 由 AIBrain 每帧调用，更新目标信息
        /// </summary>
        public void Update(BaseCharacter self, BaseCharacter target)
        {
            Target = target;

            if (target == null || target.IsDead())
            {
                DistanceToTarget = float.MaxValue;
                DirectionToTarget = Vector2.zero;
                return;
            }

            Vector2 delta = (Vector2)(target.transform.position - self.transform.position);
            DistanceToTarget = delta.magnitude;
            DirectionToTarget = delta.magnitude > 0.001f ? delta.normalized : Vector2.zero;
        }

        /// <summary>清空目标</summary>
        public void Clear()
        {
            Target = null;
            DistanceToTarget = float.MaxValue;
            DirectionToTarget = Vector2.zero;
        }
    }
}
