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
    public struct Interval : IEquatable<Interval>
    {
        public static readonly Interval zero = default;
        public float min;
        public float max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Interval(float min, float max)
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
        public bool Overlaps(Interval other) => other.min <= max && min <= other.max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(float position, float size)
        {
            min = position;
            max = position + size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClampToBounds(Interval other)
        {
            float min = System.Math.Max(this.min, other.min);
            float max = System.Math.Min(this.max, other.max);
            this.min = min;
            this.max = max;
        }
        public float GetClampedPoint(float x) => x.GetValenceInclusive(min, max) switch
        {
            true => max,
            null => x,
            false => min,
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(Interval other) => this == other;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(Interval a, Interval b) => a.min == b.min && a.max == b.max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(Interval a, Interval b) => a.min != b.min || a.max != b.max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) => other is Interval e && this == e;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Chain.Hash ^ min ^ max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"[{min}, {max}]";
    }
    public static class Extents
    {
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float NormalizedToPoint(this Interval span, float normalized)
        {
            normalized *= span.size;
            normalized += span.position;
            return normalized;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float PointToNormalized(this Interval span, float point)
        {
            point -= span.position;
            point /= span.size;
            return point;
        }
    }
}