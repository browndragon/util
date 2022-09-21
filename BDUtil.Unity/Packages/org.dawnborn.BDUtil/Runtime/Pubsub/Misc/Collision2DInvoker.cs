using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Collision2DInvoker")]
    [Tooltip("Support 2d collisions->publish to topic or unity event")]
    [RequireComponent(typeof(Collider2D))]
    public class Collision2DInvoker : MonoBehaviour
    {
        Collider2D myCollider;
        public UnityEvent<Collider2D> OnEnter;
        public UnityEvent<Collider2D> OnStay;
        public UnityEvent<Collider2D> OnExit;

        void OnCollisionEnter2D(Collision2D collision) => OnEnter?.Invoke(collision.otherCollider);
        void OnCollisionStay2D(Collision2D collision) => OnStay?.Invoke(collision.otherCollider);
        void OnCollisionExit2D(Collision2D collision) => OnExit?.Invoke(collision.otherCollider);
        void OnTriggerEnter2D(Collider2D collider) => OnEnter?.Invoke(collider);
        void OnTriggerStay2D(Collider2D collider) => OnStay?.Invoke(collider);
        void OnTriggerExit2D(Collider2D collider) => OnExit?.Invoke(collider);
    }
}