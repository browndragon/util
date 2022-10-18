using UnityEngine;

namespace BDUtil.Math
{
    /// Utilities for vectorn & bounds thereof.
    public static class Vectors
    {
        public static readonly Vector2 NaN2 = new(float.NaN, float.NaN);
        public static bool HasNaN(this Vector2 thiz) => float.IsNaN(thiz.x) || float.IsNaN(thiz.y);
        public static readonly Vector3 NaN3 = new(float.NaN, float.NaN, float.NaN);
        public static bool HasNaN(this Vector3 thiz) => float.IsNaN(thiz.x) || float.IsNaN(thiz.y) || float.IsNaN(thiz.z);
        public static readonly Vector4 NaN4 = new(float.NaN, float.NaN, float.NaN, float.NaN);
        public static bool HasNaN(this Vector4 thiz) => float.IsNaN(thiz.x) || float.IsNaN(thiz.y) || float.IsNaN(thiz.z) || float.IsNaN(thiz.w);
        public static readonly Color NaNC = new(float.NaN, float.NaN, float.NaN, float.NaN);
        public static bool HasNaN(this Color thiz) => float.IsNaN(thiz.r) || float.IsNaN(thiz.g) || float.IsNaN(thiz.b) || float.IsNaN(thiz.a);
        public static readonly HSVA NaNH = new(float.NaN, float.NaN, float.NaN, float.NaN);
        public static bool HasNaN(this HSVA thiz) => float.IsNaN(thiz.h) || float.IsNaN(thiz.s) || float.IsNaN(thiz.v) || float.IsNaN(thiz.a);

        public static Vector3 AsXYZ(this Vector2 thiz, float z = float.NaN) => new(thiz.x, thiz.y, z);
        public static Vector4 AsXYZW(this Vector3 thiz, float w = float.NaN) => new(thiz.x, thiz.y, thiz.z, w);

        public static RectInt AsBounds(this Vector2Int thiz) => new(thiz, Vector2Int.zero);
        public static Rect AsBounds(this Vector2 thiz) => new(thiz, Vector2.zero);
        public static BoundsInt AsBounds(this Vector3Int thiz) => new(thiz, Vector3Int.zero);
        public static Bounds AsBounds(this Vector3 thiz) => new(thiz, Vector3.zero);

        public static Vector2 WithI(this Vector2 thiz, int dim, float value)
        {
            thiz[dim] = value;
            return thiz;
        }
        public static Vector2 WithX(this Vector2 thiz, float value) => thiz.WithI(0, value);
        public static Vector2 WithY(this Vector2 thiz, float value) => thiz.WithI(1, value);
        public static Vector3 WithI(this Vector3 thiz, int dim, float value)
        {
            thiz[dim] = value;
            return thiz;
        }
        public static Vector3 WithX(this Vector3 thiz, float value) => thiz.WithI(0, value);
        public static Vector3 WithY(this Vector3 thiz, float value) => thiz.WithI(1, value);
        public static Vector3 WithZ(this Vector3 thiz, float value) => thiz.WithI(2, value);
        public static Vector4 WithI(this Vector4 thiz, int dim, float value)
        {
            thiz[dim] = value;
            return thiz;
        }
        public static Vector4 WithX(this Vector4 thiz, float value) => thiz.WithI(0, value);
        public static Vector4 WithY(this Vector4 thiz, float value) => thiz.WithI(1, value);
        public static Vector4 WithZ(this Vector4 thiz, float value) => thiz.WithI(2, value);
        public static Vector4 WithW(this Vector4 thiz, float value) => thiz.WithI(3, value);
        /// R/G/B/A
        public static Color WithI(this Color thiz, int dim, float value)
        {
            thiz[dim] = value;
            return thiz;
        }
        public static Color WithR(this Color thiz, float value) => thiz.WithI(0, value);
        public static Color WithG(this Color thiz, float value) => thiz.WithI(1, value);
        public static Color WithB(this Color thiz, float value) => thiz.WithI(2, value);
        public static Color WithA(this Color thiz, float value) => thiz.WithI(3, value);

        public static RectInt Containing(this RectInt thiz, RectInt other)
        {
            RectInt retval = default;
            retval.SetMinMax(
                new Vector2Int(Mathf.Min(thiz.xMin, other.xMin), Mathf.Min(thiz.yMin, other.yMin)),
                new Vector2Int(Mathf.Max(thiz.xMax, other.xMax), Mathf.Max(thiz.yMax, other.yMax))
            );
            return retval;
        }
        public static void SetMinMax(ref this Rect thiz, Vector2 min, Vector2 max)
        => thiz.Set(
            (min.x + max.x) / 2f, (min.y + max.y) / 2f,
            Mathf.Abs(max.x - min.x), Mathf.Abs(max.y - min.y));
        public static void Encapsulate(ref this Rect thiz, Rect other)
        => thiz.SetMinMax(
            new Vector2(Mathf.Min(thiz.xMin, other.xMin), Mathf.Min(thiz.yMin, other.yMin)),
            new Vector2(Mathf.Max(thiz.xMax, other.xMax), Mathf.Max(thiz.yMax, other.yMax))
        );
        public static Vector2 ClosestPoint(this Rect thiz, Vector2 other)
        => new Bounds(thiz.center, thiz.size).ClosestPoint(other);

