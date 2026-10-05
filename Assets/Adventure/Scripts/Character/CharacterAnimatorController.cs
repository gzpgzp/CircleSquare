using Adventure.Character;
using UnityEngine;

namespace Adventure.Character
{
    /// <summary>
    /// 角色动画控制器 - 管理 Animator，控制动画状态切换
    /// 职责：监听移动/拖拽/AfK事件，切换对应的动画状态
    /// </summary>
    public class CharacterAnimatorController : MonoBehaviour
    {
        [Header("引用")]
        [SerializeField] private CharacterMover mover;
        [SerializeField] private CharacterModelLoader modelLoader;

        [Header("Animator 参数名")]
        [SerializeField] private string isMovingParam = "IsMoving";
        [SerializeField] private string isDraggingParam = "IsDragging";
        [SerializeField] private string afkActionParam = "AfkAction";
        [SerializeField] private string playAfkTrigger = "PlayAfk";
        [SerializeField] private string cancelAfkTrigger = "CancelAfk";

        // 参数 Hash（性能优化）
        private int isMovingHash;
        private int isDraggingHash;
        private int afkActionHash;
        private int playAfkHash;
        private int cancelAfkHash;

        // 当前Animator（从模型获取）
        private Animator animator;

        private void Awake()
        {
            // 自动获取引用
            if (mover == null) mover = GetComponent<CharacterMover>();
            if (modelLoader == null) modelLoader = GetComponent<CharacterModelLoader>();

            if (mover == null)
            {
                Debug.LogError("[CharacterAnimatorController] CharacterMover not found!");
                return;
            }

            // 缓存参数 Hash
            isMovingHash = Animator.StringToHash(isMovingParam);
            isDraggingHash = Animator.StringToHash(isDraggingParam);
            afkActionHash = Animator.StringToHash(afkActionParam);
            playAfkHash = Animator.StringToHash(playAfkTrigger);
            cancelAfkHash = Animator.StringToHash(cancelAfkTrigger);

            // 订阅移动状态变化
            mover.onMovingStateChanged += OnMovingStateChanged;

            // 订阅模型加载完成事件
            if (modelLoader != null)
            {
                modelLoader.onModelLoaded += OnModelLoaded;
            }
        }

        private void OnDestroy()
        {
            if (mover != null)
            {
                mover.onMovingStateChanged -= OnMovingStateChanged;
            }
            if (modelLoader != null)
            {
                modelLoader.onModelLoaded -= OnModelLoaded;
            }
        }

        /// <summary>
        /// 模型加载完成 - 获取Animator并初始化
        /// </summary>
        private void OnModelLoaded(GameObject model)
        {
            if (model == null)
            {
                Debug.LogWarning("[CharacterAnimatorController] 模型为空，无法获取Animator");
                return;
            }

            // 从模型获取Animator
            animator = model.GetComponentInChildren<Animator>();
            if (animator == null)
            {
                Debug.LogError("[CharacterAnimatorController] 模型中没有Animator组件！");
                return;
            }

            Debug.Log($"[CharacterAnimatorController] 获取Animator: {animator.name}");

            // 初始化状态
            animator.SetBool(isMovingHash, mover != null && mover.isMoving);
        }

        /// <summary>
        /// 移动状态改变回调
        /// </summary>
        private void OnMovingStateChanged(bool isMoving)
        {
            animator.SetBool(isMovingHash, isMoving);

            // 移动时打断 AfK
            if (isMoving)
            {
                animator.SetTrigger(cancelAfkHash);
            }
        }

        /// <summary>
        /// 设置拖拽状态（由拖拽系统调用）
        /// </summary>
        public void SetDragging(bool isDragging)
        {
            if (animator != null)
            {
                // 获取当前状态，避免重复设置
                bool currentState = animator.GetBool(isDraggingHash);
                if (currentState == isDragging) return;

                animator.SetBool(isDraggingHash, isDragging);
            }
        }

        /// <summary>
        /// 播放 AfK 动作（由 AfK 系统调用）
        /// </summary>
        /// <param name="actionIndex">AfK 动作索引</param>
        public void PlayAfkAction(int actionIndex)
        {
            if (animator != null)
            {
                animator.SetInteger(afkActionHash, actionIndex);
                animator.SetTrigger(playAfkHash);
            }
        }

        /// <summary>
        /// 打断 AfK 动作
        /// </summary>
        public void CancelAfk()
        {
            if (animator != null)
            {
                animator.SetTrigger(cancelAfkHash);
            }
        }
    }
}
