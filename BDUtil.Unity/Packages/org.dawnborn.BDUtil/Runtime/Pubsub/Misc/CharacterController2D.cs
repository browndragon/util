using System;
using System.Collections.Generic;
using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [RequireComponent(typeof(Rigidbody2D))]
    public class CharacterController2D : MonoBehaviour
    {
        public float MaxDX = 12f;
        public float WalkDXScale = .5f;
        public float CrouchDXScale = .5f;
        public float JumpDXScale = .75f;
        public float AirDXScale = .5f;

        public float MaxDY = 6f;
        public float JumpDYScale = 3f;
        public float RiseDYScale = 0f;
        public float FallDYScale = -1f;

        public float JumpGravity = 0f;  // Gravity during initial launch phase of jump.
        public float RiseGravity = 4f;  // Gravity scale while dy > 0 (after launch phase)
        public float FallGravity = 6f;  // Gravity scale while dy < 0

        public Delay Grounded = Clock.FixedNow.StoppedDelayOf(.125f);
        public Delay Walled = Clock.FixedNow.StoppedDelayOf(.125f);
        public Delay Ceilinged = Clock.FixedNow.StoppedDelayOf(.125f);
        public Delay Laddered = Clock.FixedNow.StoppedDelayOf(.125f);
        public bool IsMidair => !Grounded && !Walled && !Laddered && !Ceilinged;

        // Ability cooldown cycle: begun, executed, and then departed.
        public Cooldown Jumping = new(0f, .125f, .0625f, Clock.FixedNow);
        public bool JumpRising;
        public Cooldown Running = new(0f, float.PositiveInfinity, .125f, Clock.FixedNow);
        public Cooldown Firing = new(0f, 0f, .5f, Clock.FixedNow);
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
            if (WantFire) Firing.Warm();
        }

        protected void FixedUpdate()
        {
            CheckColliders();
            CheckCrouch();
            ApplyMovement();
            CheckJumping();
        }

        protected void OnDrawGizmosSelected()
        {
            if (rigidbody == null) rigidbody = GetComponent<Rigidbody2D>();
            if (rigidbody == null) return;
            int i = -1;
            Color old = Gizmos.color;
            foreach (ContactPoint2D contact in GetContacts())
            {
                i++;
                Gizmos.color = new((float)i / ContactCount, 1f, 0f, .7f);
                Gizmos.DrawLine(contact.point, contact.point + contact.normal);
            }
            Gizmos.color = old;
        }

        void CheckColliders()
        {
            bool wasGrounded = Grounded;  // , wasCeilinged = Ceilinged, wasWalled = Walled;
            foreach (ContactPoint2D contact in GetContacts())
            {
                if (contact.normal.y > contact.normal.x && contact.normal.y > -contact.normal.x)
                {
                    Grounded.Reset();
                    continue;
                }
                if (contact.normal.y < contact.normal.x && contact.normal.y < -contact.normal.x)
                {
                    Ceilinged.Reset();
                    continue;
                }
                if (contact.normal.x > contact.normal.y && contact.normal.x > -contact.normal.y)
                {
                    /*Left*/
                    Walled.Reset();
                    continue;
                }
                if (contact.normal.x < contact.normal.y && contact.normal.x < -contact.normal.y)
                {
                    /*Right*/
                    Walled.Reset();
                    continue;
                }
            }
            if (!wasGrounded && Grounded)
            {
                JumpRising = false;
                Jumping.Reset();
            }
        }
        void CheckCrouch()
        {
            if (WantCrouch) Crouching.Warm(restartHot: true);
            else if (Ceilinged) Crouching.Warm(restartHot: true);
        }
        [SerializeField] Delay airtime = Clock.FixedNow.StoppedDelayOf(float.PositiveInfinity);
        void ApplyMovement()
        {
            Vector2 delta = new(WantMove.x * MaxDX, WantMove.y * MaxDY);
            Vector2 veloc = rigidbody.velocity;

            if (delta.y > 0f && Jumping && JumpDYScale > 0f) delta.y = Mathf.Max(veloc.y, delta.y * JumpDYScale);
            else if (delta.y > 0f && RiseDYScale > 0f) delta.y = Mathf.Max(veloc.y, delta.y * RiseDYScale);
            else delta.y = Mathf.Min(veloc.y, delta.y * FallDYScale);

            if (IsMidair)
            {  // Lets you "swim" in air, a little. Could be ice-friction too?
                delta.x *= AirDXScale;
                switch ((delta.x.Valence(), veloc.x.Valence()))
                {
                    case (true, true): delta.x = Mathf.Max(delta.x, veloc.x); break;
                    case (false, false): delta.x = Mathf.Min(delta.x, veloc.x); break;
                    default: delta.x += veloc.x; break;
                }
            }
            if (Crouching) delta.x *= CrouchDXScale;

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

            if (Jumping.ResetCount() > 0)
            {
                JumpRising = true;
                rigidbody.velocity = new(rigidbody.velocity.x * JumpDXScale, rigidbody.velocity.y);
            }

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