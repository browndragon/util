using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;

namespace BDUtil
{
    public static class Numbers
    {
        public static int PosMod(this int thiz, int mod)
        => (thiz % mod + mod) % mod;
        public static int CheckRange(this int thiz, int min, int max, string context = default)
        => min <= thiz & thiz < max
        ? thiz
        : throw new IndexOutOfRangeException($"{thiz} <> [{min},{max}) {context}");
        public static int CheckRangeInclusive(this int thiz, int min, int max, string context = default)
        => min <= thiz & thiz <= max
        ? thiz
        : throw new IndexOutOfRangeException($"{thiz} <> [{min},{max}] {context}");

        public static float PosMod(this float thiz, float y)
        => (thiz % y + y) % y;
        public static float CheckRange(this float thiz, float min, float max, string context = default)
        => min <= thiz & thiz <= max
        ? thiz
        : throw new IndexOutOfRangeException($"{thiz} <> [{min},{max}] {context}");
    }
}