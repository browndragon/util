using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.CompilerServices;

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
        public static long Span
        {
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            get => (long)(1 + MaxL - MinL);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasValue(U u)
        => IsFlags ? GetValue(GetInvalidFlags(u)) == 0L : Enum.IsDefined(typeof(U), u);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U GetInvalidFlags(U u)
        {
            long l = GetValue(u);
            foreach (U value in declaredOrder)
            {
                l &= ~GetValue(value);
            }
            return FromValue(l);
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetValue(U u) => Convert.ToInt64(u);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U FromValue(long i) => (U)Enum.ToObject(typeof(U), i);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static long GetOffset(U u)
        {
            long v = GetValue(u);
            if (v > MaxL) return -1;
            return v - MinL;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static U FromOffset(long i)
        => i >= 0
        ? FromValue(i + MinL)
        : throw new ArgumentException($"No possible value for offset {i}");
    }
    public static class Enums
    {
        /// Create a new exception declaring this is not a named enum value.
        /// Usual use is final case in a ` x switch {..., _=>throw x.BadValue() };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static NotImplementedException BadValue<E>(this E thiz)
        where E : Enum
        => new($"Unhandled {thiz} <> {thiz.GetType()}");
        /// Throw BadValue if the value is not named; usable on assignment or something.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static E OrThrow<E>(this E thiz)
        where E : Enum
        => Enums<E>.HasValue(thiz) ? thiz : throw thiz.BadValue();

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAllFlags<E>(this E thiz, E flags)
        where E : Enum
        => thiz.HasFlag(flags);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool HasAnyFlags<E>(this E thiz, E flags)
        where E : Enum
        => 0 != (Enums<E>.GetValue(thiz) & Enums<E>.GetValue(flags));
    }
}