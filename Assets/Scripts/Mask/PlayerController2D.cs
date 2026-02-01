using GameFramework;
using UnityEngine;

namespace Mask
{
    public class PlayerController2D : MonoBehaviour
    {
        [Header("移动设置")] [SerializeField] private float maxSpeed = 10f;
        [SerializeField] private float acceleration = 80f;
        [SerializeField] private float deceleration = 70f;
        [SerializeField] private float groundCheckRadius = 0.2f;

        [Header("马里奥式跳跃物理")] [SerializeField] private float baseJumpHeight = 2f; // 短按跳跃高度
        [SerializeField] private float maxJumpHeight = 5f; // 长按跳跃高度
        [SerializeField] private float minJumpTime = 0.15f; // 最短跳跃上升时间
        [SerializeField] private float maxJumpTime = 0.35f; // 最长跳跃上升时间

        [Header("跳跃曲线 - 关键！")] [SerializeField]
        private AnimationCurve jumpVelocityCurve = AnimationCurve.EaseInOut(0, 1, 1, 0);

        [SerializeField] private float jumpApexHeight = 2.5f; // 跳跃顶点高度（用于曲线调整）

        [Header("物理参数")] [SerializeField] private float gravity = 35f; // 基础重力
        [SerializeField] private float maxFallSpeed = 20f; // 最大下落速度
        [SerializeField] private float apexGravityModifier = 0.7f; // 跳跃顶点附近重力减弱

        [Header("手感微调")] [SerializeField] private float jumpCutMultiplier = 0.5f; // 释放跳跃键时速度削减系数
        [SerializeField] private float coyoteTime = 0.1f; // 土狼时间
        [SerializeField] private float jumpBufferTime = 0.1f; // 跳跃缓冲时间
        [SerializeField] private float airDragMultiplier = 0.95f; // 空中阻力

        [Header("组件引用")] [SerializeField] private Transform groundCheckPoint;
        [SerializeField] private LayerMask groundLayer;
        [SerializeField] private Animator animator;
        [SerializeField] private SpriteRenderer spriteRenderer;
        [SerializeField] private ParticleSystem jumpDust;
        [SerializeField] private ParticleSystem landDust;
        [SerializeField] private DeathAnimation death;
        [SerializeField] private WearMask wearMask;
        
        // 私有变量
        private Rigidbody2D rb;
        private float horizontalInput;
        private bool jumpPressed;
        private bool jumpHeld;
        private bool jumpReleased;

        private bool isGrounded;
        private bool wasGroundedLastFrame;
        private bool isJumping;
        private bool canJumpCut;

        private float coyoteTimeCounter;
        private float jumpBufferCounter;
        private float currentJumpTime;
        private float currentGravity;

        // 跳跃物理相关
        private float initialJumpVelocity;
        private float jumpStartY;
        private bool reachedJumpApex;

        // 属性
        public bool IsGrounded => isGrounded;
        public bool IsJumping => isJumping;
        public float JumpProgress => Mathf.Clamp01(currentJumpTime / maxJumpTime);
        public float CurrentVelocityY => rb.velocity.y;

        private bool isDeath = false;
        private bool isStart = false;
        public bool isMaxAirTime => lastAirTime >= 0.23f;
        private float lastAirTime;
        
        private void Awake()
        {
            rb = GetComponent<Rigidbody2D>();
            currentGravity = gravity;

            if (groundCheckPoint == null)
                groundCheckPoint = transform;
            death.Init(OnDeath);
        }

        private void Update()
        {
            if (GuideManager.Instance.isInGuide)
            {
                return;
            }

            if (!isStart)
            {
                return;
            }

            if (isDeath)
            {
                return;
            }

            if (!isGrounded && rb.velocity.y < 0)
            {
                lastAirTime += Time.deltaTime;
            }

            GetInput();
            HandleTimers();
            HandleJumpInput();
            UpdateAnimations();

            wasGroundedLastFrame = isGrounded;
            if(lastAirTime > 0 && isGrounded)
                lastAirTime = 0f;
        }

        private void FixedUpdate()
        {
            if (GuideManager.Instance.isInGuide)
            {
                return;
            }           
            
            if (!isStart)
            {
                return;
            }

            if (isDeath)
            {
                return;
            }
            
            HandleGroundDetection();
            HandleMovement();
            HandleJumpPhysics();
            HandleGravity();
            ClampFallSpeed();
        }

