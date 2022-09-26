using BDUtil.Math;
using UnityEngine;

namespace BDRPG.Screen
{
    /// Creates a camera which uses 2 finger gestures/right click to pan.
    /// The idea is that your gesture's centroid should remain under your fingers.
    [AddComponentMenu("BDUtil/PanCamera")]
    [RequireComponent(typeof(Camera))]
    public class PanCamera : MonoBehaviour
    {
        public float ScaleBase = 1.5f;
        new Camera camera;
        void Awake() => camera = GetComponent<Camera>();
        float StartTime = float.NaN;
        Vector3 StartPos;
        float StartDelta;

        void Update()
        {
            bool updating = false;
            Vector3 position = default;
            float delta = default;
            if (Input.GetMouseButton(1) || Input.mouseScrollDelta.y != default)
            {
                position = camera.ScreenPointToRay(Input.mousePosition).AtZ();
                delta = Input.mouseScrollDelta.y;
                if (!float.IsFinite(StartTime))
                {
                    StartTime = Time.time;
                    StartPos = position;
                    StartDelta = 0f;
                    if (delta == 0f) return;
                }
                updating = true;
            }
            else if (Input.touchCount >= 2)
            {
                Vector2 touch0, touch1;
                touch0 = Input.GetTouch(0).position;
                touch1 = Input.GetTouch(1).position;
                delta = Vector2.Distance(touch0, touch1);
                position = camera.ScreenPointToRay((touch0 + touch1) / 2f).AtZ();
                if (!float.IsFinite(StartTime))
                {
                    StartTime = Time.time;
                    StartPos = position;
                    StartDelta = delta;
                    return;
                }
                updating = true;
            }
            if (updating)
            {
                transform.position += StartPos - position;
                camera.orthographicSize /= Mathf.Pow(ScaleBase, delta - StartDelta);
                return;
            }
            // ending, and not updating anymore...
            if (float.IsFinite(StartTime))
            {
                StartDelta = 0f;
                StartTime = float.NaN;
            }
        }
    }
}