using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BDUtil.Math
{
    public static class Rays
    {
        /// Projects forward or back, plane oriented or not, to find where the line would hit.
        public static Vector3 Linecast(this Ray ray, Plane plane)
        {
            Vector3 pointOnPlane = plane.distance * plane.normal;
            // Okay: Now they might have handed us a backwards plane; fix that.
            if (Vector3.Dot(plane.normal, pointOnPlane - ray.origin) < 0) plane.Flip();
            // AND the raycast might fail in the case where we're parallel to the plane, doc'd to return false/0f.
            if (!plane.Raycast(ray, out float distance) && distance == default) return Vectors.NaN3;
            // Returns true/+distance or false/-distance based on in front/behind ray (ugh).
            return ray.GetPoint(distance);
        }

        /// Projects a ray as though it were a line and discovers where it intersects x/y/z (default =0).
        /// This is great for projecting camera rays to hit world 0, where theoretically the plane of the camera might break the plane of the scene...
        /// Simpler math than the Linecast case, since there's no plane to flip.
        public static Vector3 AtI(this Ray thiz, int dimension, float value = 0f)
        {
            /// It will never ever ever intersect/will always intersect at every point.
            if (thiz.direction[dimension] == 0f) return Vectors.NaN3;
            float scale = (thiz.origin[dimension] - value) / thiz.direction[dimension];  // If negative, naturally converges.
            return thiz.origin - scale * thiz.direction;
        }
        public static Vector3 AtX(this Ray thiz, float x = 0f) => thiz.AtI(0, x);
        public static Vector3 AtY(this Ray thiz, float y = 0f) => thiz.AtI(1, y);
        public static Vector3 AtZ(this Ray thiz, float z = 0f) => thiz.AtI(2, z);
    }
}