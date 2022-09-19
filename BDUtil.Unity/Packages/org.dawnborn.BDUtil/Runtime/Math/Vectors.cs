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
    }
}