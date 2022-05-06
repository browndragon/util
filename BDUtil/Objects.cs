using System;
using System.Collections.Generic;

namespace BDUtil
{
    public static class Objects
    {
        public static bool IsEmpty(this string thiz) => thiz == null || thiz.Length <= 0;
        public static void OrThrow(this bool thiz, string tmpl = default, params object[] args)
        {
            if (!thiz) throw new Exception(string.Format(tmpl ?? "Unexpected False", args));
        }
        public static T OrThrow<T>(this T thiz, string tmpl = default, params object[] args)
        where T : class
        {
            if (thiz == null) throw new Exception(string.Format(tmpl ?? "Unexpected Null", args));
            return thiz;
        }
        public static string OrThrow(this string thiz, string tmpl = default, params object[] args)
        {
            if (thiz.IsEmpty()) throw new Exception(string.Format(tmpl ?? "Unexpected empty string", args));
            return thiz;
        }
        public static void AndThrow(this bool thiz, string tmpl = default, params object[] args)
        {
            if (thiz) throw new Exception(string.Format(tmpl ?? "Unexpected True", args));
        }

        public static bool EqualsT<T>(this T a, T b) => EqualityComparer<T>.Default.Equals(a, b);
        public static bool AsEqual<T>(this T thiz, object unknown) where T : IEquatable<T>
            => unknown is T other && thiz.Equals(other);
        public static bool AsEqual<T>(this IEqualityComparer<T> thiz, T known, object unknown)
            => unknown is T other && thiz.Equals(known, other);
        public static int AsCompare<T>(this T thiz, object unknown) where T : IComparable<T>
            => unknown is T other
            ? thiz.CompareTo(other)
            : throw new InvalidCastException($"{thiz?.GetType()}!={unknown?.GetType()}");
        public static int AsCompare<T>(this IComparer<T> thiz, T known, object unknown)
            => unknown is T other
            ? thiz.Compare(known, other)
            : throw new InvalidCastException($"{known?.GetType()}!={unknown?.GetType()}");
    }
}