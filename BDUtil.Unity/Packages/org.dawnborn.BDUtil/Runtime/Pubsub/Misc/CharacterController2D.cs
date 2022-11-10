using System;
using System.Collections.Generic;
using BDUtil.Clone;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController2D : MonoBehaviour
    {
        public float MaxDX = 12f;
        public float WalkDXScale = 1f;
        public float CrouchDXScale = .5f;
        public float JumpDXScale = .75f;
        public float AirDXScale = .5f;

        public float MaxDY = 6f;
        public float JumpDYScale = 3f;
        public float RiseDYScale = 0f;
        public float FallDYScale = 1f;

        public float JumpGravity = 0f;  // Gravity during initial launch phase of jump.
        public float RiseGravity = 4f;  // Gravity scale while dy > 0 (after launch phase)
        public float FallGravity = 6f;  // Gravity scale while dy < 0

        [Flags]
        public enum Touchings
        {
            None = default,
            Floor = 1 << 0,
            Left = 1 << 1,
            Top = 1 << 2,
            Right = 1 << 3,
            Ladder = 1 << 4,
        };
        public Touchings Touching;
        public Delay Grounded = Clock.FixedNow.StoppedDelayOf(.125f);
        public bool IsMidair => !Touching.HasFlag(Touchings.Floor) && !Grounded;

        // Ability cooldown cycle: begun, executed, and then departed.
        public Cooldown Jumping = new(0f, .125f, .0625f, Clock.FixedNow);
        public bool JumpRising;
        public Cooldown Firing = new(0f, 0f, .5f, Clock.FixedNow);
        public Rigidbody2D Ammo;
        public float FireVelocity = 12f;
        public Cooldown Interacting = new(0f, 0f, .5f, Clock.FixedNow);

        public Cooldown Crouching = new(0f, float.PositiveInfinity, 0f, Clock.FixedNow);

        [Range(0, .3f)] public float MovementSmoothing = .05f;  // How much to smooth out the deltaV.

        // So that we can use sprites that face left.
        public bool FacingRight = true;

        new Rigidbody2D rigidbody;

        [SerializeField] Vector2 wantMove;
        public Vector2 WantMove { get => wantMove; set => SetWantMove(value); }
        public void SetWantMove(Vector2 value)
        {
            wantMove = value;
            if (value != default) LastWantMove = value;
            LastComponentMove = new(
                wantMove.x != 0f ? wantMove.x : LastComponentMove.x,
                wantMove.y != 0f ? wantMove.y : LastComponentMove.y
            );
        }
        // Last (nondefault) move.
        public Vector2 LastWantMove { get; private set; } = Vector2.right;
        // Per-component last (nondefault) move.
        public Vector2 LastComponentMove { get; private set; } = new(1, 1);
        public Vector3 FireDir => (LastComponentMove.x * Vector3.right + LastWantMove.y * Vector3.up).normalized;

        [field: SerializeField] public bool WantRun { get; set; }
        [field: SerializeField] public bool WantFire { get; set; }
        [field: SerializeField] public bool WantInteract { get; set; }

        public bool WantCrouch => WantMove.y < 0f;
        public bool WantJump => WantMove.y > 0f;


        protected void Awake() => rigidbody = GetComponentInParent<Rigidbody2D>();
        protected void Start() => rigidbody.gravityScale = FallGravity;

        protected void Update()
        {
            bool? ShouldFaceRight = WantMove.x.Valence();
            // basically localScale's valence, but flipped if !FacingRight.
            bool? IsFacingRight = transform.localScale.x.Valence() ^ !FacingRight;
            switch (ShouldFaceRight ^ IsFacingRight)
            {
                case null: break;
                case false: break;
                case true: transform.localScale = transform.localScale.WithX(-transform.localScale.x); break;
            }
            if (Ammo != null)
            {
                if (WantFire) Firing.Warm();
                if (Firing.ResetCount() > 0)
                {
                    Rigidbody2D fired = Pool.main.Acquire(Ammo, false);
                    fired.tag = tag;
                    fired.transform.position = transform.position + FireDir;
                    fired.gameObject.SetActive(true);
                    fired.velocity = FireVelocity * FireDir;
                }
            }
            if (WantInteract) Interacting.Warm();
            if (Interacting.ResetCount() > 0)
            {
                Debug.Log($"Interacting!");
            }
        }

        protected void FixedUpdate()
        {
            CheckColliders();
            CheckCrouch();
            ApplyMovement();
            CheckJumping();
        }

        void CheckColliders()
        {
            bool wasGrounded = Grounded;  // , wasCeilinged = Ceilinged, wasWalled = Walled;
            bool wasLaddered = Touching.HasFlag(Touchings.Ladder);
            Touching = Touchings.None;
            foreach (ContactPoint2D contact in GetContacts())
            {
                if (contact.normal.y > contact.normal.x && contact.normal.y > -contact.normal.x)
                {
                    Touching |= Touchings.Floor;
                    continue;
                }
                if (contact.normal.y < contact.normal.x && contact.normal.y < -contact.normal.x)
                {
                    Touching |= Touchings.Top;
                    continue;
                }
                if (contact.normal.x > contact.normal.y && contact.normal.x > -contact.normal.y)
                {
                    Touching |= Touchings.Left;
                    continue;
                }
                if (contact.normal.x < contact.normal.y && contact.normal.x < -contact.normal.y)
                {
                    Touching |= Touchings.Right;
                    continue;
                }
            }
            if (wasLaddered) Touching |= Touchings.Ladder;
            if (Touching.HasFlag(Touchings.Floor)) Grounded.Reset();
            if (!wasGrounded && Grounded)
            {
                JumpRising = false;
                Jumping.Reset();
            }
        }
        void CheckCrouch()
        {
            if (WantCrouch) Crouching.Warm(restartHot: true);
            else if (Touching.HasFlag(Touchings.Top)) Crouching.Warm(restartHot: true);
            else Crouching.Cool();
        }
        void ApplyMovement()
        {
            Vector2 delta = new(WantMove.x * MaxDX, WantMove.y * MaxDY);
            Vector2 veloc = rigidbody.velocity;  //, origVeloc = veloc;

            if (delta.y > 0f && Jumping && JumpDYScale > 0f) delta.y = Mathf.Max(veloc.y, delta.y * JumpDYScale);
            else if (delta.y > 0f && RiseDYScale > 0f) delta.y = Mathf.Max(veloc.y, delta.y * RiseDYScale);
            else delta.y = Mathf.Min(veloc.y, delta.y * FallDYScale);

            if (Crouching) delta.x *= CrouchDXScale;
            if (IsMidair && !Grounded)  // After a little bit of coyote time
            {  // Lets you "swim" in air, a little. Could be ice-friction too?
                delta.x *= AirDXScale;
                delta.x += veloc.x;
                delta.x = Mathf.Clamp(delta.x, -MaxDX * JumpDXScale, +MaxDX * JumpDXScale);
            }

            rigidbody.velocity = new(
                Mathf.SmoothDamp(rigidbody.velocity.x, delta.x, ref _accX, MovementSmoothing),
                Mathf.SmoothDamp(rigidbody.velocity.y, delta.y, ref _accY, MovementSmoothing)
            );
        }
        void CheckJumping()
        {
            if (!WantJump) { Jumping.Cool(); JumpRising = false; }
            else if (Grounded) Jumping.Warm();
            else if (rigidbody.velocity.y < 0) { Jumping.Cool(); JumpRising = false; }

            if (Jumping) rigidbody.gravityScale = JumpGravity;
            else if (JumpRising) rigidbody.gravityScale = RiseGravity;
            else rigidbody.gravityScale = FallGravity;
        }
        float _accX = default, _accY = default;

        static int ContactCount = -1;
        static readonly ContactPoint2D[] Contacts = new ContactPoint2D[32];
        IEnumerable<ContactPoint2D> GetContacts()
        {
            ContactCount = rigidbody.GetContacts(Contacts);
            for (int i = 0; i < ContactCount; ++i) yield return Contacts[i];
        }
    }
}