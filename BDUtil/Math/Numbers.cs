using System;
using System.Diagnostics.CodeAnalysis;

namespace BDUtil.Math
{
    public static class Numbers
    {
        [SuppressMessage("IDE", "IDE1006")]
        public static T @switch<T>(this bool? thiz, T @true = default, T @null = default, T @false = default)
        => thiz switch { true => @true, null => @null, false => @false };
        public static int AsPInt(this bool? thiz, int @true = +1, int @null = 0, int @false = -1)
        => thiz.@switch(@true, @null, @false);
        public static float AsPFloat(this bool? thiz, float @true = +1f, float @null = 0f, float @false = -1f)
        => thiz.@switch(@true, @null, @false);


        /// Returns true if thiz > 0, false if < 0, and null if ==0.
        public static bool? Valence(this int thiz)
        => thiz > 0 ? true : thiz < 0 ? false : null;
        public static int PosMod(this int thiz, int mod)
        => (thiz % mod + mod) % mod;
        public static bool IsInRange(this int thiz, int min, int max)
        => min <= thiz & thiz < max;
        public static bool? GetValence(this int thiz, int min, int max)
        => thiz >= max ? true : thiz < min ? false : null;
        public static int CheckRange(this int thiz, int min, int max, string context = default)
        => thiz.IsInRange(min, max) ? thiz
        : throw new IndexOutOfRangeException($"{thiz} <> [{min},{max}) {context}");
        public static bool IsInRangeInclusive(this int thiz, int min, int max)
        => min <= thiz & thiz <= max;
        public static bool? GetValenceInclusive(this int thiz, int min, int max)
        => thiz > max ? true : thiz < min ? false : null;

        public static int CheckRangeInclusive(this int thiz, int min, int max, string context = default)
        => thiz.IsInRangeInclusive(min, max)
        ? thiz
        : throw new IndexOutOfRangeException($"{thiz} <> [{min},{max}] {context}");

        /// Returns true if thiz > 0, false if < 0, and null if ==0.
        public static bool? Valence(this float thiz)
        => thiz > 0f ? true : thiz < 0f ? false : null;
        public static float PosMod(this float thiz, float y)
        => (thiz % y + y) % y;
        public static bool IsInRange(this float thiz, float min, float max)
        => min <= thiz & thiz < max;
        /// as with Valence: true is gt, null is within, and false is lt.
        /// null if ==min (but not max!)
        public static bool? GetValence(this float thiz, float min, float max)
        => thiz >= max ? true : thiz < min ? false : null;
        public static bool IsInRangeInclusive(this float thiz, float min, float max)
        => min <= thiz & thiz <= max;
        /// As GetValence but ==min and ==max both return null.
        public static bool? GetValenceInclusive(this float thiz, float min, float max)
        => thiz > max ? true : thiz < min ? false : null;
        public static float CheckRange(this float thiz, float min, float max, string context = default)
        => thiz.IsInRange(min, max)
        ? thiz
        : throw new IndexOutOfRangeException($"{thiz} <> [{min},{max}] {context}");
        public static float CheckRangeInclusive(this float thiz, float min, float max, string context = default)
        => thiz.IsInRangeInclusive(min, max)
        ? thiz
        : throw new IndexOutOfRangeException($"{thiz} <> [{min},{max}] {context}");
    }
}