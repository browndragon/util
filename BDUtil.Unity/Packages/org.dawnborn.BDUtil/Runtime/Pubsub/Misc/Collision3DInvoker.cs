using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Collision3DInvoker")]
    [Tooltip("Support 3d collisions->publish to topic or unity event")]
    [RequireComponent(typeof(Collider))]
    public class Collision3DInvoker : MonoBehaviour
    {
        public UnityEvent<Collider> OnEnter;
        public UnityEvent<Collider> OnStay;
        public UnityEvent<Collider> OnExit;

        protected void OnCollisionEnter(Collision collision) => OnEnter?.Invoke(collision.collider);
        protected void OnCollisionStay(Collision collision) => OnStay?.Invoke(collision.collider);
        protected void OnCollisionExit(Collision collision) => OnExit?.Invoke(collision.collider);
        protected void OnTriggerEnter(Collider collider) => OnEnter?.Invoke(collider);
        protected void OnTriggerStay(Collider collider) => OnStay?.Invoke(collider);
        protected void OnTriggerExit(Collider collider) => OnExit?.Invoke(collider);
    }
}