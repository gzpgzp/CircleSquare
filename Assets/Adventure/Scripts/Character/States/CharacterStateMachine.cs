using UnityEngine;
using System;
using System.Collections.Generic;

namespace Adventure.Character
{
    /// <summary>
    /// 角色状态机 - 管理角色状态的切换和生命周期
    /// </summary>
    public class CharacterStateMachine : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private CharacterMover mover;
        [SerializeField] private CharacterAnimatorController animatorCtrl;

        [Header("===== Idle 待机配置 =====")]
        [Tooltip("随机行为触发的最小等待时间（秒）")]
        [SerializeField] private float idleMinTime = 3f;
        [Tooltip("随机行为触发的最大等待时间（秒）")]
        [SerializeField] private float idleMaxTime = 8f;
        [Tooltip("触发跳舞的概率（0-1）")]
        [SerializeField] private float idleDanceProbability = 0.3f;
        [Tooltip("触发战斗的概率（0-1），0表示不会自动触发")]
        [SerializeField] private float idleFightProbability = 0f;
        [Tooltip("触发随机移动的概率（0-1），0表示不会自动触发")]
        [SerializeField] private float idleRandomMoveProbability = 0.2f;
        [Tooltip("触发移动到建筑的概率（0-1），0表示不会自动触发")]
        [SerializeField] private float idleMoveToBuildingProbability = 0.1f;

        [Header("===== 随机移动配置 =====")]
        [Tooltip("随机移动时，距离角色的最大距离")]
        [SerializeField] private float randomMoveRadius = 5f;

        [Header("===== Dance 跳舞配置 =====")]
        [Tooltip("跳舞最短时间（秒）")]
        [SerializeField] private float danceMinDuration = 2f;
        [Tooltip("跳舞最长时间（秒）")]
        [SerializeField] private float danceMaxDuration = 5f;
        [Tooltip("有多少种跳舞动作可选")]
        [SerializeField] private int danceActionCount = 3;
        [Tooltip("是否支持连续跳舞（跳完一段继续跳下一段）")]
        [SerializeField] private bool allowContinuousDance = true;
        [Tooltip("连续跳舞的概率（0-1）")]
        [SerializeField] private float continuousDanceProbability = 0.3f;

        [Header("===== Fighting 战斗配置 =====")]
        [Tooltip("攻击动画时长（秒），0表示使用动画事件")]
        [SerializeField] private float attackDuration = 1f;
        [Tooltip("两次攻击之间的间隔时间（秒）")]
        [SerializeField] private float attackInterval = 0.5f;
        [Tooltip("是否自动循环攻击")]
        [SerializeField] private bool autoAttack = true;

        /// <summary>当前状态</summary>
        public CharacterState CurrentState { get; private set; }

        /// <summary>当前状态类型</summary>
        public CharacterStateType CurrentStateType => CurrentState?.StateType ?? CharacterStateType.Idle;

        /// <summary>状态改变事件</summary>
        public event Action<CharacterStateType, CharacterStateType> onStateChanged;

        /// <summary>Mover引用（供状态访问）</summary>
        public CharacterMover Mover => mover;

        /// <summary>Animator引用（供状态访问）</summary>
        public CharacterAnimatorController AnimatorCtrl => animatorCtrl;

        /// <summary>所有已注册的状态</summary>
        private Dictionary<CharacterStateType, CharacterState> states = new Dictionary<CharacterStateType, CharacterState>();

        /// <summary>是否正在被拖拽</summary>
        private bool isBeingDragged;

        private void Awake()
        {
            // 自动获取引用
            if (mover == null) mover = GetComponent<CharacterMover>();
            if (animatorCtrl == null) animatorCtrl = GetComponent<CharacterAnimatorController>();
        }

        private void Start()
        {
            // 注册所有状态并应用配置
            RegisterAllStates();

            // 监听移动状态变化
            if (mover != null)
            {
                mover.onMovingStateChanged += OnMovingStateChanged;
            }

            // 默认进入Idle状态
            ChangeState(CharacterStateType.Idle);
        }

