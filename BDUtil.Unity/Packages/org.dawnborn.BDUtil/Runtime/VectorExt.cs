using UnityEngine;

namespace BDUtil
{
    public static class VectorExt
    {
        public static Vector2Int Abs(this Vector2Int thiz) => new(Mathf.Abs(thiz.x), Mathf.Abs(thiz.y));
        public static Vector2 Abs(this Vector2 thiz) => new(Mathf.Abs(thiz.x), Mathf.Abs(thiz.y));
        public static Vector2Int Sign(this Vector2 thiz) => new((int)Mathf.Sign(thiz.x), (int)Mathf.Sign(thiz.y));
        public static Vector2Int Sign(this Vector2Int thiz) => new((int)Mathf.Sign(thiz.x), (int)Mathf.Sign(thiz.y));
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

        public static Vector3 Clamp(this Vector3 thiz, float min = 0f, float max = 1f)
        {
            if (max <= 0f) return Vector3.zero;
            float magnitude = thiz.sqrMagnitude;
            if (magnitude >= (min * min) && magnitude <= (max * max)) return thiz;
            magnitude = Mathf.Sqrt(magnitude);
            return (Mathf.Clamp(magnitude, min, max) / magnitude) * thiz;
        }
        public static Vector2 Clamp(this Vector2 thiz, float min = 0f, float max = 1f)
        {
            if (max <= 0f) return Vector2.zero;
            float magnitude = thiz.sqrMagnitude;
            if (magnitude >= (min * min) && magnitude <= (max * max)) return thiz;
            magnitude = Mathf.Sqrt(magnitude);
            return (Mathf.Clamp(magnitude, min, max) / magnitude) * thiz;
        }

        public static readonly float Epsilon = 1e-06f;

        public static bool Approximately(this Vector3 thiz, Vector3 other, float epsilon)
        {
            Vector3 delta = other - thiz;
            if (Mathf.Abs(delta.x) > epsilon) return false;
            if (Mathf.Abs(delta.y) > epsilon) return false;
            if (Mathf.Abs(delta.z) > epsilon) return false;
            return true;
        }
        public static bool Approximately(this Vector3 thiz, Vector3 other = default) => thiz.Approximately(other, Epsilon);
        public static bool Approximately(this Vector2 thiz, Vector2 other, float epsilon)
        {
            Vector2 delta = other - thiz;
            if (Mathf.Abs(delta.x) > epsilon) return false;
            if (Mathf.Abs(delta.y) > epsilon) return false;
            return true;
        }
        public static bool Approximately(this Vector2 thiz, Vector2 other = default) => thiz.Approximately(other, Epsilon);
        public static Vector3 Rounded(this Vector3 thiz, float resolution = 1f) => new(
            resolution * Mathf.Round(thiz.x / resolution),
            resolution * Mathf.Round(thiz.y / resolution),
            resolution * Mathf.Round(thiz.z / resolution)
        );
        public static Vector2 Rounded(this Vector2 thiz, float resolution = 1f) => new(
            resolution * Mathf.Round(thiz.x / resolution),
            resolution * Mathf.Round(thiz.y / resolution)
        );


        /// Projects a ray as though it were a line and discovers where it intersects z (default =0).
        /// This is great for projecting camera rays to hit world 0, where theoretically the plane of the camera might break the plane of the scene...
        public static Vector3 AtZ(this Ray thiz, float z = 0f)
        {
            /// It will never ever ever intersect.
            if (thiz.direction.z == 0f) return Vector2.positiveInfinity;
            float scale = (thiz.origin.z - z) / thiz.direction.z;  // If negative, naturally converges.
            return thiz.origin - scale * thiz.direction;
        }
    }
}