        public static void Encapsulate(ref this Rect thiz, Vector2 other)
        => thiz.Encapsulate(new Rect(other, Vector2.zero));

        public static void Encapsulate(ref this BoundsInt thiz, BoundsInt other)
        => thiz.SetMinMax(
            new Vector3Int(Mathf.Min(thiz.xMin, other.xMin), Mathf.Min(thiz.yMin, other.yMin), Mathf.Min(thiz.zMin, other.zMin)),
            new Vector3Int(Mathf.Max(thiz.xMax, other.xMax), Mathf.Max(thiz.yMax, other.yMax), Mathf.Max(thiz.zMax, other.zMax))
        );
        public static void Encapsulate(ref this BoundsInt thiz, Vector3Int other)
        => thiz.Encapsulate(new BoundsInt(other, Vector3Int.zero));

        public static Extent ScaledBy(in this Extent thiz, float scale, float moveCenter = 0f)
        {
            float center = thiz.center + moveCenter;
            float half = thiz.size * scale / 2f;
            return Extent.MinMax(center - half, center + half);
        }
        public static Rect ScaledBy(in this Rect thiz, float scale, Vector2 moveCenter = default)
        {
            Vector2 center = thiz.center + moveCenter;
            Vector2 half = thiz.size * scale / 2f;
            return new(center - half, 2 * half);
        }
        public static Bounds ScaledBy(in this Bounds thiz, float scale, Vector3 moveCenter = default)
        {
            Vector3 center = thiz.center + moveCenter;
            Vector3 half = thiz.size * scale / 2f;
            return new(center - half, 2 * half);
        }

        public static void Override(ref this Vector2 thiz, in Vector2 @override)
        {
            if (float.IsFinite(@override.x)) thiz.x = @override.x;
            if (float.IsFinite(@override.y)) thiz.y = @override.y;
        }
        public static Vector2 Overridden(in this Vector2 thiz, in Vector2 @override)
        {
            Vector2 ret = thiz;
            ret.Override(@override);
            return ret;
        }
        public static void Override(ref this Vector3 thiz, in Vector3 @override)
        {
            if (float.IsFinite(@override.x)) thiz.x = @override.x;
            if (float.IsFinite(@override.y)) thiz.y = @override.y;
            if (float.IsFinite(@override.z)) thiz.z = @override.z;
        }
        public static Vector3 Overridden(in this Vector3 thiz, in Vector3 @override)
        {
            Vector3 ret = thiz;
            ret.Override(@override);
            return ret;
        }
        public static void Override(ref this Vector4 thiz, in Vector4 @override)
        {
            if (float.IsFinite(@override.x)) thiz.x = @override.x;
            if (float.IsFinite(@override.y)) thiz.y = @override.y;
            if (float.IsFinite(@override.z)) thiz.z = @override.z;
            if (float.IsFinite(@override.w)) thiz.w = @override.w;
        }
        public static void Override(ref this Quaternion thiz, in Quaternion @override)
        {
            if (float.IsFinite(@override.x)) thiz.x = @override.x;
            if (float.IsFinite(@override.y)) thiz.y = @override.y;
            if (float.IsFinite(@override.z)) thiz.z = @override.z;
            if (float.IsFinite(@override.w)) thiz.w = @override.w;
        }
        public static void Override(ref this Color thiz, in Color @override)
        {
            if (float.IsFinite(@override.r)) thiz.r = @override.r;
            if (float.IsFinite(@override.g)) thiz.g = @override.g;
            if (float.IsFinite(@override.b)) thiz.b = @override.b;
            if (float.IsFinite(@override.a)) thiz.a = @override.a;
        }
        public static Color Overridden(in this Color thiz, in Color @override)
        {
            Color ret = thiz;
            ret.Override(@override);
            return ret;
        }
        public static void Override(ref this HSVA thiz, in HSVA @override)
        {
            if (float.IsFinite(@override.h)) thiz.h = @override.h;
            if (float.IsFinite(@override.s)) thiz.s = @override.s;
            if (float.IsFinite(@override.v)) thiz.v = @override.v;
            if (float.IsFinite(@override.a)) thiz.a = @override.a;
        }
        public static HSVA Overridden(in this HSVA thiz, in HSVA @override)
        {
            HSVA ret = thiz;
            ret.Override(@override);
            return ret;
        }

        public static Vector2 OfAngle(float degrees)
        => new(Mathf.Cos(Mathf.Deg2Rad * degrees), Mathf.Sin(Mathf.Deg2Rad * degrees));
    }
}