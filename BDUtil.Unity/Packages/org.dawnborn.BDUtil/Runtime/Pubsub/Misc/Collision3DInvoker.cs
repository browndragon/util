using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Collision3DInvoker")]
    [Tooltip("Support 3d collisions->publish to topic or unity event")]
    [RequireComponent(typeof(Collider))]
    public class Collision3DInvoker : MonoBehaviour
    {
        Collider myCollider;
        public UnityEvent<Collider> OnEnter;
        public UnityEvent<Collider> OnStay;
        public UnityEvent<Collider> OnExit;

        void OnCollisionEnter(Collision collision) => OnEnter?.Invoke(collision.collider);
        void OnCollisionStay(Collision collision) => OnStay?.Invoke(collision.collider);
        void OnCollisionExit(Collision collision) => OnExit?.Invoke(collision.collider);
        void OnTriggerEnter(Collider collider) => OnEnter?.Invoke(collider);
        void OnTriggerStay(Collider collider) => OnStay?.Invoke(collider);
        void OnTriggerExit(Collider collider) => OnExit?.Invoke(collider);
    }
}