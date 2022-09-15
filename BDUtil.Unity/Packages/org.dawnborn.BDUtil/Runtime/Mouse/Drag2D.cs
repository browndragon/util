using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil.Mouse
{
    [AddComponentMenu("BDUtil/Drag2D")]
    /// If you don't use a rigidbody, you do not get trigger collisions (etc).
    [RequireComponent(typeof(Rigidbody2D), typeof(Collider2D))]
    public class Drag2D : MonoBehaviour
    {
        new Camera camera;
        new Rigidbody2D rigidbody;
        public float Veloc = 24f;
        Vector3 Offset;
        Vector2 Target;
        bool IsDragging;

        [SuppressMessage("IDE", "IDE0051")]
        void Awake()
        {
            camera = Camera.main;
            rigidbody = GetComponent<Rigidbody2D>();
        }
        [SuppressMessage("IDE", "IDE0051")]
        void OnMouseDown()
        {
            Offset = transform.position - camera.ScreenToWorldPoint(Input.mousePosition);
            IsDragging = true;
        }
        [SuppressMessage("IDE", "IDE0051")]
        void OnMouseDrag() => Target = camera.ScreenToWorldPoint(Input.mousePosition) + Offset;
        [SuppressMessage("IDE", "IDE0051")]
        void OnMouseUp()
        {
            IsDragging = false;
        }
        [SuppressMessage("IDE", "IDE0051")]
        void FixedUpdate()
        {
            if (!IsDragging) return;
            Vector2 offset = Target - rigidbody.position;
            float expectStep = Time.fixedDeltaTime * Veloc;
            expectStep *= expectStep;
            if (offset.sqrMagnitude <= expectStep)
            {
                rigidbody.position = Target;
                rigidbody.velocity = Vector2.zero;
                return;
            }
            rigidbody.velocity = Veloc * offset.normalized;
        }
    }
}
