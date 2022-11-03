using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BDUtil.Math
{
    // A one-dimensional rectangle/bounds object.
    // *does* includes its maximal size (by analogy with RectInt).
    [Serializable]
    [SuppressMessage("IDE", "IDE0064")]
    [SuppressMessage("IDE", "IDE1006")]
    [StructLayout(LayoutKind.Sequential)]
    public struct ExtentInt : IEquatable<ExtentInt>
    {
        public static readonly ExtentInt zero = default;

        public int min;
        public int max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ExtentInt(int min, int max)
        {
            this.min = min;
            this.max = max;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClampToBounds(ExtentInt other)
        {
            int min = System.Math.Max(this.min, other.min);
            int max = System.Math.Min(this.max, other.max);
            this.min = min;
            this.max = max;
        }
        public float center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (min + max) / 2f;
        }
        public int size
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => max - min;
        }
        public float radius
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => size / 2f;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int x) => x.IsInRangeInclusive(min, max);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(ExtentInt other) => other.min <= max && min <= other.max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int position, int size)
        {
            min = position;
            max = position + size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ExtentInt other) => this == other;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ExtentInt a, ExtentInt b) => a.min == b.min && a.max == b.max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ExtentInt a, ExtentInt b) => a.min != b.min || a.max != b.max;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object obj) => obj is ExtentInt other && this == other;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Chain.Hash ^ min ^ max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"[{min}, {max}]";

        public struct Iter : IEnumerator<int>
        {
            private readonly int _min, _max;
            public int Current;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Iter(int min, int max)
            {
                _min = min;
                _max = max;
                Current = _min - 1;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset()
            {
                Current = _min - 1;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Iter GetEnumerator() => this;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => (++Current) <= _max;

            int IEnumerator<int>.Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Current;
            }

            object IEnumerator.Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Current;
            }

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            void IDisposable.Dispose() { }
        }
    }
}