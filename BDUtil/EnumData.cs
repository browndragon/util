using System;
using System.Collections.Generic;

namespace BDUtil
{
    /// Descriptor & integer-mapping information about an enum.
    public struct EnumData<U> where U : Enum
    {
        static readonly U[] declaredOrder;
        public static readonly int Min, Max;
        public static readonly U MinU, MaxU;
        static EnumData()
        {
            declaredOrder = (U[])Enum.GetValues(typeof(U));
            Min = int.MaxValue;
            Max = int.MinValue;
            foreach (U u in declaredOrder)
            {
                int uInt = Convert.ToInt32(u);
                if (uInt < Min)
                {
                    Min = uInt;
                    MinU = u;
                }
                if (uInt > Max)
                {
                    Max = uInt;
                    MaxU = u;
                }
            }
        }
        public static IReadOnlyCollection<U> Entries => declaredOrder;
        public static int Span => 1 + Max - Min;

        // This isn't really great... But it's mostly fine for well defined enums...
        public static bool HasValue(U _) => true;

        public static int GetValue(U u) => TryGetValue(u, out int v) ? v
            : throw new ArgumentException($"{u}={v} outside [{MinU}={Min}..{MaxU}={Max}]");

        public static bool TryGetValue(U u, out int value)
        {
            value = Convert.ToInt32(u);
            if (value < Min) return false;
            if (value > Max) return false;
            return Enum.IsDefined(typeof(U), u);
        }
        public static U FromValue(int i) => (U)Enum.ToObject(typeof(U), i);

        public static int GetOffset(U u) => GetValue(u) - Min;
        public static int GetOffsetOrNegative(U u) => TryGetValue(u, out int offset) ? (offset - Min) : -1;
        public static U FromOffset(int i) => i < 0 ? throw new ArgumentException($"No possible value for offset {i}") : FromValue(i + Min);

        public static float GetOffsetFraction(U u) => (float)GetOffset(u) / Span;
    }
}