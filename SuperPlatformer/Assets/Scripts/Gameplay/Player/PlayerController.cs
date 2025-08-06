#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
using UnityEngine;
using Core.Input;

namespace Gameplay.Player
{
    /// &lt;summary&gt;
    /// Handles player movement and state machine, using movement parameters from PlayerConstants.
    /// &lt;/summary&gt;
    [RequireComponent(typeof(SpriteRenderer))]
    public class PlayerController : MonoBehaviour
    {
        public enum PlayerState { Grounded, Jumping, Falling }

        [Header("Testing/Override Fields")]
        [Tooltip("If true, the player is considered on the ground (debug only, replace with PhysicsEngine2D CheckGrounded in future).")]
        public bool isGroundedDebug = true;

        [Header("Movement Parameters (defaults from PlayerConstants)")]
        public float walkSpeed = PlayerConstants.WALK_SPEED;
        public float runSpeed = PlayerConstants.RUN_SPEED;
        public float runAccelTime = PlayerConstants.RUN_ACCEL_TIME;
        public float gravity = PlayerConstants.GRAVITY;
        public float jumpInitialVelocity = PlayerConstants.JUMP_INITIAL_VELOCITY;
        public float coyoteTime = PlayerConstants.COYOTE_TIME;
        public float jumpBufferTime = PlayerConstants.JUMP_BUFFER_TIME;

        [Header("References")]
        public Rigidbody2D rb;

        // Internal state
        private PlayerState currentState = PlayerState.Grounded;
        private Vector2 velocity = Vector2.zero;

        // Timers for coyote time and jump buffering
        private float coyoteTimer = 0f;
        private float jumpBufferTimer = 0f;

        private bool jumpQueued = false;

        void Start()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
        }

        void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            InputManager.InputState input = InputManager.Instance != null ? InputManager.Instance.Current : default;

            // Update timers
            bool grounded = isGroundedDebug;
            // TODO: Replace with: grounded = Core.Physics.PhysicsEngine2D.CheckGrounded(...);

            if (grounded)
                coyoteTimer = coyoteTime;
            else
                coyoteTimer -= Time.deltaTime;

            if (input.jumpPressed)
                jumpBufferTimer = jumpBufferTime;
            else
                jumpBufferTimer -= Time.deltaTime;

            // State transitions
            switch (currentState)
            {
                case PlayerState.Grounded:
                    if (jumpBufferTimer > 0 && coyoteTimer > 0)
                    {
                        Jump();
                        currentState = PlayerState.Jumping;
                        jumpBufferTimer = 0;
                    }
                    else if (!grounded)
                    {
                        currentState = PlayerState.Falling;
                    }
                    break;
                case PlayerState.Jumping:
                    if (velocity.y &lt;= 0)
                    {
                        currentState = PlayerState.Falling;
                    }
                    break;
                case PlayerState.Falling:
                    if (grounded)
                    {
                        currentState = PlayerState.Grounded;
                        velocity.y = 0;
                    }
                    else if (jumpBufferTimer > 0 && coyoteTimer > 0)
                    {
                        Jump();
                        currentState = PlayerState.Jumping;
                        jumpBufferTimer = 0;
                    }
                    break;
            }

            // Horizontal movement
            float targetSpeed = (input.runHeld ? runSpeed : walkSpeed) * input.moveAxis;
            float accel = runAccelTime > 0 ? (targetSpeed - velocity.x) / runAccelTime : 0;
            velocity.x = Mathf.MoveTowards(velocity.x, targetSpeed, Mathf.Abs(accel) * Time.deltaTime);

            // Gravity
            if (!grounded)
                velocity.y -= gravity * Time.deltaTime;

            // Jump held for variable jump height (optional; can expand if spec asks)
            if (!input.jumpHeld && velocity.y > 0)
                velocity.y -= gravity * 2f * Time.deltaTime; // fast fall when jump released early

            // Apply velocity
            Vector2 delta = velocity * Time.deltaTime;
            Vector2 newPosition = (Vector2)transform.position + delta;

            // TODO: Replace with Core.Physics.PhysicsEngine2D.ResolveCollisions(...) once ready
            transform.position = newPosition;
        }

        void Jump()
        {
            velocity.y = jumpInitialVelocity;
            coyoteTimer = 0;
        }
    }
}
#endif