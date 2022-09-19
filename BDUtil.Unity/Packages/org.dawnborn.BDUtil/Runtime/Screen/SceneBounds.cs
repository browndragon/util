using BDUtil.Math;
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
                if (bounds == default) bounds = RecalculateBounds();
                return bounds;
            }
        }
        public static void ClearBounds() => bounds = default;
        public static Bounds RecalculateBounds()
        {
            Bounds bounds = new();
            foreach (Renderer body in Object.FindObjectsOfType<Renderer>())
            {
                bounds.Encapsulate(body.bounds);
            }
            return bounds;
        }
    }
}
