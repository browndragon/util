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
        public float MaxAcceleration = float.PositiveInfinity;
        public float JumpForce = 60f;  // Amount of force added when the player jumps.
        public Easings.Enum JumpFunc = Easings.Enum.Expo | Easings.Enum.Out | Easings.Enum.FlipX;
        [Range(0, 1)] public float CrouchSpeed = .333f;  // Amount of maxSpeed applied to crouching movement. 1 = 100%
        [Range(0, .3f)] public float MovementSmoothing = .05f;  // How much to smooth out the movement
        [Range(0, 1f)] public float AirControl = .333f;
        public LayerMask GroundLayerMask = -1;  // A mask determining what is ground to the character
        public Vector3 GroundCheck = new(.5f, 0, .2f);  // A position marking where to check if the player is grounded. Z is radius!
        public Vector3 CeilingCheck = new(.5f, 1f, .2f);  // A position marking where to check for ceilings. Z is radius!

        public bool IsGrounded;
        public bool IsCrouching = false;
        public FixedTimer JumpingSince = .125f;
        public Timer JumpingCooldown = .125f;

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
        Vector2 delta;

        public void Move(Vector2 move)
        {
            WantDX = move.x;
            WantJump = !JumpingCooldown.Tick.IsLive && move.y > 0;
            WantCrouch = move.y < 0;
        }


        protected void Awake()
        {
            rigidbody = GetComponentInParent<Rigidbody2D>();
            renderer = GetComponentInChildren<SpriteRenderer>();
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
            ApplyWalkAndJump();
        }

        void ApplyWalkAndJump()
        {
            delta = new(WantDX, rigidbody.velocity.y);
            delta.x *= MaxDX;
            if (!IsGrounded)
            {
                delta.x *= AirControl;
                float MaxSpeed = Mathf.Max(Mathf.Abs(rigidbody.velocity.x), Mathf.Abs(delta.x));
                delta.x += rigidbody.velocity.x;
                delta.x = Mathf.Clamp(delta.x, -MaxSpeed, +MaxSpeed);
                if (IsCrouching) delta.y = Mathf.Min(rigidbody.velocity.y, -MaxDX);
                // Might also need platform support when yes-grounded?
            }
            else if (IsCrouching)
            {
                delta.x *= CrouchSpeed;
            }
            if (!WantJump)
            {
                JumpingCooldown.Stop();
                JumpingSince.Stop();
            }
            else if (IsGrounded && !JumpingCooldown.Tick.IsLive && !JumpingSince.Tick.IsLive) JumpingSince.Reset();
            // Move the character by finding the target velocity
            Vector2 _acceleration = default;
            // And then smoothing it out and applying it to the character
            rigidbody.velocity = Vector2.SmoothDamp(
                rigidbody.velocity, delta, ref _acceleration, MovementSmoothing, MaxAcceleration
            );
            Tick jumping = JumpingSince.Tick;
            if (jumping.IsLive) rigidbody.AddForce(JumpForce * JumpFunc.Invoke(jumping) * Vector2.up);
        }

        void CheckCrouch()
        {
            // If crouching, check to see if the character can stand up
            if (IsCrouching && !WantCrouch)
            {
                foreach (Collider2D _ in OverlapRelativeCircle(CeilingCheck))
                {
                    WantCrouch = true;
                    break;
                }
            }
            if (WantCrouch ^ IsCrouching)
            {
                IsCrouching = WantCrouch;
                OnCrouchEvent.Invoke(IsCrouching);
            }
        }

        void CheckGround()
        {
            bool wasGrounded = IsGrounded;
            IsGrounded = false;
            foreach (Collider2D _ in OverlapRelativeCircle(GroundCheck))
            {
                IsGrounded = true;
                break;
            }
            if (wasGrounded ^ IsGrounded)
            {
                OnGroundEvent.Invoke(IsGrounded);
                if (IsGrounded)
                {
                    JumpingCooldown.Reset();
                    JumpingSince.Stop();
                }
            }
        }

        static readonly Collider2D[] colliders = new Collider2D[16];
    }
}