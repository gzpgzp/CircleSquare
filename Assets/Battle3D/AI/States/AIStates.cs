using Battle3D.Character;
using Battle3D.Character.Ability;
using UnityEngine;

namespace Battle3D.AI.States
{
    /// <summary>
    /// 空闲状态 - AI无事可做时的默认状态
    /// 检测周围环境，发现目标后切换到追击或巡逻
    /// </summary>
    public class IdleState : BaseState
    {
        public override AIStateEnum StateId => AIStateEnum.Idle;

        private float idleTimer;
        private float maxIdleTime = 2f;

        public IdleState(BaseCharacter character, StateMachine machine) : base(character, machine) { }

        public override void Enter()
        {
            idleTimer = 0f;
            character.Motor?.StopHorizontalMovement();

            // 播放待机动画
            var char3D = character as Character3D;
            char3D?.AnimatorComponent?.SetFloat("MoveSpeed", 0f);
        }

        public override void Update(float deltaTime)
        {
            idleTimer += deltaTime;

            // 检测目标
            if (aiBrain != null && aiBrain.IsInDetectRange())
            {
                if (aiBrain.IsInAttackRange())
                    machine.ChangeState(AIStateEnum.Attack);
                else
                    machine.ChangeState(AIStateEnum.Chase);
                return;
            }

            // 空闲超时，切换到巡逻
            if (idleTimer >= maxIdleTime)
            {
                machine.ChangeState(AIStateEnum.Patrol);
            }
        }
    }

    /// <summary>
    /// 巡逻状态 - AI在出生点附近随机移动
    /// </summary>
    public class PatrolState : BaseState
    {
        public override AIStateEnum StateId => AIStateEnum.Patrol;

        private Vector3 patrolTarget;
        private float patrolTimer;
        private float waitTime = 1.5f;
        private float patrolRange = 5f;
        private bool isWaiting;

        public PatrolState(BaseCharacter character, StateMachine machine) : base(character, machine) { }

        public override void Enter()
        {
            isWaiting = false;
            patrolTimer = 0f;
            ChooseNewPatrolTarget();
        }

        public override void Update(float deltaTime)
        {
            // 优先检测目标
            if (aiBrain != null && aiBrain.IsInDetectRange())
            {
                machine.ChangeState(AIStateEnum.Chase);
                return;
            }

            if (isWaiting)
            {
                patrolTimer += deltaTime;
                if (patrolTimer >= waitTime)
                {
                    isWaiting = false;
                    ChooseNewPatrolTarget();
                }
                return;
            }

            // 向巡逻点移动
            Vector3 direction = (patrolTarget - character.transform.position);
            direction.y = 0f;

            if (direction.magnitude < 0.5f)
            {
                // 到达巡逻点，等待
                isWaiting = true;
                patrolTimer = 0f;
                character.Motor?.StopHorizontalMovement();
                return;
            }

            direction.Normalize();
            character.Motor?.Move(direction, character.MoveSpeed * 0.5f, deltaTime);
        }

        private void ChooseNewPatrolTarget()
        {
            Vector2 randomCircle = Random.insideUnitCircle * patrolRange;
            patrolTarget = character.transform.position + new Vector3(randomCircle.x, 0f, randomCircle.y);
        }
    }

    /// <summary>
    /// 追击状态 - AI向目标移动
    /// </summary>
    public class ChaseState : BaseState
    {
        public override AIStateEnum StateId => AIStateEnum.Chase;

        public ChaseState(BaseCharacter character, StateMachine machine) : base(character, machine) { }

        public override void Update(float deltaTime)
        {
            if (aiBrain == null || aiBrain.Target == null || !aiBrain.Target.IsAlive)
            {
                machine.ChangeState(AIStateEnum.Idle);
                return;
            }

            // 超出检测范围，放弃追击
            if (!aiBrain.IsInDetectRange())
            {
                machine.ChangeState(AIStateEnum.Idle);
                return;
            }

            // 进入攻击范围
            if (aiBrain.IsInAttackRange())
            {
                machine.ChangeState(AIStateEnum.Attack);
                return;
            }

            // 朝向并移动向目标
            aiBrain.FaceTarget(deltaTime);
            aiBrain.MoveToTarget(deltaTime);
        }
    }

