using System.Collections;
using UnityEngine;

namespace Battle.Character.Ability
{
    public class DashAbility : BaseAbility
    {
        [Header("基础设置")] public Rigidbody2D rb;
        public float dashSpeed = 20f; // 冲刺速度
        public float dashDuration = 0.15f; // 冲刺持续时间
        public float dashCooldown = 0.6f; // 冲刺冷却时间（秒）

        [Header("输入设置")] public bool useRightMouseButton = true; // 用右键触发
        public KeyCode dashKey = KeyCode.LeftShift; // 如果不用右键，可以用键盘

        private bool isDashing = false;
        private float dashTimer = 0f;

        public override void Init(BaseCharacter character)
        {
            base.Init(character);
            rb = character.GetComponent<Rigidbody2D>();
        }
        
        public void Dash(Vector3 dir)
        {
            // 冷却计时
            if (dashTimer > 0f)
                dashTimer -= Time.deltaTime;

            // 输入检测
            bool dashPressed = false;

            if (useRightMouseButton && Input.GetMouseButtonDown(1)) // 右键
                dashPressed = true;

            if (!useRightMouseButton && Input.GetKeyDown(dashKey)) // 键盘
                dashPressed = true;

            if (dashPressed && !isDashing && dashTimer <= 0f)
            {
                if (dir.sqrMagnitude > 0.001f)
                {
                    dir.Normalize();
                    // StartCoroutine(DashRoutine(dir));
                }
            }
        }

        private IEnumerator DashRoutine(Vector2 direction)
        {
            isDashing = true;
            dashTimer = dashCooldown;

            // 记录原本的重力
            float originalGravity = rb.gravityScale;

            // 冲刺时关掉重力（不想关可以注释掉）
            rb.gravityScale = 0f;
            var originVelocity = rb.velocity;

            // 可以先清掉原来的速度，避免方向乱飘
            rb.velocity = Vector2.zero;

            float timer = 0f;

            while (timer < dashDuration)
            {
                // 冲刺期间保持一个固定速度
                rb.velocity = direction * dashSpeed;

                timer += Time.deltaTime;
                yield return null;
            }

            // 冲刺结束，恢复重力，保留一点速度也行
            rb.gravityScale = originalGravity;
            rb.velocity = rb.velocity.normalized * originVelocity.magnitude;

            isDashing = false;
        }
    }
}