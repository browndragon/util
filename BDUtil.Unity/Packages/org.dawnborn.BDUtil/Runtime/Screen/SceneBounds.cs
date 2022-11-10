using BDUtil.Clone;
using UnityEngine;

namespace BDUtil.Screen
{
    public static class SceneBounds
    {
        static Bounds bounds;
        public static Bounds Bounds
        {
            get
            {
                if (bounds == default) RecalculateBounds();
                return bounds;
            }
            set => bounds = value;
        }
        public static void ClearBounds() => bounds = default;
        public static void RecalculateBounds(float bottom = 0f, float left = 0f, float top = 0f, float right = 0f)
        {
            Bounds bounds = new();
            foreach (Renderer body in Object.FindObjectsOfType<Renderer>())
            {
                bounds.Encapsulate(body.bounds);
                bounds.Encapsulate(body.transform.position);
            }
            Bounds prev = bounds;
            bounds.SetMinMax(
                bounds.min - new Vector3(left, bottom, 0f),
                bounds.max + new Vector3(right, top, 0f)
            );
            Debug.Log($"{prev}+{bottom}/{left}/{top}/{right}={bounds}");
            SceneBounds.bounds = bounds;
        }

        public static void BounceIn(GameObject @object)
        {
            if (Bounds.Contains(@object.transform.position)) return;
            Rigidbody2D rigidbody2D = @object.GetComponent<Rigidbody2D>();
            if (rigidbody2D) { rigidbody2D.Bounce(Bounds); return; }
            Rigidbody rigidbody = @object.GetComponent<Rigidbody>();
            if (rigidbody) { rigidbody.Bounce(Bounds); return; }
            else @object.transform.position = bounds.ClosestPoint(@object.transform.position);
        }
    }
}