        public void StartLevel()
        {
            isStart = true;
            isDeath = false;
            wearMask.Restart();
        }

        public void Die()
        {
            isDeath = true;
            death.Die();
        }

        public void Pause()
        {
            isStart = false;
            rb.velocity = Vector2.zero;
        }

        private void OnDeath()
        {
            gameObject.SetActive(false);
            GameRestartEvent.Trigger();
        }

        private void GetInput()
        {
            horizontalInput = Input.GetAxisRaw("Horizontal");

            jumpPressed = Input.GetKeyDown(KeyCode.Space);
            jumpHeld = Input.GetKey(KeyCode.Space);
            jumpReleased = Input.GetKeyUp(KeyCode.Space);

            // 跳跃缓冲
            if (jumpPressed)
                jumpBufferCounter = jumpBufferTime;
        }

        private void HandleTimers()
        {
            // 更新土狼时间
            if (isGrounded)
                coyoteTimeCounter = coyoteTime;
            else
                coyoteTimeCounter -= Time.deltaTime;

            // 更新跳跃缓冲
            jumpBufferCounter -= Time.deltaTime;
        }

        private void HandleGroundDetection()
        {
            isGrounded = Physics2D.OverlapCircle(groundCheckPoint.position, groundCheckRadius, groundLayer);

            // 落地检测
            if (!wasGroundedLastFrame && isGrounded)
            {
                OnLand();
            }
        }

        private void HandleJumpInput()
        {
            bool canJump = isGrounded || coyoteTimeCounter > 0;

            // 有跳跃输入且可以跳跃
            if (jumpBufferCounter > 0 && canJump && !isJumping)
            {
                StartJump();
                jumpBufferCounter = 0;
            }

            // 释放跳跃键时的切割效果
            if (jumpReleased && isJumping && canJumpCut && rb.velocity.y > 0)
            {
                // 削减上升速度
                rb.velocity = new Vector2(rb.velocity.x, rb.velocity.y * jumpCutMultiplier);
                canJumpCut = false;
            }
        }

        private void StartJump()
        {
            isJumping = true;
            canJumpCut = true;
            currentJumpTime = 0f;
            reachedJumpApex = false;
            jumpStartY = transform.position.y;

            // 计算初始跳跃速度（基于基础跳跃高度）
            initialJumpVelocity = Mathf.Sqrt(2f * gravity * baseJumpHeight);

            // 应用初始速度
            rb.velocity = new Vector2(rb.velocity.x, initialJumpVelocity);

            // 特效
            if (jumpDust != null)
                jumpDust.Play();

            Debug.Log($"开始跳跃 - 初始速度: {initialJumpVelocity}");
        }

        private void HandleJumpPhysics()
        {
            if (!isJumping) return;

            currentJumpTime += Time.fixedDeltaTime;

            // 如果按住跳跃键且在最大跳跃时间内
            if (jumpHeld && currentJumpTime < maxJumpTime)
            {
                // 计算目标高度（从基础高度渐变到最大高度）
                float t = currentJumpTime / maxJumpTime;
                float targetHeight = Mathf.Lerp(baseJumpHeight, maxJumpHeight, t);

                // 当前高度
                float currentHeight = transform.position.y - jumpStartY;

                // 如果当前高度低于目标高度，继续给予上升力
                if (currentHeight < targetHeight)
                {
                    // 使用曲线控制上升速度：先快后慢
                    float curveValue = jumpVelocityCurve.Evaluate(currentHeight / targetHeight);
                    float additionalVelocity = curveValue * 15f * Time.fixedDeltaTime;

                    rb.velocity += new Vector2(0, additionalVelocity);
                }
            }

            // 如果达到跳跃顶点附近，标记
            float heightFromStart = transform.position.y - jumpStartY;
            if (heightFromStart >= jumpApexHeight * 0.8f && !reachedJumpApex)
            {
                reachedJumpApex = true;
                Debug.Log("达到跳跃顶点附近");
            }

            // 如果速度开始下降或达到最大时间，结束跳跃
            if (rb.velocity.y <= 0 || currentJumpTime >= maxJumpTime)
            {
                if (rb.velocity.y <= 0)
                {
                    isJumping = false;
                    canJumpCut = false;
                }
            }
        }

