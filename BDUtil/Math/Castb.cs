using System.Runtime.InteropServices;

namespace BDUtil.Math
{
    /// Reinterpret-cast a 32 bit quantity as another (BUT NOT OF EQUAL VALUE).
    /// For instance, this lets you take a float and return its bits as an integer.
    [StructLayout(LayoutKind.Explicit)]
    public readonly ref struct Castb<TI, TU, TF>
    {
        [FieldOffset(0)]
        public readonly TI Int;
        [FieldOffset(0)]
        public readonly TU Uint;
        [FieldOffset(0)]
        public readonly TF Float;
        public Castb(TI v)
        {
            Float = default;
            Uint = default;
            Int = v;
        }
        public Castb(TU v)
        {
            Float = default;
            Int = default;
            Uint = v;
        }
        public Castb(TF v)
        {
            Int = default;
            Uint = default;
            Float = v;
        }
    }
    public readonly ref struct Cast32b
    {
        readonly Castb<int, uint, float> Cast;
        public Cast32b(int v) => Cast = new(v);
        public Cast32b(uint v) => Cast = new(v);
        public Cast32b(float v) => Cast = new(v);
        public int Int => Cast.Int;
        public uint Uint => Cast.Uint;
        public float Float => Cast.Float;
    }
    public readonly ref struct Cast64b
    {
        readonly Castb<long, ulong, double> Cast;
        public Cast64b(long v) => Cast = new(v);
        public Cast64b(ulong v) => Cast = new(v);
        public Cast64b(double v) => Cast = new(v);

        public long Long => Cast.Int;
        public ulong Ulong => Cast.Uint;
        public double Double => Cast.Float;

    }
}

