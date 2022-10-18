using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [RequireComponent(typeof(Groundling), typeof(Rigidbody2D)), RequireComponent(typeof(Animator))]
    public class PhysicalAnimator2D : MonoBehaviour
    {
        public string NamedX = "dXZ";
        public string NamedY = "dY";
        public string NamedOnGround = "OnGround";

        new Rigidbody2D rigidbody;
        Groundling groundling;
        Animator animator;

        protected void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            groundling = GetComponent<Groundling>();
            animator = GetComponent<Animator>();
        }

        protected void Update()
        {
            animator.SetBool(NamedOnGround, groundling.OnGround.Value);
            animator.SetFloat(NamedX, rigidbody.velocity.x);
            animator.SetFloat(NamedY, rigidbody.velocity.y);
        }
        static readonly RaycastHit2D[] scratch = new RaycastHit2D[1];
    }
}