using System;

namespace BDUtil
{
    public static class Numbers
    {
        public static int PosMod(this int thiz, int mod)
        => (thiz % mod + mod) % mod;
        public static bool IsInRange(this int thiz, int min, int max)
        => min <= thiz & thiz < max;
        public static int CheckRange(this int thiz, int min, int max, string context = default)
        => thiz.IsInRange(min, max) ? thiz
        : throw new IndexOutOfRangeException($"{thiz} <> [{min},{max}) {context}");
        public static bool IsInRangeInclusive(this int thiz, int min, int max)
        => min <= thiz & thiz <= max;
        public static int CheckRangeInclusive(this int thiz, int min, int max, string context = default)
        => thiz.IsInRangeInclusive(min, max)
        ? thiz
        : throw new IndexOutOfRangeException($"{thiz} <> [{min},{max}] {context}");

        public static float PosMod(this float thiz, float y)
        => (thiz % y + y) % y;
        public static bool IsInRange(this float thiz, float min, float max)
        => min <= thiz & thiz < max;
        public static float CheckRange(this float thiz, float min, float max, string context = default)
        => thiz.IsInRange(min, max)
        ? thiz
        : throw new IndexOutOfRangeException($"{thiz} <> [{min},{max}] {context}");
    }
}