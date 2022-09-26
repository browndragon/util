using System;
using System.Collections.Generic;
using System.Reflection;

namespace BDUtil
{
    /// Descriptor & integer-mapping information about an enum.
    public static class Enums<U> where U : Enum
    {
        static readonly U[] declaredOrder;
        public static readonly long MinL, MaxL;
        public static readonly U MinU, MaxU;
        public static readonly bool IsFlags;
        /// Only meaningful if this is a Flags.
        public static readonly U Everything;
        static Enums()
        {
            IsFlags = typeof(U).GetCustomAttribute<FlagsAttribute>() != null;
            declaredOrder = (U[])Enum.GetValues(typeof(U));
            MinL = long.MaxValue;
            MaxL = long.MinValue;
            ulong everything = 0;
            foreach (U u in declaredOrder)
            {
                long value = Convert.ToInt64(u);
                everything |= (ulong)value;
                if (value < MinL)
                {
                    MinL = value;
                    MinU = u;
                }
                if (value > MaxL)
                {
                    MaxL = value;
                    MaxU = u;
                }
                Everything = IsFlags ? (U)Enum.ToObject(typeof(U), everything) : default;
            }
        }
        public static IReadOnlyList<U> Entries => declaredOrder;
        /// That is, length in integer space between the min & max element.
        public static long Span => (long)(1 + MaxL - MinL);
        public static bool HasValue(U u) => IsFlags ? GetValue(GetInvalidFlags(u)) == 0L : Enum.IsDefined(typeof(U), u);
        public static U GetInvalidFlags(U u)
        {
            long l = GetValue(u);
            foreach (U value in declaredOrder)
            {
                l &= ~GetValue(value);
            }
            return FromValue(l);
        }
        public static long GetValue(U u) => Convert.ToInt64(u);
        public static U FromValue(long i) => (U)Enum.ToObject(typeof(U), i);
        public static long GetOffset(U u)
        {
            long v = GetValue(u);
            if (v > MaxL) return -1;
            return v - MinL;
        }
        public static U FromOffset(long i)
        => i >= 0
        ? FromValue(i + MinL)
        : throw new ArgumentException($"No possible value for offset {i}");
    }
    public static class Enums
    {
        /// Create a new exception declaring this is not a named enum value.
        /// Usual use is final case in a ` x switch {..., _=>throw x.BadValue() };
        public static NotImplementedException BadValue<E>(this E thiz)
        where E : Enum
        => new($"Unhandled {thiz} <> {thiz.GetType()}");
        /// Throw BadValue if the value is not named; usable on assignment or something.
        public static E OrThrow<E>(this E thiz)
        where E : Enum
        => Enums<E>.HasValue(thiz) ? thiz : throw thiz.BadValue();
    }
}