        private void OnDestroy()
        {
            if (mover != null)
            {
                mover.onMovingStateChanged -= OnMovingStateChanged;
            }
        }

        /// <summary>
        /// 移动状态改变回调
        /// </summary>
        private void OnMovingStateChanged(bool isMoving)
        {
            // 被拖拽时不自动切换
            if (isBeingDragged) return;

            if (isMoving)
            {
                // 开始移动时，切换到Moving状态（除非正在战斗）
                if (CurrentStateType != CharacterStateType.Fighting)
                {
                    ChangeState(CharacterStateType.Moving);
                }
            }
            else
            {
                // 停止移动时，如果当前是Moving状态，回到Idle
                if (CurrentStateType == CharacterStateType.Moving)
                {
                    ChangeState(CharacterStateType.Idle);
                }
            }
        }

        private void Update()
        {
            // 被拖拽时不执行状态逻辑
            if (isBeingDragged) return;

            CurrentState?.Execute();
        }

        private void FixedUpdate()
        {
            if (isBeingDragged) return;

            CurrentState?.PhysicsExecute();
        }

        /// <summary>
        /// 注册所有状态（子类可重写以添加自定义状态）
        /// </summary>
        protected virtual void RegisterAllStates()
        {
            // Idle 状态
            var idleState = new IdleState();
            RegisterState(idleState);
            idleState.SetIdleTimeRange(idleMinTime, idleMaxTime);
            idleState.SetDanceProbability(idleDanceProbability);
            idleState.SetFightProbability(idleFightProbability);
            idleState.SetRandomMoveProbability(idleRandomMoveProbability);
            idleState.SetMoveToBuildingProbability(idleMoveToBuildingProbability);
            idleState.SetRandomMoveRadius(randomMoveRadius);

            // Moving 状态
            RegisterState(new MovingState());

            // Dance 状态
            var danceState = new DanceState();
            RegisterState(danceState);
            danceState.SetDanceDurationRange(danceMinDuration, danceMaxDuration);
            danceState.SetDanceActionCount(danceActionCount);
            danceState.SetAllowContinuousDance(allowContinuousDance);
            danceState.SetContinuousDanceProbability(continuousDanceProbability);

            // Fighting 状态
            var fightState = new FightingState();
            RegisterState(fightState);
            fightState.SetAttackDuration(attackDuration);
            fightState.SetAttackInterval(attackInterval);
            fightState.SetAutoAttack(autoAttack);
        }

        /// <summary>
        /// 注册一个状态
        /// </summary>
        public void RegisterState(CharacterState state)
        {
            state.Initialize(this);
            states[state.StateType] = state;
        }

        /// <summary>
        /// 切换到新状态
        /// </summary>
        public void ChangeState(CharacterStateType newStateType)
        {
            // 检查状态是否已注册
            if (!states.TryGetValue(newStateType, out CharacterState newState))
            {
                Debug.LogWarning($"[StateMachine] 状态 {newStateType} 未注册！");
                return;
            }

            // 状态相同则不切换
            if (CurrentState == newState) return;

            CharacterStateType oldStateType = CurrentState?.StateType ?? CharacterStateType.Idle;

            // 退出旧状态
            CurrentState?.Exit();

            // 切换
            CurrentState = newState;

            // 进入新状态
            CurrentState.Enter();

            // 触发事件
            onStateChanged?.Invoke(oldStateType, newStateType);

            Debug.Log($"[StateMachine] {oldStateType} -> {newStateType}");
        }

        /// <summary>
        /// 获取指定状态
        /// </summary>
        public T GetState<T>() where T : CharacterState
        {
            foreach (var state in states.Values)
            {
                if (state is T t) return t;
            }
            return null;
        }

        /// <summary>
        /// 设置拖拽状态（由拖拽系统调用）
        /// </summary>
        public void SetDragging(bool dragging)
        {
            isBeingDragged = dragging;
        }

        /// <summary>
        /// 手动重置状态机
        /// </summary>
        public void Reset()
        {
            isBeingDragged = false;
            ChangeState(CharacterStateType.Idle);
        }
    }
}
