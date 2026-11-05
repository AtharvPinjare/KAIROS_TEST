using UnityEngine;

namespace Kairos
{
    /// <summary>
    /// Moves the player in the 2.5D plane. Velocity-driven (we set linearVelocity directly)
    /// so movement is exact and tunable. Jump has coyote time and a jump buffer.
    /// All timers run in FixedUpdate so the result does not depend on frame rate.
    /// </summary>
    [RequireComponent(typeof(Rigidbody), typeof(CapsuleCollider))]
    public class PlayerMotor : MonoBehaviour
    {
        [Header("Input")]
        [SerializeField, Tooltip("The only object allowed to read devices.")]
        private PlayerInputReader input;

        [Header("Run")]
        [SerializeField, Tooltip("Top horizontal speed in m/s.")]
        private float moveSpeed = 6f;
        [SerializeField, Tooltip("How fast speed changes on the ground (m/s per second). Small = smoothing.")]
        private float groundAcceleration = 60f;
        [SerializeField, Tooltip("Same, but in the air.")]
        private float airAcceleration = 40f;

        [Header("Jump")]
        [SerializeField, Tooltip("Height of a full jump in metres. Takeoff speed is derived from it.")]
        private float jumpApexHeight = 2.2f;
        [SerializeField, Tooltip("Seconds after leaving a ledge in which a jump is still allowed.")]
        private float coyoteTime = 0.1f;
        [SerializeField, Tooltip("Seconds a jump press is remembered before landing.")]
        private float jumpBufferTime = 0.1f;
        [SerializeField, Range(0f, 1f), Tooltip("Upward speed kept when Jump is released early (0.5 = cut by half).")]
        private float jumpCutFactor = 0.5f;

        [Header("Ground check")]
        [SerializeField, Tooltip("Layers that count as floor.")]
        private LayerMask groundMask;
        [SerializeField, Tooltip("Radius of the sphere at the feet (must be a bit smaller than the capsule).")]
        private float groundCheckRadius = 0.3f;
        [SerializeField, Tooltip("How far below the feet still counts as grounded.")]
        private float groundCheckSkin = 0.05f;

        [Header("Safety")]
        [SerializeField, Tooltip("Hard speed cap in m/s (conventions: 25).")]
        private float maxSpeed = 25f;

        Rigidbody body;
        float coyoteTimer;
        float jumpBufferTimer;
        bool isJumping;   // true from takeoff until we touch ground again

        /// <summary>True while standing on Ground or Targetable.</summary>
        public bool Grounded { get; private set; }
        /// <summary>Horizontal speed, for the Animator.</summary>
        public float VelocityX => body.linearVelocity.x;
        /// <summary>Vertical speed, for the Animator.</summary>
        public float VelocityY => body.linearVelocity.y;
        /// <summary>Takeoff speed that reaches jumpApexHeight: v = sqrt(2 g h).</summary>
        public float JumpSpeed => Mathf.Sqrt(2f * Mathf.Abs(Physics.gravity.y) * jumpApexHeight);

        void Awake()
        {
            body = GetComponent<Rigidbody>();
            body.mass = 1f;
            body.interpolation = RigidbodyInterpolation.Interpolate;
            body.collisionDetectionMode = CollisionDetectionMode.Continuous;
            body.maxLinearVelocity = maxSpeed;
            // 2.5D: Z is depth and frozen; the capsule never tips over.
            body.constraints = RigidbodyConstraints.FreezePositionZ | RigidbodyConstraints.FreezeRotation;
        }

        void Update()
        {
            // Remember the press; FixedUpdate may not run this frame.
            if (input.JumpPressed) jumpBufferTimer = jumpBufferTime;
        }

        void FixedUpdate()
        {
            float dt = Time.fixedDeltaTime;
            Vector3 v = body.linearVelocity;

            UpdateGrounded(v.y, dt);
            jumpBufferTimer -= dt;

            // Horizontal: ease toward the wanted speed.
            float accel = Grounded ? groundAcceleration : airAcceleration;
            v.x = Mathf.MoveTowards(v.x, input.Move * moveSpeed, accel * dt);

            // Jump only when a press is buffered AND we are (or just were) grounded.
            if (jumpBufferTimer > 0f && coyoteTimer > 0f)
            {
                v.y = JumpSpeed;
                jumpBufferTimer = 0f;
                coyoteTimer = 0f;   // no double jump from one coyote window
                isJumping = true;
            }
            else if (isJumping && v.y > 0f && !input.JumpHeld)
            {
                v.y *= jumpCutFactor;   // released early: short hop
                isJumping = false;
            }

            body.linearVelocity = v;
        }

        void UpdateGrounded(float velocityY, float dt)
        {
            // Sphere centre is placed so its bottom sits groundCheckSkin below the feet (pivot = feet).
            Vector3 centre = transform.position + Vector3.up * (groundCheckRadius - groundCheckSkin);
            bool touching = Physics.CheckSphere(centre, groundCheckRadius, groundMask, QueryTriggerInteraction.Ignore);

            // Ignore the ground for the first frames of a jump, while we are still rising out of the skin.
            Grounded = touching && !(isJumping && velocityY > 0f);
            if (Grounded) { isJumping = false; coyoteTimer = coyoteTime; }
            else coyoteTimer -= dt;
        }

        void OnDrawGizmosSelected()
        {
            Gizmos.color = Grounded ? Color.green : Color.red;
            Gizmos.DrawWireSphere(transform.position + Vector3.up * (groundCheckRadius - groundCheckSkin), groundCheckRadius);
        }
    }
}
