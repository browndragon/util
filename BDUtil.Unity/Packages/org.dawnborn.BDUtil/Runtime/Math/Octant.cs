using UnityEngine;

namespace BDUtil.Math
{
    /// 45-degree slices starting at the +x axis.
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
        Zero = -1
    }
    public static class Octants
    {
        /// Transforms a major/minor pair in the ENE octant into a major/minor pair in any other octant.
        /// This isn't a rotation; it's a series of mirrors such that major is along +/-x/y, and minor is along the appropriate other one.
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
        /// See MajorMinor; breaks a vector down into major & minor components. If `thiz` contains `vector`, they'll both be positive and minor<major;
        /// otherwise one might be negative or minor >= major.
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
        /// Given a vector and an optional rotation bias (-22.5f?), return the best octant for that vector.
        public static Octant BestOctant(this Vector2Int thiz, float biasAngle = 0f) => thiz == default ? Octant.Zero : BestOctant((Vector2)thiz, biasAngle);
        /// Given a vector and an optional rotation bias (-22.5f?), return the best octant for that vector.
        public static Octant BestOctant(this Vector2 thiz, float biasAngle = 0f) => thiz == default ? Octant.Zero : BestOctant(Vector2.SignedAngle(Vector2.right, thiz) + biasAngle);
        /// Given an angle return the best octant for that vector.
        public static Octant BestOctant(float angle)
        {
            if (!float.IsFinite(angle)) return Octant.Zero;
            angle = Mathf.Repeat(angle, 360f);
            angle /= 45f;
            int iangle = (int)angle;
            if (iangle == 8) iangle = 0;  // This should basically never happen...
            return Enums<Octant>.FromValue(iangle);
        }
        /// Gets the [0,360) clockwise-edge of the octant wedge.
        public static float GetClockwiseDegrees(this Octant thiz) => thiz == Octant.Zero ? float.NaN : Enums<Octant>.GetValue(thiz) * 45f;
    }
}