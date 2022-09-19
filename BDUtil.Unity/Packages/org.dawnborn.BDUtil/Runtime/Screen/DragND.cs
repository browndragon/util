using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil.Screen
{
    /// Drag a 2- or 3-d rigidbody through a scene.
    public abstract class DragND : MonoBehaviour
    {
        new Camera camera;
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
        protected virtual void Awake() => camera = Camera.main;
        protected abstract Vector3 RBPos { get; set; }
        protected abstract Vector3 RBVeloc { get; set; }

        [SuppressMessage("IDE", "IDE0051")]
        void OnMouseDown()
        {
            Offset = transform.position - ScreenPoint;
            IsDragging = true;
        }
        [SuppressMessage("IDE", "IDE0051")]
        void OnMouseDrag() => Target = ScreenPoint + Offset;
        [SuppressMessage("IDE", "IDE0051")]
        void OnMouseUp() => IsDragging = false;
        [SuppressMessage("IDE", "IDE0051")]
        void FixedUpdate()
        {
            if (!IsDragging) return;
            Vector3 offset = Target - RBPos;
            float expectStep = Time.fixedDeltaTime * Veloc;
            expectStep *= expectStep;
            if (offset.sqrMagnitude <= expectStep)
            {
                RBPos = Target;
                RBVeloc = Vector3.zero;
                return;
            }
            RBVeloc = Veloc * offset.normalized;
        }
    }
}
