using System.Collections;
using System.Collections.Generic;
using BDUtil.Math;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    [RequireComponent(typeof(Rigidbody2D), typeof(SpriteRenderer))]
    public class CharacterController2D : MonoBehaviour
    {
        public float MaxDX = 12f;
        [Range(0, 1)] public float CrouchDXScale = .333f;
        [Range(0, 1)] public float JumpDXScale = .666f;
        [Range(0, 1)] public float AirDXScale = .333f;

        public float JumpDY = 18f;
        public float RiseDY = 0f;
        public float FallDY = -6f;
        public float JumpGravity = 0f;  // Gravity during initial launch phase of jump.
        public float RiseGravity = 4f;  // Gravity scale while dy > 0 (after launch phase)
        public float FallGravity = 6f;  // Gravity scale while dy < 0
        public FixedTimer JumpTime = 1f / 8f;  // Time allowed during initial phase of jump.
        // Determines after-JumpTime but while dy>0 whether to use riseGravity ("longer jump") or fallGravity ("shorter jump").
        public bool IsJumping;

        [Range(0, .3f)] public float MovementSmoothing = .05f;  // How much to smooth out the movement

        public LayerMask GroundLayerMask = -1;  // A mask determining what is ground to the character
        public Vector3 GroundCheck = new(.5f, 0, .1f);  // A position marking where to check if the player is grounded. Z is radius!
        public FixedTimer CoyoteTime = 1f / 16f;  // Amount of time after leaving ground you still count.
        public bool IsMidair => !CoyoteTime.Tick.IsLive;

        public Vector3 CeilingCheck = new(.5f, 1f, .1f);  // A position marking where to check for ceilings. Z is radius!
        public bool IsCrouching = false;

        // So that we can use sprites that face left.
        public bool FacingRight = true;

        new Rigidbody2D rigidbody;
        new SpriteRenderer renderer;

        [Header("Events")]
        [Space]
        public UnityEvent<bool> OnGroundEvent = new();
        public UnityEvent<bool> OnCrouchEvent = new();

        [field: Range(-1f, 1f)] public float WantDX { get; set; }
        [field: Range(-1f, 1f)] public float WantDY { get; set; }
        public bool WantCrouch => WantDY < 0f;
        public bool WantJump => WantDY > 0f;

        public void Move(Vector2 move)
        {
            WantDX = move.x;
            WantDY = move.y;
        }

        protected void Awake()
        {
            rigidbody = GetComponentInParent<Rigidbody2D>();
            renderer = GetComponentInChildren<SpriteRenderer>();
        }
        protected void Start()
        {
            rigidbody.gravityScale = FallGravity;
        }

        IEnumerable<Collider2D> OverlapRelativeCircle(Vector3 relative)
        {
            Bounds localBounds = renderer.localBounds;
            Vector3 global = localBounds.min + new Vector3(
                localBounds.size.x * relative.x,
                localBounds.size.y * relative.y,
                0f
            );
            global = renderer.transform.TransformPoint(global);
            int collisions = Physics2D.OverlapCircleNonAlloc(global, relative.z, colliders, GroundLayerMask);
            for (int i = 0; i < collisions; i++)
            {
                if (colliders[i].isTrigger) continue;
                if (colliders[i].attachedRigidbody == rigidbody) continue;
                yield return colliders[i];
            }
        }

        protected void Update()
        {
            bool? ShouldFaceRight = WantDX.Valence();
            // basically localScale's valence, but flipped if !FacingRight.
            bool? IsFacingRight = transform.localScale.x.Valence() ^ !FacingRight;
            switch (ShouldFaceRight ^ IsFacingRight)
            {
                case null: break;
                case false: break;
                case true: transform.localScale = transform.localScale.WithX(-transform.localScale.x); break;
            }
        }

        protected void FixedUpdate()
        {
            CheckGround();
            CheckCrouch();
            CheckJumping();
            ApplyMovement();
        }

        /// You can jump if you're in contact with the ground & not currently jumping.
        bool CanJump => CoyoteTime.Tick.IsLive && !IsJumping;

        void CheckJumping()
        {
            if (!WantJump) IsJumping = false;
            else if (CanJump)
            {
                IsJumping = true;
                JumpTime.Reset();
            }
            // If you're actively sinking, you're not jumping [anymore].
            if (rigidbody.velocity.y < 0) IsJumping = false;
            if (!IsJumping) JumpTime.Stop();

            if (JumpTime.Tick.IsLive) rigidbody.gravityScale = JumpGravity;
            else if (IsJumping) rigidbody.gravityScale = RiseGravity;
            else rigidbody.gravityScale = FallGravity;
        }
        void ApplyMovement()
        {
            Vector2 delta = new(WantDX * MaxDX, rigidbody.velocity.y);

            bool firstJumpTick = JumpTime.Tick.Passed <= 0f;
            if (firstJumpTick && JumpDY > 0f)
            {
                delta.x *= JumpDXScale;
                delta.y = WantDY * JumpDY;
            }
            else if (WantDY > 0f && RiseDY > 0f) delta.y = Mathf.Max(delta.y, WantDY * RiseDY);
            else delta.y = Mathf.Min(delta.y, WantDY * FallDY);

            if (IsMidair)
            {  // Lets you "swim" in air, a little. Could be ice-friction too?
                delta.x *= AirDXScale;
                switch ((delta.x.Valence(), rigidbody.velocity.x.Valence()))
                {
                    case (true, true): delta.x = Mathf.Max(delta.x, rigidbody.velocity.x); break;
                    case (false, false): delta.x = Mathf.Min(delta.x, rigidbody.velocity.x); break;
                    default: delta.x += rigidbody.velocity.x; break;
                }
            }
            if (IsCrouching) delta.x *= CrouchDXScale;

            rigidbody.velocity = new(
                Mathf.SmoothDamp(rigidbody.velocity.x, delta.x, ref _accX, MovementSmoothing),
                Mathf.SmoothDamp(rigidbody.velocity.y, delta.y, ref _accY, MovementSmoothing)
            );
        }
        float _accX = default, _accY = default;

        void CheckCrouch()
        {
            bool wasCrouching = IsCrouching;
            if (WantCrouch) IsCrouching = true;
            else
            {
                bool canStand = true;
                foreach (Collider2D _ in OverlapRelativeCircle(CeilingCheck))
                {
                    canStand = false;
                    break;
                }
                IsCrouching = !canStand;
            }
            if (IsCrouching ^ wasCrouching) OnCrouchEvent.Invoke(IsCrouching);
        }

        void CheckGround()
        {
            bool wasMidair = IsMidair;
            foreach (Collider2D _ in OverlapRelativeCircle(GroundCheck))
            {
                CoyoteTime.Reset();
                break;
            }
            if (wasMidair ^ IsMidair) OnGroundEvent.Invoke(wasMidair);
        }

        static readonly Collider2D[] colliders = new Collider2D[16];
    }
}