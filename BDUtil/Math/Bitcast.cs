using System;
using System.Runtime.InteropServices;

namespace BDUtil.Math
{
    /// Reinterpret-cast a 32 bit quantity as another (BUT NOT OF EQUAL VALUE).
    /// For instance, this lets you take a float and return its bits as an integer.
    [StructLayout(LayoutKind.Explicit)]
    public readonly ref struct Bitcast<U, V>
    where U : unmanaged
    where V : unmanaged
    {
        [FieldOffset(0)]
        readonly U In;
        [FieldOffset(0)]
        public readonly V Out;
        public Bitcast(U @in)
        {
            Out = default;
            In = @in;
        }
        public static explicit operator Bitcast<U, V>(U @in) => new(@in);
        public static implicit operator V(Bitcast<U, V> thiz) => thiz.Out;
    }
    public static class Bitcast
    {
        public static int Int(float f) => new Bitcast<float, int>(f).Out;
        public static uint Uint(float f) => new Bitcast<float, uint>(f).Out;
        public static long Long(double d) => new Bitcast<double, int>(d).Out;
        public static ulong Ulong(double d) => new Bitcast<double, ulong>(d).Out;
    }
}
