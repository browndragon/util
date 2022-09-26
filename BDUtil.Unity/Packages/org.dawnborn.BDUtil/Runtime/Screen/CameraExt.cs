using BDUtil.Math;
using UnityEngine;

namespace BDUtil.Screen
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
        /// Move the camera along xy so that the point indicated in screencoords is now at viewport(.5,.5).
        public static void MoveAlongXY(this Camera thiz, Vector2 screenPoint, float maxDisplacement, bool isBounded)
        {
            Vector3 lookAt = thiz.ViewportPointToIntersection(new(.5f, .5f, 0f));
            Vector3 pointingAt = thiz.ScreenPointToIntersection(screenPoint);
            Vector3 delta = Vector3.ClampMagnitude(pointingAt - lookAt, maxDisplacement);
            pointingAt = lookAt + delta;
            if (isBounded && !SceneBounds.Bounds.Contains(pointingAt)) return;
            thiz.transform.position = thiz.transform.position + delta;
        }
        public static void MoveAlongXYDelta(this Camera thiz, Vector2 delta, bool isBounded)
        => thiz.MoveAlongXY((Vector2)thiz.ViewportToScreenPoint(.5f * Vector2.one) + delta, float.PositiveInfinity, isBounded);
    }
}