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
        public float DropDY = -6f;
        public float JumpGravity = 0f;  // Gravity during initial phase of jump.
        public float RiseGravity = 4f;  // Gravity scale while dy > 0 (but not jumping)
        public float FallGravity = 6f;  // Gravity scale while dy < 0
        public FixedTimer JumpTime = 1f / 8f;  // Time allowed during initial phase of jump.
        public bool IsJumping;

        [Range(0, .3f)] public float MovementSmoothing = .05f;  // How much to smooth out the movement

        public LayerMask GroundLayerMask = -1;  // A mask determining what is ground to the character
        public Vector3 GroundCheck = new(.5f, 0, .1f);  // A position marking where to check if the player is grounded. Z is radius!
        public bool IsGrounded;
        bool wasGrounded;
        public FixedTimer CoyoteTime = 1f / 16f;

        public Vector3 CeilingCheck = new(.5f, 1f, .1f);  // A position marking where to check for ceilings. Z is radius!
        public bool IsCrouching = false;
        bool wasCrouching;

        // So that we can use sprites that face left.
        public bool FacingRight = true;

        new Rigidbody2D rigidbody;
        new SpriteRenderer renderer;

        [Header("Events")]
        [Space]
        public UnityEvent<bool> OnGroundEvent = new();
        public UnityEvent<bool> OnCrouchEvent = new();

        [field: Range(-1f, 1f)] public float WantDX { get; set; }
        public bool WantCrouch { get; set; }
        public bool WantJump { get; set; }

        public void Move(Vector2 move)
        {
            WantDX = move.x;
            WantJump = move.y > 0;
            WantCrouch = move.y < 0;
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
            // Check to see if we're on the ground.
            CheckGround();
            CheckCrouch();
            ApplyJump();
            ApplyWalk();
        }

        void ApplyJump()
        {
            bool isRising = rigidbody.velocity.y > 0f;
            if (WantJump)
            {
                if (CoyoteTime.Tick.IsLive && !isRising && !JumpTime.Tick.IsLive)
                {
                    rigidbody.velocity = new(rigidbody.velocity.x, Mathf.Max(JumpDY, rigidbody.velocity.y));
                    IsJumping = true;
                    JumpTime.Reset();
                }
            }
            else
            {
                IsJumping = false;
                JumpTime.Stop();
            }

            if (!isRising) IsJumping = false;

            if (JumpTime.Tick.IsLive) rigidbody.gravityScale = JumpGravity;
            else if (isRising) rigidbody.gravityScale = IsJumping ? RiseGravity : FallGravity;
            else rigidbody.gravityScale = FallGravity;
        }
        void ApplyWalk()
        {
            Vector2 delta = new(WantDX * MaxDX, rigidbody.velocity.y);
            if (JumpTime.Tick.IsLive) delta.x *= JumpDXScale;
            else if (!CoyoteTime.Tick.IsLive)  // "we're off the ground for real".
            {
                delta.x *= AirDXScale;
                switch ((delta.x.Valence(), rigidbody.velocity.x.Valence()))
                {
                    case (true, true): delta.x = Mathf.Max(delta.x, rigidbody.velocity.x); break;
                    case (false, false): delta.x = Mathf.Min(delta.x, rigidbody.velocity.x); break;
                    default: delta.x += rigidbody.velocity.x; break;
                }
            }
            if (IsCrouching)
            {
                delta.x *= CrouchDXScale;
                delta.y = Mathf.Min(delta.y, DropDY);
            }
            rigidbody.velocity = new(
                Mathf.SmoothDamp(rigidbody.velocity.x, delta.x, ref _accX, MovementSmoothing),
                Mathf.SmoothDamp(rigidbody.velocity.y, delta.y, ref _accY, MovementSmoothing)
            );
        }
        float _accX = default, _accY = default;

        void CheckCrouch()
        {
            wasCrouching = IsCrouching;
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
            wasGrounded = IsGrounded;
            IsGrounded = false;
            foreach (Collider2D _ in OverlapRelativeCircle(GroundCheck))
            {
                IsGrounded = true;
                break;
            }
            if (IsGrounded) CoyoteTime.Reset();
            if (wasGrounded ^ IsGrounded) OnGroundEvent.Invoke(IsGrounded);
        }

        static readonly Collider2D[] colliders = new Collider2D[16];
    }
}