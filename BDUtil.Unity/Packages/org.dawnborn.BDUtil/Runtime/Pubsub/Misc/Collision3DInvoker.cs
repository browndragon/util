using UnityEngine;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Collision3DInvoker")]
    [Tooltip("Support 3d collisions->publish to topic or unity event")]
    [RequireComponent(typeof(Collider))]
    public class Collision3DInvoker : CollisionInvoker<Collider>
    {
        protected void OnCollisionEnter(Collision collision) => OnCollider.OnEnter.Invoke(collision.collider);
        protected void OnCollisionStay(Collision collision) => OnCollider.OnStay.Invoke(collision.collider);
        protected void OnCollisionExit(Collision collision) => OnCollider.OnExit.Invoke(collision.collider);
        protected void OnTriggerEnter(Collider collider) => OnCollider.OnEnter.Invoke(collider);
        protected void OnTriggerStay(Collider collider) => OnCollider.OnStay.Invoke(collider);
        protected void OnTriggerExit(Collider collider) => OnCollider.OnExit.Invoke(collider);
    }
}