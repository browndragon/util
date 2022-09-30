using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Collision2DInvoker")]
    [Tooltip("Support 2d collisions->publish to topic or unity event")]
    [RequireComponent(typeof(Collider2D))]
    public class Collision2DInvoker : MonoBehaviour
    {
        public UnityEvent<Collider2D> OnEnter;
        public UnityEvent<Collider2D> OnStay;
        public UnityEvent<Collider2D> OnExit;

        protected void OnCollisionEnter2D(Collision2D collision) => OnEnter?.Invoke(collision.otherCollider);
        protected void OnCollisionStay2D(Collision2D collision) => OnStay?.Invoke(collision.otherCollider);
        protected void OnCollisionExit2D(Collision2D collision) => OnExit?.Invoke(collision.otherCollider);
        protected void OnTriggerEnter2D(Collider2D collider) => OnEnter?.Invoke(collider);
        protected void OnTriggerStay2D(Collider2D collider) => OnStay?.Invoke(collider);
        protected void OnTriggerExit2D(Collider2D collider) => OnExit?.Invoke(collider);
    }
}