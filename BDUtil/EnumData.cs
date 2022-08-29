using System;
using System.Collections.Generic;

namespace BDUtil
{
    /// Descriptor & integer-mapping information about an enum.
    public static class Enums<U> where U : Enum
    {
        static readonly U[] declaredOrder;
        public static readonly int Min, Max;
        public static readonly U MinU, MaxU;
        static Enums()
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
        public static IReadOnlyList<U> Entries => declaredOrder;
        public static int Span => 1 + Max - Min;
        public static bool HasValue(U u) => Enum.IsDefined(typeof(U), u);
        public static int GetValue(U u) => TryGetValue(u, out int v) ? v : throw u.BadValue();
        public static bool TryGetValue(U u, out int value)
        => Enum.IsDefined(typeof(U), u).Let(value = Convert.ToInt32(u));
        public static U FromValue(int i) => (U)Enum.ToObject(typeof(U), i);
        public static int GetOffset(U u) => GetValue(u) - Min;
        public static int GetOffsetOrNegative(U u) => TryGetValue(u, out int offset) ? (offset - Min) : -1;
        public static U FromOffset(int i) => i < 0 ? throw new ArgumentException($"No possible value for offset {i}") : FromValue(i + Min);
        // public static float GetOffsetFraction(U u) => (float)GetOffset(u) / Span;
    }
    public static class Enums
    {
        /// Create a new exception declaring this is not a named enum value.
        /// Usual use is final case in a ` x switch {..., _=>throw x.BadValue() };
        public static ArgumentOutOfRangeException BadValue<E>(this E thiz)
        where E : Enum
        => new($"{thiz} <> {thiz.GetType()}");
        /// Throw BadValue if the value is not named.
        public static E OrThrow<E>(this E thiz)
        where E : Enum
        => Enums<E>.HasValue(thiz) ? thiz : throw thiz.BadValue();
    }

}