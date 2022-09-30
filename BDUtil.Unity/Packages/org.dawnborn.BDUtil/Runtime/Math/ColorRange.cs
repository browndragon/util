using System;
using UnityEngine;

namespace BDUtil.Math
{
    [Serializable]
    public struct ColorRange : IEquatable<ColorRange>
    {
        public static ColorRange Black = new(Color.black, Color.black);
        public static ColorRange White = new(Color.white, Color.white);
        public static ColorRange Clear = new(Color.clear, Color.clear);

        public Color Min;
        public Color Max;

        [Serializable]
        public struct HSVA : IEquatable<HSVA>
        {
            public MinMax H;
            public MinMax S;
            public MinMax V;
            public MinMax A;
            public HSVA(MinMax h, MinMax s, MinMax v, MinMax a)
            {
                H = h;
                S = s;
                V = v;
                A = a;
            }
            public HSVA(Color min, Color max)
            {
                Color.RGBToHSV(min, out float hmin, out float smin, out float vmin);
                Color.RGBToHSV(max, out float hmax, out float smax, out float vmax);
                H = new MinMax(hmin, hmax).Ordered;
                S = new MinMax(smin, smax).Ordered;
                V = new MinMax(vmin, vmax).Ordered;
                A = new MinMax(min.a, max.a).Ordered;
            }
            public bool IsValid => H.IsValid && S.IsValid && V.IsValid && A.IsValid;
            public bool Contains(HSVA other) => H.Contains(other.H) && S.Contains(other.S) && V.Contains(other.V) && A.Contains(other.A);
            public bool Overlaps(HSVA other) => H.Overlaps(other.H) && S.Overlaps(other.S) && V.Overlaps(other.V) && A.Overlaps(other.A);
            public bool Contains(Color value)
            {
                Color.RGBToHSV(value, out float h, out float s, out float v);
                return H.Contains(h) && S.Contains(s) && V.Contains(v) && A.Contains(value.a);
            }

            public HSVA Union(HSVA value) => new(H.Union(value.H), S.Union(value.S), V.Union(value.V), A.Union(value.A));
            public HSVA Intersect(HSVA value) => new(H.Intersect(value.H), S.Intersect(value.S), V.Intersect(value.V), A.Intersect(value.A));
            /// Warning: these areas have to be contiguous, so this can surprise you!
            /// [-2,+2].Difference([-1,+1]) => [+1, -1], which is invalid, because there would be wingies left over.
            /// But: [-2, +2].Difference([-3, -1]) => [-1, +2], which is what you expect.
            public HSVA Difference(HSVA value) => new(H.Difference(value.H), S.Difference(value.S), V.Difference(value.V), A.Difference(value.A));
            public HSVA Union(Color value)
            {
                Color.RGBToHSV(value, out float h, out float s, out float v);
                return new(H.Union(h), S.Union(s), V.Union(v), A.Union(value.a));
            }
            public Color Random => UnityEngine.Random.ColorHSV(
                H.Min, H.Max,
                S.Min, S.Max,
                V.Min, V.Max,
                A.Min, A.Max
            );
            public bool Equals(HSVA other) => H == other.H && S == other.S && V == other.V && A == other.A;
            public static bool operator ==(HSVA a, HSVA b) => a.Equals(b);
            public static bool operator !=(HSVA a, HSVA b) => !a.Equals(b);
            public ColorRange AsColorRange => new(
                Color.HSVToRGB(H.Min, S.Min, V.Min).WithA(A.Min),
                Color.HSVToRGB(H.Max, S.Max, V.Max).WithA(A.Max)
            );
            public static implicit operator ColorRange(HSVA thiz) => thiz.AsColorRange;
            public override bool Equals(object other) => other is HSVA hsva && Equals(hsva);
            public override int GetHashCode() => Chain.Hash ^ H.GetHashCode() ^ S.GetHashCode() ^ V.GetHashCode() ^ A.GetHashCode();
            public override string ToString() => $"<H={H},S={S},V={V},A={A}>";
        }
        public ColorRange(Color min, Color max)
        {
            Min = min;
            Max = max;
        }
        public HSVA Value => new(Min, Max);
        public static implicit operator HSVA(ColorRange thiz) => thiz.Value;

        public bool Equals(ColorRange other) => Min == other.Min && Max == other.Max;
        public override bool Equals(object other) => other is ColorRange colorRange && Equals(colorRange);
        public override int GetHashCode() => Chain.Hash ^ Min.GetHashCode() ^ Max.GetHashCode();
        public override string ToString() => $"[{Min},{Max}]";
    }
}