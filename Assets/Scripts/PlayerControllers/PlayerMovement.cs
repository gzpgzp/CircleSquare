using System;
using UnityEngine;

namespace PlayerControllers
{
    public class PlayerMovement : MonoBehaviour
    {
        [Header("References")] public PlayerMovementStat moveStats;
        [SerializeField] private Collider2D feetColl;
        [SerializeField] private Collider2D bodyColl;

        private Rigidbody2D rb;

        private Vector2 moveVelocity;
        private bool isFacingRight;

        private RaycastHit2D groundHit;
        private RaycastHit2D headHit;

        private bool isGrounded;
        private bool bumpedHead;

        // jump vars
        public float VerticalVelocity { get; private set; }
        private bool isJumping;
        private bool isFastFalling;
        private bool isFalling;
        private float fastFallTime;
        private float fastFallReleaseSpeed;
        private int numberOfJumpsUsed;

        // apex vars
        private float apexPoint;
        private float timePastApexThreshold;
        private bool isPastApexThreshold;

        // jump buffer vars
        private float jumpBufferTimer;
        private bool jumpReleaseDuringBuffer;

        // coyoto time vars
        private float coyoteTimer;

        private void Awake()
        {
            isFacingRight = true;
            rb = GetComponent<Rigidbody2D>();
        }

        private void Update()
        {
            CountTimers();
            JumpChecks();
        }

        private void FixedUpdate()
        {
            CollisionChecks();
            Jump();
            
            if (isGrounded)
            {
                Move(moveStats.groundAcceleration, moveStats.groundDeceleration, InputManager.Movement);
            }
            else
            {
                Move(moveStats.airAcceleration, moveStats.airAcceleration, InputManager.Movement);
            }
        }

        #region Movement

        private void Move(float acceleration, float deceleration, Vector2 moveInput)
        {
            if (moveInput != Vector2.zero)
            {
                TurnCheck(moveInput);

                var targetV = Vector2.zero;
                if (InputManager.RunIsHeld)
                {
                    targetV = new Vector2(moveInput.x, 0f) * moveStats.maxRunSpeed;
                }
                else
                {
                    targetV = new Vector2(moveInput.x, 0f) * moveStats.maxWalkSpeed;
                }

                moveVelocity = Vector2.Lerp(moveVelocity, targetV, acceleration * Time.fixedDeltaTime);
                rb.velocity = new Vector2(moveVelocity.x, rb.velocity.y);
            }

            else if (moveInput == Vector2.zero)
            {
                moveVelocity = Vector2.Lerp(moveVelocity, Vector2.zero, deceleration * Time.fixedDeltaTime);
                rb.velocity = new Vector2(moveVelocity.x, rb.velocity.y);
            }
        }

        private void TurnCheck(Vector2 moveInput)
        {
            if (isFacingRight && moveInput.x < 0)
            {
                Turn(false);
            }
            else if (!isFacingRight && moveInput.x > 0)
            {
                Turn(true);
            }
        }

        private void Turn(bool turnRight)
        {
            if (turnRight)
            {
                isFacingRight = true;
                transform.Rotate(0f, 180f, 0f);
            }
            else
            {
                isFacingRight = false;
                transform.Rotate(0f, -180f, 0f);
            }
        }

        #endregion

        #region Jump

        private void JumpChecks()
        {
            if (InputManager.JumpWasPressed)
            {
                jumpBufferTimer = moveStats.jumpBufferTime;
                jumpReleaseDuringBuffer = false;
            }

            if (InputManager.JumpWasReleased)
            {
                if (jumpBufferTimer > 0f)
                {
                    jumpReleaseDuringBuffer = true;
                }

                if (isJumping && VerticalVelocity > 0f)
                {
                    if (isPastApexThreshold)
                    {
                        isPastApexThreshold = false;
                        isFastFalling = true;
                        fastFallTime = moveStats.timeForUpwardCancel;
                        VerticalVelocity = 0f;
                    }
                    else
                    {
                        isFastFalling = true;
                        fastFallReleaseSpeed = VerticalVelocity;
                    }
                }
            }

            
            // initiate jump with jump buffering and coyote time
            if (jumpBufferTimer > 0f && !isJumping && (isGrounded || coyoteTimer > 0f))
            {
                InitiateJump(1);

                if (jumpReleaseDuringBuffer)
                {
                    isFastFalling = true;
                    fastFallReleaseSpeed = VerticalVelocity;
                }
            }
            
            // double jump
            else if(jumpBufferTimer < 0f && isJumping && numberOfJumpsUsed < moveStats.numberOfJumpAllowed)
            {
                isFastFalling = false;
                InitiateJump(1);
            }
            
            // air jump after coyote time lapsed
            else if (jumpBufferTimer > 0f && isFastFalling && numberOfJumpsUsed < moveStats.numberOfJumpAllowed - 1)
            {
                InitiateJump(2);
                isFastFalling = false;
            }
            
            // landed
            if ((isJumping || isFalling) && isGrounded && VerticalVelocity <= 0f)
            {
                isJumping = false;
                isFalling = false;
                isFastFalling = false;
                fastFallTime = 0f;
                isPastApexThreshold = false;
                numberOfJumpsUsed = 0;
                
                VerticalVelocity = Physics2D.gravity.y;
            }
        }

