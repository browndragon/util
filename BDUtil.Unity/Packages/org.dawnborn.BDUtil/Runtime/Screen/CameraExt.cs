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
        public static void MoveAlongXY(this Camera thiz, Vector2 screenPoint, ref Vector2 velocity, float smoothTime = .025f, float maxSpeed = 24f)
        {
            Vector2 lookAt = thiz.ViewportPointToIntersection(new(.5f, .5f, 0f));
            Vector2 pointingAt = thiz.ScreenPointToIntersection(screenPoint);
            thiz.MoveAlongXYDelta(pointingAt - lookAt, ref velocity, smoothTime, maxSpeed);
        }
        public static void MoveAlongXYDelta(this Camera thiz, Vector2 delta, ref Vector2 velocity, float smoothTime = .025f, float maxSpeed = 24f)
        => thiz.transform.position += (Vector3)Vector2.SmoothDamp(Vector2.zero, delta, ref velocity, smoothTime, maxSpeed);
    }
}