using BDUtil.Math;
using BDUtil.Screen;
using UnityEngine;

namespace BDRPG.Screen
{
    /// Creates a camera which uses 2 finger gestures/right click to pan.
    /// The idea is that your gesture's centroid should remain under your fingers.
    [AddComponentMenu("BDUtil/PanCamera")]
    [RequireComponent(typeof(Camera))]
    public class PanCamera : MonoBehaviour
    {
        public bool IsBounded;
        public float ScrollSensitivity = -5f;
        public float PinchSensitivity = -.25f;
        public Vector2 Limits = new(1f, 100f);
        new Camera camera;
        void Awake() => camera = GetComponent<Camera>();
        float StartTime = float.NaN;
        Vector3 StartPos;

        void Update()
        {
            Vector3 position = camera.ScreenPointToRay(Input.mousePosition).AtZ();
            float delta = Input.mouseScrollDelta.y * ScrollSensitivity;
            if (Input.touchCount >= 2)
            {
                Touch touch0 = Input.GetTouch(0), touch1 = Input.GetTouch(1);
                Vector2
                    new0 = touch0.position,
                    new1 = touch1.position;
                position = camera.ScreenPointToRay((new0 + new1) / 2f).AtZ();
                if (!float.IsFinite(StartTime))
                {
                    StartTime = Time.time;
                    StartPos = position;
                    return;
                }
                Vector2
                    old0 = new0 - touch0.deltaTime * touch0.deltaPosition,
                    old1 = new1 - touch1.deltaTime * touch1.deltaPosition;
                float oldDistance = Vector2.Distance(old0, old1);
                float newDistance = Vector2.Distance(new0, new1);
                delta = PinchSensitivity * (newDistance - oldDistance);
            }
            else if (
                Input.GetMouseButton(1)  // Right click or...
                                         // v-- emulated right click or...
                || (Input.GetMouseButton(0) && (Input.GetKey(KeyCode.LeftShift) || Input.GetKey(KeyCode.RightShift)))
                // scroll wheel.
                || delta != default)
            {
                if (!float.IsFinite(StartTime))
                {
                    StartTime = Time.time;
                    StartPos = position;
                    if (delta == 0f) return;
                }
            }
            else
            {
                if (float.IsFinite(StartTime))
                {
                    StartPos = default;
                    StartTime = float.NaN;
                }
                return;
            }
            if (SceneBounds.Bounds.Contains(position) || !IsBounded) transform.position += StartPos - position;
            camera.orthographicSize += delta;
            camera.orthographicSize = Mathf.Clamp(camera.orthographicSize, Limits.x, Limits.y);
        }
    }
}