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
        public int position;
        public int size;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public ExtentInt(int position, int size)
        {
            this.position = position;
            this.size = size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void ClampToBounds(ExtentInt other)
        {
            int min = System.Math.Max(this.min, other.min);
            int max = System.Math.Min(this.max, other.max);
            position = min;
            size = max - min;
        }
        public float center
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => position + size / 2f;
        }
        public int min
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => position;
        }
        public int max
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => position + size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Contains(int x) => x.IsInRangeInclusive(min, max);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Overlaps(ExtentInt other) => other.min <= max && min <= other.max;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public void Set(int position, int size)
        {
            this.position = position;
            this.size = size;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static ExtentInt MinMax(int min, int max)
        {
            if (max < min) (min, max) = (max, min);
            int position = min;
            int size = max - min;
            return new(position, size);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public bool Equals(ExtentInt other) => this == other;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator ==(ExtentInt a, ExtentInt b) => a.position == b.position && a.size == b.size;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator !=(ExtentInt a, ExtentInt b) => a.position == b.position && a.size == b.size;

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override bool Equals(object other) => other is ExtentInt e && this == e;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override int GetHashCode() => Chain.Hash ^ position ^ size;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public override string ToString() => $"[min, max]";

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