using UnityEngine;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Collision2DInvoker")]
    [Tooltip("Support 2d collisions->publish to topic or unity event")]
    [RequireComponent(typeof(Collider2D))]
    public class Collision2DInvoker : CollisionInvoker<Collider2D>
    {
        protected void OnCollisionEnter2D(Collision2D collision) { if (DoCollide(collision.gameObject)) OnCollider.OnEnter.Invoke(collision.collider); }
        protected void OnCollisionStay2D(Collision2D collision) { if (DoCollide(collision.gameObject)) OnCollider.OnStay.Invoke(collision.collider); }
        protected void OnCollisionExit2D(Collision2D collision) { if (DoCollide(collision.gameObject)) OnCollider.OnExit.Invoke(collision.collider); }
        protected void OnTriggerEnter2D(Collider2D collider) { if (DoCollide(collider.gameObject)) OnCollider.OnEnter.Invoke(collider); }
        protected void OnTriggerStay2D(Collider2D collider) { if (DoCollide(collider.gameObject)) OnCollider.OnStay.Invoke(collider); }
        protected void OnTriggerExit2D(Collider2D collider) { if (DoCollide(collider.gameObject)) OnCollider.OnExit.Invoke(collider); }
    }
}