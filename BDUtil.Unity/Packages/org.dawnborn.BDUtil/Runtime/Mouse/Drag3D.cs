using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil.Mouse
{
    [AddComponentMenu("BDUtil/Drag3D")]
    /// If you don't use a rigidbody, you do not get trigger collisions (etc).
    [RequireComponent(typeof(Rigidbody), typeof(Collider))]
    public class Drag3D : MonoBehaviour
    {
        new Camera camera;
        new Rigidbody rigidbody;
        public float Veloc = 24f;
        Vector3 Offset;
        Vector3 Target;
        bool IsDragging;
        [Tooltip("The plane to drag along; by default the XY plane but easily could be XZ with normal y=+1")]
        public Vector3 PlaneNormal = CameraExt.DefaultPlane.normal;
        [Tooltip("The offset along the normal from the origin of the dragplane (default @ origin)")]
        public float PlaneOriginDistance = CameraExt.DefaultPlane.distance;
        public Vector3 ScreenPoint => camera.ScreenPointToIntersection(Input.mousePosition, new(PlaneNormal, PlaneOriginDistance));

        [SuppressMessage("IDE", "IDE0051")]
        void Awake()
        {
            camera = Camera.main;
            rigidbody = GetComponent<Rigidbody>();
        }
        [SuppressMessage("IDE", "IDE0051")]
        void OnMouseDown()
        {
            Offset = transform.position - ScreenPoint;
            IsDragging = true;
        }
        [SuppressMessage("IDE", "IDE0051")]
        void OnMouseDrag() => Target = ScreenPoint + Offset;
        [SuppressMessage("IDE", "IDE0051")]
        void OnMouseUp()
        {
            IsDragging = false;
        }
        [SuppressMessage("IDE", "IDE0051")]
        void FixedUpdate()
        {
            if (!IsDragging) return;
            Vector3 offset = Target - rigidbody.position;
            float expectStep = Time.fixedDeltaTime * Veloc;
            expectStep *= expectStep;
            if (offset.sqrMagnitude <= expectStep)
            {
                rigidbody.position = Target;
                rigidbody.velocity = Vector3.zero;
                return;
            }
            rigidbody.velocity = Veloc * offset.normalized;
        }
    }
}