    /// <summary>
    /// 攻击状态 - AI执行攻击
    /// </summary>
    public class AttackState : BaseState
    {
        public override AIStateEnum StateId => AIStateEnum.Attack;

        private float attackCooldown;
        private float attackInterval = 1.5f;
        private float attackTimer;

        public AttackState(BaseCharacter character, StateMachine machine) : base(character, machine) { }

        public override void Enter()
        {
            attackTimer = 0f;
            character.Motor?.StopHorizontalMovement();

            // 执行攻击
            PerformAttack();
        }

        public override void Update(float deltaTime)
        {
            if (aiBrain == null || aiBrain.Target == null || !aiBrain.Target.IsAlive)
            {
                machine.ChangeState(AIStateEnum.Idle);
                return;
            }

            // 离开攻击范围，重新追击
            if (!aiBrain.IsInAttackRange())
            {
                machine.ChangeState(AIStateEnum.Chase);
                return;
            }

            // 攻击间隔
            attackTimer += deltaTime;
            if (attackTimer >= attackInterval)
            {
                attackTimer = 0f;
                PerformAttack();
            }

            // 朝向目标
            aiBrain.FaceTarget(deltaTime);

            // 低血量时后撤
            if (character.HealthNormalized < 0.2f)
            {
                machine.ChangeState(AIStateEnum.Retreat);
            }
        }

        private void PerformAttack()
        {
            aiBrain?.FaceTarget(0.1f);
            aiBrain?.TryUseAbility(AbilitySlot.BasicAttack);
        }
    }

    /// <summary>
    /// 后撤状态 - AI低血量时远离目标
    /// </summary>
    public class RetreatState : BaseState
    {
        public override AIStateEnum StateId => AIStateEnum.Retreat;

        private float retreatTimer;
        private float retreatDuration = 2f;

        public RetreatState(BaseCharacter character, StateMachine machine) : base(character, machine) { }

        public override void Enter()
        {
            retreatTimer = 0f;
        }

        public override void Update(float deltaTime)
        {
            retreatTimer += deltaTime;

            if (retreatTimer >= retreatDuration)
            {
                machine.ChangeState(AIStateEnum.Idle);
                return;
            }

            // 远离目标
            aiBrain?.MoveAwayFromTarget(deltaTime);

            // 血量恢复到安全线
            if (character.HealthNormalized > 0.4f)
            {
                machine.ChangeState(AIStateEnum.Chase);
            }
        }
    }

    /// <summary>
    /// 硬直状态 - AI受击后短暂无法行动
    /// </summary>
    public class StunState : BaseState
    {
        public override AIStateEnum StateId => AIStateEnum.Stun;

        private float stunTimer;
        private float stunDuration = 0.5f;

        public StunState(BaseCharacter character, StateMachine machine) : base(character, machine) { }

        public override void Enter()
        {
            stunTimer = 0f;
            character.Motor?.StopHorizontalMovement();
            if (character.Motor != null)
                character.Motor.CanMove = false;
        }

        public override void Update(float deltaTime)
        {
            stunTimer += deltaTime;

            if (stunTimer >= stunDuration)
            {
                if (character.Motor != null)
                    character.Motor.CanMove = true;

                // 恢复到之前的状态
                machine.ChangeToPreviousState();
            }
        }
    }

    /// <summary>
    /// 死亡状态 - AI死亡后的最终状态
    /// </summary>
    public class DeadState : BaseState
    {
        public override AIStateEnum StateId => AIStateEnum.Dead;

        public DeadState(BaseCharacter character, StateMachine machine) : base(character, machine) { }

        public override void Enter()
        {
            character.Motor?.StopHorizontalMovement();
            if (character.Motor != null)
                character.Motor.CanMove = false;
        }

        public override void Update(float deltaTime)
        {
            // 死亡状态不做任何事
        }
    }
}
