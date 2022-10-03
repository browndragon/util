using System;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

namespace BDUtil.Math
{
    /// Reinterpret-cast a 32 bit quantity as another (BUT NOT OF EQUAL VALUE).
    /// For instance, this lets you take a float and return its bits as an integer.
    [StructLayout(LayoutKind.Explicit)]
    public readonly ref struct Bitcast
    {
        [FieldOffset(0)]
        public readonly int @int;
        [FieldOffset(0)]
        public readonly float @float;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitcast(int @int)
        {
            this = default;
            this.@int = @int;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public Bitcast(float @float)
        {
            this = default;
            this.@float = @float;
        }

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static int Int(float f) => new Bitcast(f).@int;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static float Float(int i) => new Bitcast(i).@float;
    }
}
