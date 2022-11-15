using UnityEngine;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Collision3DInvoker")]
    [Tooltip("Support 3d collisions->publish to topic or unity event")]
    [RequireComponent(typeof(Collider))]
    public class Collision3DInvoker : CollisionInvoker<Collider>
    {
        protected void OnCollisionEnter(Collision collision) { if (DoCollide(collision.gameObject)) OnCollider.OnEnter.Invoke(collision.collider); }
        protected void OnCollisionStay(Collision collision) { if (DoCollide(collision.gameObject)) OnCollider.OnStay.Invoke(collision.collider); }
        protected void OnCollisionExit(Collision collision) { if (DoCollide(collision.gameObject)) OnCollider.OnExit.Invoke(collision.collider); }
        protected void OnTriggerEnter(Collider collider) { if (DoCollide(collider.gameObject)) OnCollider.OnEnter.Invoke(collider); }
        protected void OnTriggerStay(Collider collider) { if (DoCollide(collider.gameObject)) OnCollider.OnStay.Invoke(collider); }
        protected void OnTriggerExit(Collider collider) { if (DoCollide(collider.gameObject)) OnCollider.OnExit.Invoke(collider); }
    }
}