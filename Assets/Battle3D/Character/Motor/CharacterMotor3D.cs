using UnityEngine;

namespace Battle3D.Character.Motor
{
    /// <summary>
    /// 3D角色运动系统
    /// 使用 CharacterController 处理移动、跳跃、重力、冲刺、击退
    /// 移动方向相对于摄像机朝向
    /// </summary>
    [RequireComponent(typeof(CharacterController))]
    public class CharacterMotor3D : MonoBehaviour, ICharacterMotor
    {
        [Header("基础移动")]
        [SerializeField] private float moveSpeed = 6f;
        [SerializeField] private float rotationSpeed = 10f;

        [Header("跳跃")]
        [SerializeField] private float jumpForce = 8f;
        [SerializeField] private float gravity = -20f;

        [Header("冲刺/闪避")]
        [SerializeField] private float dodgeSpeed = 15f;

        [Header("地面检测")]
        [SerializeField] private float groundedOffset = 0.1f;
        [SerializeField] private float groundedRadius = 0.3f;
        [SerializeField] private LayerMask groundMask = ~0;

        private CharacterController controller;
        private Transform owner;

        private Vector3 velocity;
        private float verticalVelocity;
        private float defaultGravity;
        private float gravityScale = 1f;

        // 闪避状态
        private bool isDodging;
        private Vector3 dodgeDirection;
        private float dodgeTimer;
        private float dodgeDuration;

        // 击退状态
        private Vector3 knockbackVelocity;
        private float knockbackDecay = 10f;

        public Vector3 Velocity => velocity + Vector3.up * verticalVelocity;
        public bool IsGrounded { get; private set; }
        public bool CanMove { get; set; } = true;

        private void Awake()
        {
            controller = GetComponent<CharacterController>();
            defaultGravity = gravity;
        }

        public void Init(Transform ownerTransform)
        {
            owner = ownerTransform;
            velocity = Vector3.zero;
            verticalVelocity = 0f;
        }

        public void Move(Vector3 direction, float speed, float dt)
        {
            if (!CanMove || isDodging) return;
            if (direction.sqrMagnitude < 0.01f)
            {
                velocity = Vector3.zero;
                return;
            }

            // 归一化方向
            direction.Normalize();

            // 计算移动速度
            velocity = direction * speed;

            // 角色朝向旋转（朝向移动方向）
            if (direction.sqrMagnitude > 0.01f)
            {
                Quaternion targetRotation = Quaternion.LookRotation(direction);
                owner.rotation = Quaternion.Slerp(owner.rotation, targetRotation, rotationSpeed * dt);
            }
        }

        public void Jump(float force)
        {
            if (!CanMove) return;
            if (!IsGrounded) return;

            verticalVelocity = force > 0 ? force : jumpForce;
        }

        public void Dodge(Vector3 direction, float distance, float duration)
        {
            if (!CanMove) return;
            if (isDodging) return;

            isDodging = true;
            dodgeDirection = direction.normalized;
            dodgeDuration = duration;
            dodgeTimer = duration;
        }

        public void ApplyKnockback(Vector3 force)
        {
            knockbackVelocity = force;
            // 击退时短暂禁用移动
            if (force.magnitude > 5f)
            {
                verticalVelocity = force.y > 0 ? force.y : verticalVelocity;
            }
        }

        public void FixedTick(float dt)
        {
            CheckGrounded();
            ApplyGravity(dt);
            HandleDodge(dt);
            HandleKnockback(dt);
            ApplyMovement(dt);
        }

        public void StopHorizontalMovement()
        {
            velocity = Vector3.zero;
        }

        public void SetGravityScale(float scale)
        {
            gravityScale = scale;
        }

        public float GetGravityScale()
        {
            return gravityScale;
        }

        public void ResetGravityScale()
        {
            gravityScale = 1f;
        }

        private void CheckGrounded()
        {
            IsGrounded = false;

            Vector3 spherePosition = owner.position + Vector3.down * groundedOffset;
            var colliders = Physics.OverlapSphere(spherePosition, groundedRadius, groundMask, QueryTriggerInteraction.Ignore);

            foreach (var col in colliders)
            {
                // 排除自身及子物体上的所有碰撞体（模型可能自带Collider），否则 IsGrounded 恒为 true
                if (!col.transform.IsChildOf(owner))
                {
                    IsGrounded = true;
                    break;
                }
            }

            if (IsGrounded && verticalVelocity < 0f)
            {
                verticalVelocity = -2f; // 小值确保贴地
            }
        }

        private void ApplyGravity(float dt)
        {
            if (!IsGrounded)
            {
                verticalVelocity += gravity * gravityScale * dt;
            }
        }

        private void HandleDodge(float dt)
        {
            if (!isDodging) return;

            dodgeTimer -= dt;
            if (dodgeTimer <= 0f)
            {
                isDodging = false;
                return;
            }

            // 闪避期间使用冲刺速度
            velocity = dodgeDirection * dodgeSpeed;
        }

        private void HandleKnockback(float dt)
        {
            if (knockbackVelocity.sqrMagnitude > 0.01f)
            {
                knockbackVelocity = Vector3.Lerp(knockbackVelocity, Vector3.zero, knockbackDecay * dt);
            }
            else
            {
                knockbackVelocity = Vector3.zero;
            }
        }

        private void ApplyMovement(float dt)
        {
            Vector3 move = velocity + knockbackVelocity;
            move.y = verticalVelocity;

            controller.Move(move * dt);
        }

        private void OnDrawGizmosSelected()
        {
            if (owner == null) return;
            Gizmos.color = IsGrounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(owner.position + Vector3.down * groundedOffset, groundedRadius);
        }
    }
}
