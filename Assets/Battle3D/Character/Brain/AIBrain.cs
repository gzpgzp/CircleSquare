using Battle3D.AI;
using Battle3D.Character.Ability;
using UnityEngine;

namespace Battle3D.Character.Brain
{
    /// <summary>
    /// AI脑系统 - 通过状态机驱动AI行为
    /// 管理AI的决策循环和能力使用
    /// </summary>
    public class AIBrain : CharacterBrain
    {
        private StateMachine stateMachine;
        private BaseCharacter target; // 当前目标（通常是玩家）

        [Header("AI 配置")]
        public float detectRange = 15f;
        public float attackRange = 3f;
        public float patrolRange = 10f;

        public BaseCharacter Target => target;
        public StateMachine StateMachine => stateMachine;

        /// <summary>
        /// 初始化AI状态机
        /// </summary>
        public void InitStateMachine(StateMachine machine)
        {
            stateMachine = machine;
        }

        /// <summary>
        /// 设置攻击目标
        /// </summary>
        public void SetTarget(BaseCharacter targetCharacter)
        {
            target = targetCharacter;
        }

        public override void Tick(float dt)
        {
            stateMachine?.Update(dt);
        }

        public override void FixedTick(float dt)
        {
            // AI物理更新（如移动由状态驱动）
        }

        /// <summary>
        /// 获取到目标的距离
        /// </summary>
        public float GetDistanceToTarget()
        {
            if (target == null || character == null) return float.MaxValue;
            return Vector3.Distance(character.transform.position, target.transform.position);
        }

        /// <summary>
        /// 获取到目标的方向
        /// </summary>
        public Vector3 GetDirectionToTarget()
        {
            if (target == null || character == null) return Vector3.zero;
            Vector3 dir = target.transform.position - character.transform.position;
            dir.y = 0f;
            return dir.normalized;
        }

        /// <summary>
        /// 是否在攻击范围内
        /// </summary>
        public bool IsInAttackRange()
        {
            return GetDistanceToTarget() <= attackRange;
        }

        /// <summary>
        /// 是否在检测范围内
        /// </summary>
        public bool IsInDetectRange()
        {
            return GetDistanceToTarget() <= detectRange;
        }

        /// <summary>
        /// AI尝试使用指定技能
        /// </summary>
        public bool TryUseAbility(AbilitySlot slot)
        {
            return ActivateAbility(slot);
        }

        /// <summary>
        /// AI朝向目标
        /// </summary>
        public void FaceTarget(float dt)
        {
            if (target == null || character == null) return;

            Vector3 dir = GetDirectionToTarget();
            if (dir.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(dir);
                character.transform.rotation = Quaternion.Slerp(
                    character.transform.rotation, targetRotation, 5f * dt);
            }
        }

        /// <summary>
        /// AI向目标移动
        /// </summary>
        public void MoveToTarget(float dt)
        {
            if (character.Motor == null) return;

            Vector3 dir = GetDirectionToTarget();
            character.Motor.Move(dir, character.MoveSpeed, dt);
        }

        /// <summary>
        /// AI远离目标移动
        /// </summary>
        public void MoveAwayFromTarget(float dt)
        {
            if (character.Motor == null) return;

            Vector3 dir = -GetDirectionToTarget();
            character.Motor.Move(dir, character.MoveSpeed, dt);
        }
    }
}
