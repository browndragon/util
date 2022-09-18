using BDUtil.Math;
using UnityEngine;

namespace BDUtil
{
    public static class CameraExt
    {
        /// Because vector2 NEEDS to be xy, this basically NEEDS to be -z. But if you do something fancy...
        public static Plane DefaultPlane = new(Vector3.back, 0f);
        public static Vector3 ScreenPointToIntersection(this Camera thiz, Vector3 screenPoint, Plane plane = default)
        {
            if (plane.normal == default) plane = DefaultPlane;
            Ray ray = thiz.ScreenPointToRay(screenPoint);
            return ray.Linecast(plane);
        }
        public static Vector3 ViewportPointToIntersection(this Camera thiz, Vector3 viewPoint, Plane plane = default)
        {
            if (plane.normal == default) plane = DefaultPlane;
            Ray ray = thiz.ViewportPointToRay(viewPoint);
            return ray.Linecast(plane);
        }
    }
}