        private void InitiateJump(int numberOfJumpsUsed)
        {
            if (isJumping)
            {
                isJumping = true;
            }

            jumpBufferTimer = 0f;
            this.numberOfJumpsUsed += numberOfJumpsUsed;
            VerticalVelocity = moveStats.initialJumpVelocity;
        }

        private void Jump()
        {
            // apply gravity while jumping
            if (isJumping)
            {
                // check for head bump
                if (bumpedHead)
                {
                    isFastFalling = true;
                }
            
                // gravity on ascending
                if (VerticalVelocity >= 0f)
                {
                    // apex controls
                    apexPoint = Mathf.InverseLerp(moveStats.initialJumpVelocity, 0f, VerticalVelocity);

                    if (apexPoint > moveStats.apexThreshold)
                    {
                        if (!isPastApexThreshold)
                        {
                            isPastApexThreshold = true;
                            timePastApexThreshold = 0f;
                        }

                        if (isPastApexThreshold)
                        {
                            timePastApexThreshold += Time.fixedDeltaTime;
                            if (timePastApexThreshold < moveStats.apexHangTime)
                            {
                                VerticalVelocity = 0f;
                            }
                            else
                            {
                                VerticalVelocity = -0.01f;
                            }
                        }
                    }
                
                    // gravity on ascending but past apex threshold
                    else
                    {
                        VerticalVelocity += moveStats.gravity * Time.fixedDeltaTime;
                        if (isPastApexThreshold)
                        {
                            isPastApexThreshold = false;
                        }
                    }
                }

                // gravity on descending
                else if (!isFastFalling)
                {
                    VerticalVelocity += moveStats.gravity * moveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }
            
                else if (VerticalVelocity < 0f)
                {
                    if (!isFalling)
                    {
                        isFalling = true;
                    }
                }
            }

            // jump cut
            if (isFastFalling)
            {
                if (fastFallTime >= moveStats.timeForUpwardCancel)
                {
                    VerticalVelocity += moveStats.gravity * moveStats.gravityOnReleaseMultiplier * Time.fixedDeltaTime;
                }
                else if (fastFallTime < moveStats.timeForUpwardCancel)
                {
                    VerticalVelocity = Mathf.Lerp(fastFallReleaseSpeed, 0f,
                        (fastFallTime / moveStats.timeForUpwardCancel));
                }

                fastFallTime += Time.fixedDeltaTime;
            }
            
            // normal gravity while falling
            if (!isGrounded && !isJumping)
            {
                if (!isFalling)
                {
                    isFalling = true;
                }

                VerticalVelocity += moveStats.gravity * Time.fixedDeltaTime;
            }

            // clamp fall speed
            VerticalVelocity = Mathf.Clamp(VerticalVelocity, -moveStats.maxFallSpeed, 50f);
            
            Debug.Log($"verticalvelocity is {VerticalVelocity}, isGround is {isGrounded}");
            rb.velocity = new Vector2(rb.velocity.x, VerticalVelocity);
        }

