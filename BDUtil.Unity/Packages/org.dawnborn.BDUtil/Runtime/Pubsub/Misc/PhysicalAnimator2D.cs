using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [RequireComponent(typeof(Rigidbody2D)), RequireComponent(typeof(Animator))]
    public class PhysicalAnimator2D : MonoBehaviour
    {
        public string NamedX = "dXZ";
        public string NamedY = "dY";
        public string NamedOnGround = "OnGround";
        public float GroundDistance = 1 / 128f;

        public bool onGround;
        public Timer HangTime = .125f;

        new Rigidbody2D rigidbody;
        Animator animator;

        protected void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            animator = GetComponent<Animator>();
        }
        protected void FixedUpdate()
        {
            scratch[0] = default;
            onGround = rigidbody.Cast(Vector2.down, scratch, GroundDistance) > 0;
            scratch[0] = default;
        }

        protected void Update()
        {
            if (!onGround) { if (!HangTime.IsStarted) HangTime.Reset(); }
            else HangTime = HangTime.Length;
            animator.SetBool(NamedOnGround, onGround || HangTime.Tick.IsLive);
            animator.SetFloat(NamedX, rigidbody.velocity.x);
            animator.SetFloat(NamedY, rigidbody.velocity.y);
        }
        static readonly RaycastHit2D[] scratch = new RaycastHit2D[1];
    }
}