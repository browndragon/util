using BDUtil;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Screen
{
    /// A camera with precise angling support. Can adjust angle or position to match.
    [AddComponentMenu("BDUtil/IsometricCamera")]
    [RequireComponent(typeof(Camera))]
    [Tooltip("Remember to set custom sort order (1,1,10000)")]
    public class IsometricCamera : MonoBehaviour
    {
        public const float DimetricAngle = 30f;
        public const float IsometricAngle = 35.26438968275467f;
        public enum Isometries
        {
            Custom = default,
            ByPosition,
            Dimetric2To1,
            TrueIsometric,
        };
        public Isometries Isometry = Isometries.TrueIsometric;
        [Tooltip("For UpdateWithCurrentCamera; where should the camera look?")]
        public Vector3 Target = default;
        [Tooltip("For UpdateWithCurrentCamera; which way is up? It's recommended this be `back` or `up` (though either works)")]
        public Vector3 WorldUp = Vector3.back;
        public float Distance = 10f;
        void OnValidate()
        {
            UpdateCameraGeometry();
            // "SendMessage can't be called from OnValidate" grumble grumble.
            EditorUtils.Delay(this, UpdateWithCurrentCamera);
        }

        public void UpdateCameraGeometry()
        {
            switch (Isometry)
            {
                case Isometries.ByPosition: transform.LookAt(Target, WorldUp); break;
                case Isometries.Dimetric2To1: transform.eulerAngles = new(-DimetricAngle, 45, -60); break;
                case Isometries.TrueIsometric: transform.eulerAngles = new(-IsometricAngle, 45, -60); break;
                default: return;
            };
            if (Distance >= 0f) transform.position = Target - Distance * transform.TransformVector(Vector3.forward);
        }

        [ContextMenu("BDUtil/UpdateWorldFromCamera")]
        public void UpdateWithCurrentCamera() => Billboard.BillboardAll(GetComponent<Camera>());
    }
}
