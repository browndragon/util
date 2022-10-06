using System;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BDUtil.Math
{
    // A one-dimensional rectangle/bounds object.
    [Serializable]
    [SuppressMessage("IDE", "IDE0064")]
    [SuppressMessage("IDE", "IDE1006")]
    [StructLayout(LayoutKind.Sequential)]
    public struct Extent : IEquatable<Extent>
    {
        public static readonly Extent zero = default;
        public float position;
        public float size;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Extent(float position, float size)
        {
            this.position = position;
            this.size = size;
        }
        public float center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => position + size / 2;
        }
        public float min
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => position;
        }
        public float max
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => position + size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(float x) => x.IsInRangeInclusive(min, max);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(Extent other) => other.min <= max && min <= other.max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float position, float size)
        {
            this.position = position;
            this.size = size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClampToBounds(Extent other)
        {
            float min = System.Math.Max(this.min, other.min);
            float max = System.Math.Min(this.max, other.max);
            position = min;
            size = max - min;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Extent MinMax(float min, float max)
        {
            if (max < min) (min, max) = (max, min);
            float position = min;
            float size = max - min;
            return new(position, size);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormalizedToPoint(Extent span, float normalized)
        {
            normalized *= span.size;
            normalized += span.position;
            return normalized;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PointToNormalized(Extent span, float point)
        {
            point -= span.position;
            point /= span.size;
            return point;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Extent other) => this == other;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Extent a, Extent b) => a.position == b.position && a.size == b.size;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Extent a, Extent b) => a.position == b.position && a.size == b.size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) => other is Extent e && this == e;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Chain.Hash ^ position ^ size;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"[min, max]";
    }
}