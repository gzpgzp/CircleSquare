using UnityEngine;

namespace Battle.Character.CharacterMotor
{
    public class CharacterMotor2D
    {
        private Rigidbody2D rb;
        public Vector2 velocity => rb.velocity;

        public void Init(Rigidbody2D rb)
        {
            this.rb = rb;
        }

        public void SetVelocityX(float x)
        {
            rb.velocity = new Vector2(x, rb.velocity.y);
        }

        public void AddForce(Vector2 force)
        {
            rb.AddForce(force, ForceMode2D.Impulse);
        }

        public void SetGravityScale(float scale)
        {
            rb.gravityScale = scale;
        }
    }
}