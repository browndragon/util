using UnityEngine;

namespace BDUtil.Screen
{
    /// A camera which forces others to face me at all times.
    /// SUPER fucking inefficient to leave enabled! But during a period of camera motion...
    [AddComponentMenu("BDUtil/ContinuousBillboardCamera")]
    [RequireComponent(typeof(Camera))]
    public class ContinuousBillboardCamera : MonoBehaviour
    {
        new Camera camera;
        protected void Awake() => camera = GetComponent<Camera>();
        protected void LateUpdate() => Billboard.BillboardAll(camera);
        protected void Start() => Debug.Log($"Info: You're using a continuous billboard camera, broadcasting each tick...");
    }
}
