using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [RequireComponent(typeof(CharacterController2D), typeof(Rigidbody2D)), RequireComponent(typeof(Animator))]
    public class PhysicalAnimator2D : MonoBehaviour
    {
        public string NamedX = "dXZ";
        public string NamedY = "dY";
        public string NamedOnGround = "OnGround";

        new Rigidbody2D rigidbody;
        CharacterController2D characterController;
        Animator animator;

        protected void Awake()
        {
            rigidbody = GetComponent<Rigidbody2D>();
            characterController = GetComponent<CharacterController2D>();
            animator = GetComponent<Animator>();
        }

        protected void Update()
        {
            animator.SetBool(NamedOnGround, characterController.IsGrounded);
            animator.SetFloat(NamedX, rigidbody.velocity.x);
            animator.SetFloat(NamedY, rigidbody.velocity.y);
        }
    }
}