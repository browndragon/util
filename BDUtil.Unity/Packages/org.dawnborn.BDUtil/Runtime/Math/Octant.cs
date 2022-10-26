using UnityEngine;

namespace BDUtil.Math
{
    public enum Octant
    {
        ENE = default,
        NNE,
        NNW,
        WNW,
        WSW,
        SSW,
        SSE,
        ESE,
    }
    public static class Octants
    {
        /// Transforms a major/minor pair in the ENE octant into a major/minor pair in any other octant.
        /// This isn't a rotation; it's
        public static Vector2Int AsVector(this Octant thiz, int major, int minor) => thiz switch
        {
            Octant.ENE => new(major, minor),
            Octant.NNE => new(minor, major),
            Octant.NNW => new(-minor, major),
            Octant.WNW => new(-major, minor),
            Octant.WSW => new(-major, -minor),
            Octant.SSW => new(-minor, -major),
            Octant.SSE => new(minor, -major),
            Octant.ESE => new(major, -minor),
            _ => throw thiz.BadValue(),
        };
        public static void AsMajorMinor(this Octant thiz, Vector2Int vector, out int major, out int minor)
        {
            switch (thiz)
            {
                case Octant.ENE: major = vector.x; minor = vector.y; break;
                case Octant.NNE: major = vector.y; minor = vector.x; break;
                case Octant.NNW: major = vector.y; minor = -vector.x; break;
                case Octant.WNW: major = -vector.x; minor = vector.y; break;
                case Octant.WSW: major = -vector.x; minor = -vector.y; break;
                case Octant.SSW: major = -vector.y; minor = -vector.x; break;
                case Octant.SSE: major = -vector.y; minor = vector.x; break;
                case Octant.ESE: major = vector.x; minor = -vector.y; break;
                default: throw thiz.BadValue();
            }
        }
        public static Octant BestOctant(this Vector2Int thiz)
        {
            float angle = Vector2.SignedAngle(Vector2.right, thiz);
            angle = Mathf.Repeat(angle, 360f);
            angle /= 45f;
            int iangle = (int)angle;
            if (iangle == 8) iangle = 0;
            return Enums<Octant>.FromValue(iangle);
        }
    }
}