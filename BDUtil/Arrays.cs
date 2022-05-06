using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil
{
    public static class Arrays
    {
        public static bool IsEmpty(this string thiz) => thiz == null || thiz.Length <= 0;
        public static bool IsEmpty<T>(this T[] thiz) => thiz == null || thiz.Length <= 0;

        public static bool IsValidIndex(this Array thiz, int index) => index >= 0 && index < thiz.Length;

        public static void CopyTo(this IEnumerable thiz, Array array, int index)
        {
            foreach (var o in thiz) array.SetValue(o, index++);
        }
        public static void CopyTo<T>(this IEnumerable<T> thiz, T[] array, int arrayIndex)
        {
            foreach (var t in thiz) array[arrayIndex++] = t;
        }

        /// Mostly/entirely for unit testing
        public static T[] Of<T>(params T[] args) => args;
    }
}