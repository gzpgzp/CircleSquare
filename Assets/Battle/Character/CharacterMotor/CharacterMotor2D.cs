using UnityEngine;

namespace Battle.Character.CharacterMotor
{
    public class CharacterMotor2D : BaseCharacterMotor
    {
        private Rigidbody2D rb;
        public Vector2 velocity => rb.velocity;
        
        private Transform groundCheckPoint;
        private float groundCheckRadius = 0.15f;
        private LayerMask groundLayer;

        public bool IsGrounded { get; private set; }

        public float gravity
        {
            get { return rb.gravityScale; }
        }

        public void InitMotor(MotorHelper helper)
        {
            rb = helper.rb;
            groundCheckPoint = helper.groundCheckPoint;
        }

        public void SetVelocityX(float x)
        {
            rb.velocity = new Vector2(x, rb.velocity.y);
        }

        public void SetVelocity(Vector2 v)
        {
            rb.velocity = v;
        }

        public void AddForce(Vector2 force, ForceMode2D mode = ForceMode2D.Impulse)
        {
            rb.AddForce(force, mode);
        }

        public void SetGravityScale(float scale)
        {
            rb.gravityScale = scale;
        }

        public void Tick(float dt)
        {
            CheckGround();
        }

        private void CheckGround()
        {
            IsGrounded = Physics2D.OverlapCircle(
                groundCheckPoint.position,
                groundCheckRadius,
                groundLayer
            );
        }
    }
}