using UnityEngine;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Collision2DInvoker")]
    [Tooltip("Support 2d collisions->publish to topic or unity event")]
    [RequireComponent(typeof(Collider2D))]
    public class Collision2DInvoker : CollisionInvoker<Collider2D>
    {
        protected void OnCollisionEnter2D(Collision2D collision) => OnCollider.OnEnter.Invoke(collision.otherCollider);
        protected void OnCollisionStay2D(Collision2D collision) => OnCollider.OnStay.Invoke(collision.otherCollider);
        protected void OnCollisionExit2D(Collision2D collision) => OnCollider.OnExit.Invoke(collision.otherCollider);
        protected void OnTriggerEnter2D(Collider2D collider) => OnCollider.OnEnter.Invoke(collider);
        protected void OnTriggerStay2D(Collider2D collider) => OnCollider.OnStay.Invoke(collider);
        protected void OnTriggerExit2D(Collider2D collider) => OnCollider.OnExit.Invoke(collider);
    }
}