        private void HandleMovement()
        {
            float targetSpeed = horizontalInput * maxSpeed;
            float currentSpeed = rb.velocity.x;
            float speedDifference = targetSpeed - currentSpeed;

            // 选择加速度（加速或减速）
            float accelRate = (Mathf.Abs(targetSpeed) > 0.01f) ? acceleration : deceleration;

            // 应用力
            float movement = speedDifference * accelRate * Time.fixedDeltaTime;
            rb.velocity += new Vector2(movement, 0);

            // 空中阻力
            if (!isGrounded && Mathf.Abs(horizontalInput) < 0.1f)
            {
                rb.velocity *= new Vector2(airDragMultiplier, 1);
            }

            // 翻转角色
            if (Mathf.Abs(horizontalInput) > 0.1f)
            {
                spriteRenderer.flipX = horizontalInput < 0;
            }
        }

        private void HandleGravity()
        {
            // 动态调整重力
            float modifiedGravity = gravity;

            // 跳跃顶点附近减少重力，让顶点更平滑
            if (reachedJumpApex && rb.velocity.y > -2f)
            {
                modifiedGravity *= apexGravityModifier;
            }

            // 应用重力
            rb.velocity += Vector2.down * modifiedGravity * Time.fixedDeltaTime;
            currentGravity = modifiedGravity;
        }

        private void ClampFallSpeed()
        {
            // 限制最大下落速度
            if (rb.velocity.y < -maxFallSpeed)
            {
                rb.velocity = new Vector2(rb.velocity.x, -maxFallSpeed);
            }
        }

        private void OnLand()
        {
            isJumping = false;
            canJumpCut = false;
            lastAirTime = currentJumpTime;
            currentJumpTime = 0f;
            reachedJumpApex = false;

            // 落地特效
            if (landDust != null && Mathf.Abs(rb.velocity.y) > 5f)
                landDust.Play();

            Debug.Log("落地");
        }

        private void UpdateAnimations()
        {
            if (animator == null) return;

            animator.SetFloat("Speed", Mathf.Abs(horizontalInput));
            animator.SetBool("IsGrounded", isGrounded);
            animator.SetFloat("VerticalVelocity", rb.velocity.y);
            animator.SetBool("IsJumping", isJumping);

            // 跳跃状态特殊处理
            if (isJumping && rb.velocity.y > 0)
            {
                animator.SetFloat("JumpHeight", JumpProgress);
            }
        }

        // 调试信息
        private void OnGUI()
        {
            GUIStyle style = new GUIStyle(GUI.skin.label);
            style.fontSize = 12;

            GUILayout.BeginArea(new Rect(10, 10, 300, 200));
            GUILayout.Label($"速度: {rb.velocity}", style);
            GUILayout.Label($"地面: {isGrounded}", style);
            GUILayout.Label($"跳跃: {isJumping}", style);
            GUILayout.Label($"跳跃进度: {JumpProgress:P0}", style);
            GUILayout.Label($"当前重力: {currentGravity:F1}", style);
            GUILayout.Label($"土狼时间: {coyoteTimeCounter:F2}", style);
            GUILayout.Label($"跳跃缓冲: {jumpBufferCounter:F2}", style);
            GUILayout.EndArea();
        }

        private void OnDrawGizmosSelected()
        {
            if (groundCheckPoint != null)
            {
                Gizmos.color = isGrounded ? Color.green : Color.red;
                Gizmos.DrawWireSphere(groundCheckPoint.position, groundCheckRadius);
            }

            // 绘制跳跃高度参考线
            if (Application.isPlaying)
            {
                Vector3 pos = transform.position;

                // 基础跳跃高度
                Gizmos.color = Color.yellow;
                Gizmos.DrawLine(pos, pos + Vector3.up * baseJumpHeight);

                // 最大跳跃高度
                Gizmos.color = Color.green;
                Gizmos.DrawLine(pos, pos + Vector3.up * maxJumpHeight);

                // 跳跃顶点线
                Gizmos.color = Color.blue;
                Gizmos.DrawLine(pos + Vector3.left * 0.5f, pos + Vector3.right * 0.5f + Vector3.up * jumpApexHeight);
            }
        }

        // 外部调用的跳跃方法
        public void ForceJump(float height)
        {
            float jumpVelocity = Mathf.Sqrt(2f * gravity * height);
            rb.velocity = new Vector2(rb.velocity.x, jumpVelocity);
            isJumping = true;
            canJumpCut = true;
            currentJumpTime = 0f;
        }
    }
}