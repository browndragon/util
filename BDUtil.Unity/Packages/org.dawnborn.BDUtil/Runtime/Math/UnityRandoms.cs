using UnityEngine;

namespace BDUtil.Math
{
    /// Returns values near 0 which can be used to fuzz other values.
    public static class UnityRandoms
    {
        public static float Index(this Randoms.IRandom thiz, AnimationCurve curve)
        => curve?.length switch
        {
            null => float.NaN,
            0 => 0,
            int x => thiz.Range(curve[0].time, curve[x - 1].time)
        };
        public static float Range(this Randoms.IRandom thiz, AnimationCurve curve)
        => curve.Evaluate(thiz.Index(curve));
        public static float Index(this Randoms.IRandom thiz, AnimationCurves.Scaled curve)
        {
            Rect rect = curve.Bounds;
            return thiz.Range(rect.xMin, rect.xMax);
        }
        public static float Range(this Randoms.IRandom thiz, AnimationCurves.Scaled curve)
        => curve.Evaluate(thiz.Index(curve));

        public static Vector2 Range(this Randoms.IRandom thiz, Vector2 min, Vector2 max)
        => new(thiz.Range(min.x, max.x), thiz.Range(min.y, max.y));
        public static Vector2Int Range(this Randoms.IRandom thiz, Vector2Int min, Vector2Int max)
        => new(thiz.Range(min.x, max.x), thiz.Range(min.y, max.y));
        public static Vector3 Range(this Randoms.IRandom thiz, Vector3 min, Vector3 max)
        => new(thiz.Range(min.x, max.x), thiz.Range(min.y, max.y), thiz.Range(min.z, max.z));
        public static Vector3Int Range(this Randoms.IRandom thiz, Vector3Int min, Vector3Int max)
        => new(thiz.Range(min.x, max.x), thiz.Range(min.y, max.y), thiz.Range(min.z, max.z));
        public static Quaternion Range(this Randoms.IRandom thiz, Quaternion min, Quaternion max)
        => new(thiz.Range(min.x, max.x), thiz.Range(min.y, max.y), thiz.Range(min.z, max.z), thiz.Range(min.w, max.w));
        public static Color Range(this Randoms.IRandom thiz, Color min, Color max)
        => new(thiz.Range(min.r, max.r), thiz.Range(min.g, max.g), thiz.Range(min.b, max.b), thiz.Range(min.a, max.a));
        public static HSVA Range(this Randoms.IRandom thiz, HSVA min, HSVA max)
        => new(thiz.Range(min.h, max.h), thiz.Range(min.s, max.s), thiz.Range(min.v, max.v), thiz.Range(min.a, max.a));
        public static HSVA Range(this Randoms.IRandom thiz, HSVA.Fuzzed hsva)
        => thiz.Fuzz(hsva.Color, hsva.Fuzz);

        public static Vector2 Range(this Randoms.IRandom thiz, Rect rect)
        => thiz.Range(rect.min, rect.max);
        public static Vector2Int Range(this Randoms.IRandom thiz, RectInt rect)
        => thiz.Range(rect.min, rect.max);
        public static Vector3 Range(this Randoms.IRandom thiz, Bounds bounds)
        => thiz.Range(bounds.min, bounds.max);
        public static Vector3Int Range(this Randoms.IRandom thiz, BoundsInt bounds)
        => thiz.Range(bounds.min, bounds.max);

        public static Color Fuzz(this Randoms.IRandom thiz, HSVA target, HSVA fuzz, HSVA @base = default)
        {
            if (@base == default) @base = Vectors.NaNH;
            return @base.Overridden(target) + thiz.Range(-fuzz, fuzz);
        }
        public static Color Fuzz(this Randoms.IRandom thiz, Color target, HSVA fuzz, Color @base = default)
        {
            if (@base == default) @base = Vectors.NaNC;
            return (HSVA)@base.Overridden(target) + thiz.Range(-fuzz, fuzz);
        }

        /// Uniform distribution of points on the perimeter of the unit circle.
        public static Vector2 OnUnitCircle(this Randoms.IRandom thiz) => Vectors.OfAngle(thiz.Range(-180f, 180f));
        public static Vector2 InUnitCircle(this Randoms.IRandom thiz) => Mathf.Sqrt(thiz.Unit) * thiz.OnUnitCircle();
        /// Uniform distribution of points on the surface of the unit sphere.
        // https://math.stackexchange.com/questions/1585975/how-to-generate-random-points-on-a-sphere/1586185#1586185
        public static Vector3 OnUnitSphere(this Randoms.IRandom thiz)
        {
            // Generate a point in [0,1)^2
            Vector2 uv = thiz.Range(Vector2.zero, Vector2.one);
            // Turn them into points on the unit cylinder with correct probability density
            // In particular, the latitude distribution needs to be thinner at the ends (since: sphere), thus the acos.
            Vector2 latlong = new(
                Mathf.Acos(2 * uv.x - 1) - Mathf.PI / 2f,
                2f * Mathf.PI * uv.y
            );
            // Then trivially map those points onto the unit sphere.
            Vector2 sin = new(Mathf.Sin(latlong.x), Mathf.Sin(latlong.y));
            Vector2 cos = new(Mathf.Cos(latlong.x), Mathf.Cos(latlong.y));
            return new(cos.x * cos.y, cos.x * sin.y, sin.x);
        }
        public static Vector2 InUnitSphere(this Randoms.IRandom thiz) => Mathf.Pow(thiz.Unit, 1f / 3f) * thiz.OnUnitSphere();
        /// Uniform distribution of unit rotations.
        // https://stackoverflow.com/questions/31600717/how-to-generate-a-random-quaternion-quickly
        public static Quaternion GetUnitRotation(this Randoms.IRandom thiz)
        {
            Vector3 uvw = thiz.Range(Vector3.zero, Vector3.one);
            float sqrt_u = Mathf.Sqrt(uvw.x);
            float sqrt1_u = Mathf.Sqrt(1 - uvw.x);
            float v2pi = uvw.y * Mathf.PI * 2f;
            float w2pi = uvw.z * Mathf.PI * 2f;
            return new(sqrt1_u * Mathf.Sin(v2pi), sqrt1_u * Mathf.Cos(v2pi), sqrt_u * Mathf.Sin(w2pi), sqrt_u * Mathf.Cos(w2pi));
        }
    }
}