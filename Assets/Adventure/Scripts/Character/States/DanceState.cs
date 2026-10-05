using UnityEngine;

namespace Adventure.Character
{
    /// <summary>
    /// 跳舞状态 - 角色原地播放跳舞动画
    /// 支持随机时长，动画播放完成后自动回到Idle状态
    /// </summary>
    public class DanceState : CharacterState
    {
        [Tooltip("跳舞最短时间（秒）")]
        private float minDanceDuration = 2f;

        [Tooltip("跳舞最长时间（秒）")]
        private float maxDanceDuration = 5f;

        [Tooltip("有多少种跳舞动作可选")]
        private int danceActionCount = 3;

        [Tooltip("是否支持连续跳舞（跳完一段继续跳下一段）")]
        private bool allowContinuousDance = true;

        [Tooltip("连续跳舞时，切换到下一段的概率（0-1）")]
        private float continuousDanceProbability = 0.3f;

        private float currentDanceDuration; // 本次随机生成的跳舞时长
        private float danceTimer;
        private int currentDanceAction;

        public DanceState()
        {
            StateType = CharacterStateType.Dancing;
        }

        public override void Enter()
        {
            base.Enter();
            StartNewDance();
        }

        public override void Execute()
        {
            danceTimer += Time.deltaTime;

            // 达到本次跳舞时长
            if (danceTimer >= currentDanceDuration)
            {
                OnDanceSegmentFinished();
            }
        }

        public override void Exit()
        {
            base.Exit();

            // 取消跳舞动画
            if (animatorCtrl != null)
            {
                animatorCtrl.CancelAfk();
            }
        }

        /// <summary>
        /// 开始一段新的跳舞
        /// </summary>
        private void StartNewDance()
        {
            // 随机生成本次跳舞时长
            currentDanceDuration = Random.Range(minDanceDuration, maxDanceDuration);
            
            // 随机选择一个跳舞动作
            currentDanceAction = Random.Range(0, danceActionCount);

            // 播放跳舞动画
            if (animatorCtrl != null)
            {
                animatorCtrl.PlayAfkAction(currentDanceAction);
            }

            danceTimer = 0;
        }

        /// <summary>
        /// 一段跳舞完成
        /// </summary>
        private void OnDanceSegmentFinished()
        {
            // 检查是否继续跳
            if (allowContinuousDance && Random.value < continuousDanceProbability)
            {
                // 继续下一段跳舞
                StartNewDance();
            }
            else
            {
                // 结束跳舞，回到Idle
                ChangeState(CharacterStateType.Idle);
            }
        }

        /// <summary>
        /// 动画事件调用 - 跳舞动画播放完成
        /// 在Animator中给跳舞动画添加事件调用此方法
        /// </summary>
        public void OnDanceAnimationComplete()
        {
            OnDanceSegmentFinished();
        }

        #region 配置方法

        /// <summary>
        /// 设置跳舞时长范围
        /// </summary>
        public void SetDanceDurationRange(float min, float max)
        {
            minDanceDuration = Mathf.Max(0, min);
            maxDanceDuration = Mathf.Max(minDanceDuration, max);
        }

        /// <summary>
        /// 设置可选的跳舞动作数量
        /// </summary>
        public void SetDanceActionCount(int count)
        {
            danceActionCount = Mathf.Max(1, count);
        }

        /// <summary>
        /// 设置是否允许连续跳舞
        /// </summary>
        public void SetAllowContinuousDance(bool allow)
        {
            allowContinuousDance = allow;
        }

        /// <summary>
        /// 设置连续跳舞的概率
        /// </summary>
        public void SetContinuousDanceProbability(float probability)
        {
            continuousDanceProbability = Mathf.Clamp01(probability);
        }

        #endregion
    }
}
