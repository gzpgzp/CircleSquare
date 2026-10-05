using UnityEngine;

namespace Adventure.Character
{
    /// <summary>
    /// 战斗状态 - 角色正在战斗
    /// 必须由外部调用 EndFight() 才能退出，不会自动结束
    /// </summary>
    public class FightingState : CharacterState
    {
        [Tooltip("攻击动画时长（秒），0表示使用动画事件")]
        private float attackDuration = 1f;

        [Tooltip("两次攻击之间的间隔时间（秒）")]
        private float attackInterval = 0.5f;

        [Tooltip("是否自动循环攻击（false则需要外部触发每次攻击）")]
        private bool autoAttack = true;

        private float attackTimer;
        private float intervalTimer;
        private bool isAttacking;
        private bool isWaitingForNextAttack;
        private bool fightEnded; // 标记战斗是否已被外部结束

        public FightingState()
        {
            StateType = CharacterStateType.Fighting;
        }

        public override void Enter()
        {
            base.Enter();
            fightEnded = false;
            isAttacking = false;
            isWaitingForNextAttack = false;
            attackTimer = 0;
            intervalTimer = 0;

            // 开始第一次攻击
            StartAttack();
        }

        public override void Execute()
        {
            // 战斗已被标记结束，退出
            if (fightEnded)
            {
                ChangeState(CharacterStateType.Idle);
                return;
            }

            if (isAttacking)
            {
                // 正在攻击中
                attackTimer += Time.deltaTime;

                // 攻击动画时长为0时，等待动画事件
                if (attackDuration > 0 && attackTimer >= attackDuration)
                {
                    OnAttackAnimationComplete();
                }
            }
            else if (isWaitingForNextAttack && autoAttack)
            {
                // 等待攻击间隔
                intervalTimer += Time.deltaTime;
                if (intervalTimer >= attackInterval)
                {
                    StartAttack();
                }
            }
        }

        public override void Exit()
        {
            base.Exit();
            isAttacking = false;
            isWaitingForNextAttack = false;
            fightEnded = false;
        }

        /// <summary>
        /// 开始攻击
        /// </summary>
        private void StartAttack()
        {
            isAttacking = true;
            isWaitingForNextAttack = false;
            attackTimer = 0;

            // TODO: 触发攻击动画
            // if (animatorCtrl != null) animatorCtrl.PlayAttack();
        }

        /// <summary>
        /// 攻击动画完成（可由动画事件调用）
        /// </summary>
        public void OnAttackAnimationComplete()
        {
            isAttacking = false;

            if (!fightEnded)
            {
                // 战斗未结束，进入等待下一次攻击
                isWaitingForNextAttack = true;
                intervalTimer = 0;
            }
        }

        /// <summary>
        /// 结束战斗 - 必须由外部调用
        /// 会在当前攻击动画完成后自动切换到Idle
        /// </summary>
        public void EndFight()
        {
            fightEnded = true;
            
            // 如果没在攻击中，立即退出
            if (!isAttacking)
            {
                ChangeState(CharacterStateType.Idle);
            }
            // 否则在Execute中检测到fightEnded后会退出
        }

        /// <summary>
        /// 强制立即结束战斗（打断当前攻击）
        /// </summary>
        public void ForceEndFight()
        {
            fightEnded = true;
            isAttacking = false;
            ChangeState(CharacterStateType.Idle);
        }

        #region 配置方法

        /// <summary>
        /// 设置攻击动画时长
        /// </summary>
        public void SetAttackDuration(float duration)
        {
            attackDuration = Mathf.Max(0, duration);
        }

        /// <summary>
        /// 设置攻击间隔
        /// </summary>
        public void SetAttackInterval(float interval)
        {
            attackInterval = Mathf.Max(0, interval);
        }

        /// <summary>
        /// 设置是否自动循环攻击
        /// </summary>
        public void SetAutoAttack(bool auto)
        {
            autoAttack = auto;
        }

        /// <summary>
        /// 手动触发一次攻击（非自动模式使用）
        /// </summary>
        public void TriggerAttack()
        {
            if (!isAttacking && !fightEnded)
            {
                StartAttack();
            }
        }

        /// <summary>
        /// 是否正在攻击中
        /// </summary>
        public bool IsAttacking => isAttacking;

        /// <summary>
        /// 战斗是否已结束
        /// </summary>
        public bool IsFightEnded => fightEnded;

        #endregion
    }
}