        private void DrawJumpArc(float moveSpeed, Color gizmoColor)
        {
            Vector2 startPos = new Vector2(feetColl.bounds.center.x, feetColl.bounds.center.y);
            Vector2 previousPos = startPos;
            float speed = 0f;
            if (moveStats.drawRight)
            {
                speed = moveSpeed;
            }
            else
            {
                speed = -moveSpeed;
            }
            Vector2 velocity = new Vector2(speed, moveStats.initialJumpVelocity);
            Gizmos.color = gizmoColor;

            float timeStep = 2 * moveStats.timeTillJumpApex / moveStats.arcResolution;

            for (int i = 0; i < moveStats.timeTillJumpApex; i++)
            {
                float simulationTime = i * timeStep;
                Vector2 displacement;
                Vector2 drawPoint;

                if (simulationTime < moveStats.timeTillJumpApex)
                {
                    displacement = velocity * simulationTime + 0.5f * new Vector2(0f, moveStats.gravity) * simulationTime * simulationTime;
                }
                else if (simulationTime < moveStats.timeTillJumpApex + moveStats.apexHangTime)
                {
                    float apexTime = simulationTime - moveStats.timeTillJumpApex;
                    displacement = velocity * moveStats.timeTillJumpApex + 0.5f * new Vector2(0f, moveStats.gravity) * moveStats.timeTillJumpApex * moveStats.timeTillJumpApex;
                    displacement += new Vector2(speed, 0) * apexTime;
                }
                else
                {
                    float descendTime = simulationTime - (moveStats.timeTillJumpApex + moveStats.apexHangTime);
                    displacement = velocity * moveStats.timeTillJumpApex + 0.5f * new Vector2(0, moveStats.gravity) * moveStats.timeTillJumpApex * moveStats.timeTillJumpApex;
                    displacement += new Vector2(speed, 0) * moveStats.apexHangTime;
                    displacement += new Vector2(speed, 0) * descendTime +
                                    0.5f * new Vector2(0, moveStats.gravity) * descendTime * descendTime;
                }

                drawPoint = startPos + displacement;

                if (moveStats.stopOnCollision)
                {
                    var hit = Physics2D.Raycast(previousPos, drawPoint - previousPos, Vector2.Distance(previousPos, drawPoint), moveStats.groundLayer);
                    if (hit.collider != null)
                    {
                        Gizmos.DrawLine(previousPos, hit.point);
                        break;
                    }
                }
                
                Gizmos.DrawLine(previousPos, drawPoint);
                previousPos = drawPoint;
            }
        }

        #endregion

        #region Collision Checks

        private void IsGrounded()
        {
            var boxCastOrigin = new Vector2(feetColl.bounds.center.x, feetColl.bounds.min.y);
            var boxCastSize = new Vector2(feetColl.bounds.size.x, moveStats.groundDetectionRayLength);

            groundHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.down,
                moveStats.groundDetectionRayLength, moveStats.groundLayer);
            if (groundHit.collider != null)
            {
                isGrounded = true;
            }
            else
            {
                isGrounded = false;
            }

            // debug
            if (moveStats.debugShowIsGrounded)
            {
                Color rayColor;
                if (isGrounded)
                {
                    rayColor = Color.green;
                }
                else
                {
                    rayColor = Color.red;
                }

                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2, boxCastOrigin.y),
                    Vector2.down * moveStats.groundDetectionRayLength, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x + boxCastSize.x / 2, boxCastOrigin.y),
                    Vector2.down * moveStats.groundDetectionRayLength, rayColor);
                Debug.DrawRay(
                    new Vector2(boxCastOrigin.x - boxCastSize.x / 2,
                        boxCastOrigin.y - moveStats.groundDetectionRayLength), Vector2.right * boxCastSize.x, rayColor);
            }
        }

        private void BumpedHead()
        {
            Vector2 boxCastOrigin = new Vector2(feetColl.bounds.center.x, bodyColl.bounds.max.y);
            Vector2 boxCastSize = new Vector2(feetColl.bounds.size.x * moveStats.headWidth,
                moveStats.headDetectionRayLength);
            headHit = Physics2D.BoxCast(boxCastOrigin, boxCastSize, 0f, Vector2.up, moveStats.headDetectionRayLength,
                moveStats.groundLayer);
            if (headHit.collider != null)
            {
                bumpedHead = true;
            }
            else
            {
                bumpedHead = false;
            }

            if (moveStats.debugShowHeadBumpBox)
            {
                float headWidth = moveStats.headWidth;

                Color rayColor;
                if (bumpedHead)
                {
                    rayColor = Color.green;
                }
                else
                {
                    rayColor = Color.red;
                }
                
                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x / 2 * headWidth, boxCastOrigin.y), Vector3.up * moveStats.headDetectionRayLength, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x + (boxCastSize.x / 2) * headWidth, boxCastOrigin.y), Vector2.up * moveStats.headDetectionRayLength, rayColor);
                Debug.DrawRay(new Vector2(boxCastOrigin.x - boxCastSize.x /2 * headWidth, boxCastOrigin.y + moveStats.headDetectionRayLength), Vector3.right * boxCastSize.x * headWidth, rayColor);
            }
        }

        private void CollisionChecks()
        {
            IsGrounded();
            BumpedHead();
        }

        #endregion
        
        #region Timers

        private void CountTimers()
        {
            jumpBufferTimer -= Time.deltaTime;
            if (!isGrounded)
            {
                coyoteTimer -= Time.deltaTime;
            }
            else
            {
                coyoteTimer = moveStats.jumpCoyoteTime;
            }
        }

        #endregion

        #region debug
        
        private void OnDrawGizmos()
        {
            if (moveStats.showWalkJumpArc)
            {
                DrawJumpArc(moveStats.maxWalkSpeed, Color.white);
            }

            if (moveStats.showRunJumpArc)
            {
                DrawJumpArc(moveStats.maxRunSpeed, Color.red);
            }
        }

        #endregion
    }
}