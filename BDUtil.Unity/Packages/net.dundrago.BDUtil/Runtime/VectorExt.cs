using System;
using UnityEngine;

namespace BDUtil
{
    public static class VectorExt
    {
        public static Vector2Int Abs(this Vector2Int thiz) => new(Mathf.Abs(thiz.x), Mathf.Abs(thiz.y));
        public static Vector2 Abs(this Vector2 thiz) => new(Mathf.Abs(thiz.x), Mathf.Abs(thiz.y));
        public static Vector2Int Sign(this Vector2 thiz) => new(Math.Sign(thiz.x), Math.Sign(thiz.y));
        public static Vector2Int Sign(this Vector2Int thiz) => new(Math.Sign(thiz.x), Math.Sign(thiz.y));
        public static float Max(this Vector2 thiz) => Mathf.Max(thiz.x, thiz.y);
        public static float Min(this Vector2 thiz) => Mathf.Min(thiz.x, thiz.y);
        public static Vector3 WithZ(this Vector2 thiz, float z = 0f) => new(thiz.x, thiz.y, z);
        public static Vector3Int WithZ(this Vector2Int thiz, int z = 0) => new(thiz.x, thiz.y, z);

        public static Vector2Int AsInt(this Vector2 thiz) => new(Mathf.RoundToInt(thiz.x), Mathf.RoundToInt(thiz.y));
        public static Vector2 AsFloat(this Vector2Int thiz) => new(thiz.x, thiz.y);
        public static RectInt AsSizeInt(this Vector2Int thiz) => new(Vector2Int.zero, thiz);
        public static Rect AsSize(this Vector2 thiz) => new(Vector2.zero, thiz);


        public static Vector3Int Abs(this Vector3Int thiz) => new(Mathf.Abs(thiz.x), Mathf.Abs(thiz.y), Mathf.Abs(thiz.z));
        public static Vector3Int AsInt(this Vector3 thiz) => new(Mathf.RoundToInt(thiz.x), Mathf.RoundToInt(thiz.y), Mathf.RoundToInt(thiz.z));
        public static Vector3 AsFloat(this Vector3Int thiz) => new(thiz.x, thiz.y, thiz.z);
        public static Vector2 AsTwo(this Vector3 thiz) => (Vector2)thiz;

        public static BoundsInt AsSizeInt(this Vector3Int thiz) => new(Vector3Int.zero, thiz);
        public static Bounds AsSize(this Vector3 thiz) => new(Vector3.zero, thiz);

        public static RectInt Contain(this RectInt thiz, RectInt other)
        {
            RectInt retval = default;
            retval.SetMinMax(
                new Vector2Int(Mathf.Min(thiz.xMin, other.xMin), Mathf.Min(thiz.yMin, other.yMin)),
                new Vector2Int(Mathf.Max(thiz.xMax, other.xMax), Mathf.Max(thiz.yMax, other.yMax))
            );
            return retval;
        }
        public static BoundsInt Contain(this BoundsInt thiz, BoundsInt other)
        {
            BoundsInt retval = default;
            retval.SetMinMax(
                new Vector3Int(Mathf.Min(thiz.xMin, other.xMin), Mathf.Min(thiz.yMin, other.yMin), Mathf.Min(thiz.zMin, other.zMin)),
                new Vector3Int(Mathf.Max(thiz.xMax, other.xMax), Mathf.Max(thiz.yMax, other.yMax), Mathf.Max(thiz.zMax, other.zMax))
            );
            return retval;
        }
    }
}