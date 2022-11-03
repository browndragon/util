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
        public float min;
        public float max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Extent(float min, float max)
        {
            this.min = min;
            this.max = max;
        }
        public float center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (min + max) / 2;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { float radius = this.radius; min = value - radius; max = value + radius; }
        }
        public float position
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => min;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => min = value;
        }
        public float size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => max - min;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set => radius = value / 2;
        }
        public float radius
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => size / 2;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            set { float center = this.center; min = center - value; max = center + value; }
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(float x) => x.IsInRangeInclusive(min, max);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(Extent other) => other.min <= max && min <= other.max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float position, float size)
        {
            min = position;
            max = position + size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClampToBounds(Extent other)
        {
            float min = System.Math.Max(this.min, other.min);
            float max = System.Math.Min(this.max, other.max);
            this.min = min;
            this.max = max;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Extent other) => this == other;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Extent a, Extent b) => a.min == b.min && a.max == b.max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Extent a, Extent b) => a.min != b.min || a.max != b.max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) => other is Extent e && this == e;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Chain.Hash ^ min ^ max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"[{min}, {max}]";
    }
    public static class Extents
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormalizedToPoint(this Extent span, float normalized)
        {
            normalized *= span.size;
            normalized += span.position;
            return normalized;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PointToNormalized(this Extent span, float point)
        {
            point -= span.position;
            point /= span.size;
            return point;
        }
    }
}