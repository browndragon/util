using UnityEngine;

namespace BDUtil
{
    public static class CameraExt
    {
        /// Because vector2 NEEDS to be xy, this basically NEEDS to be -z. But if you do something fancy...
        public static Plane DefaultPlane = new(Vector3.back, 0f);
        public static Vector3 ScreenPointToIntersection(this Camera thiz, Vector3 screenPoint, Plane plane)
        {
            if (plane.normal == default) plane = DefaultPlane;
            Ray ray = thiz.ScreenPointToRay(screenPoint);
            Vector3 pointOnPlane = plane.distance * plane.normal;
            // Okay: Now they might have handed us a backwards plane; fix that.
            if (Vector3.Dot(plane.normal, pointOnPlane - ray.origin) < 0) plane.Flip();
            // AND the raycast might fail in the case where we're parallel to the plane, doc'd to return false/0f.
            if (!plane.Raycast(ray, out float distance) && distance == default) return float.NaN * Vector3.one;
            // Returns true/+distance or false/-distance based on in front/behind ray (ugh).
            return ray.GetPoint(distance);
        }
    }
}