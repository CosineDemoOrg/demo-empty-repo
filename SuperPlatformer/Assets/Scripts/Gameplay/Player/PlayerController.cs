#if UNITY_EDITOR || UNITY_STANDALONE || UNITY_WEBGL
using UnityEngine;
using Core.Input;
using SuperPlatformer.Core.Physics;

namespace Gameplay.Player
{
    /// <summary>
    /// Handles player movement and state machine, using movement parameters from PlayerConstants.
    /// </summary>
    [RequireComponent(typeof(SpriteRenderer))]
    [RequireComponent(typeof(BoxCollider2D))]
    public class PlayerController : MonoBehaviour
    {
        public enum PlayerState { Grounded, Jumping, Falling }

        [Header("Movement Parameters (defaults from PlayerConstants)")]
        [Tooltip("WALK_SPEED: 2.5 tiles/s (PRD §6.2)")]
        public float walkSpeed = PlayerConstants.WALK_SPEED;
        [Tooltip("RUN_SPEED: 4.0 tiles/s (PRD §6.2)")]
        public float runSpeed = PlayerConstants.RUN_SPEED;
        [Tooltip("RUN_ACCEL_TIME: 0.5 s (PRD §6.2, derived)")]
        public float runAccelTime = PlayerConstants.RUN_ACCEL_TIME;
        [Tooltip("GRAVITY: 30 tiles/s² (PRD §6.2)")]
        public float gravity = PlayerConstants.GRAVITY;
        [Tooltip("JUMP_INITIAL_VELOCITY: 8.5 tiles/s (PRD §6.2)")]
        public float jumpInitialVelocity = PlayerConstants.JUMP_INITIAL_VELOCITY;
        [Tooltip("COYOTE_TIME: 0.10 s (PRD §6.2)")]
        public float coyoteTime = PlayerConstants.COYOTE_TIME;
        [Tooltip("JUMP_BUFFER_TIME: 0.08 s (PRD §6.2)")]
        public float jumpBufferTime = PlayerConstants.JUMP_BUFFER_TIME;

        /// <summary>
        /// Returns true if player is holding the run input (left mouse button or shift).
        /// </summary>
        public bool IsHoldingRun()
        {
            var input = InputManager.Instance != null ? InputManager.Instance.Current : default;
            return input.runHeld;
        }

        [Header("References")]
        public Rigidbody2D rb;
        [SerializeField] private BoxCollider2D boxCollider;

        // Internal state
        private PlayerState currentState = PlayerState.Grounded;
        private Vector2 velocity = Vector2.zero;

        // Expose velocity for CameraFollow2D
        public Vector2 Velocity => velocity;

        // Timers for coyote time and jump buffering
        private float coyoteTimer = 0f;
        private float jumpBufferTimer = 0f;

        private bool jumpQueued = false;

        void Start()
        {
            if (rb == null)
                rb = GetComponent<Rigidbody2D>();
            if (boxCollider == null)
                boxCollider = GetComponent<BoxCollider2D>();
        }

        void Update()
        {
#if UNITY_EDITOR
            if (!Application.isPlaying) return;
#endif
            InputManager.InputState input = InputManager.Instance != null ? InputManager.Instance.Current : default;

            // Update timers

            // Use new deterministic physics grounded check
            bool grounded = PhysicsEngine2D.CheckGrounded(GetBounds());

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
                    if (velocity.y <= 0)
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

            // --- Deterministic physics integration ---
            var moveResult = PhysicsEngine2D.Move(GetBounds(), velocity * Time.deltaTime);
            transform.position += (Vector3)moveResult.allowedDelta;
            if (moveResult.hitGround) velocity.y = 0;
            if (moveResult.hitLeft || moveResult.hitRight) velocity.x = 0;
            grounded = PhysicsEngine2D.CheckGrounded(GetBounds());

        }

        void Jump()
        {
            velocity.y = jumpInitialVelocity;
            coyoteTimer = 0;
        }

        /// <summary>
        /// Returns the world-space bounds of the player collider.
        /// </summary>
        public Bounds GetBounds()
        {
            if (boxCollider != null)
                return boxCollider.bounds;
            // fallback: compute from transform/sprite (not recommended)
            var sr = GetComponent<SpriteRenderer>();
            if (sr != null)
                return sr.bounds;
            return new Bounds(transform.position, Vector3.one * 0.9f);
        }
    }
}
#endif