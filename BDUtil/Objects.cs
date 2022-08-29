using System;
using System.Collections.Generic;

namespace BDUtil
{
    /// Object/reference utilties.
    public static class Objects
    {
        public static bool OrThrow(this bool thiz, string tmpl = default, params object[] args)
        {
            if (!thiz) throw new ArgumentException(string.Format(tmpl ?? "Unexpected False", args));
            return thiz;
        }
        public static T OrThrow<T>(this T thiz, string tmpl = default, params object[] args)
        where T : class
        {
            if (thiz == null) throw new ArgumentException(string.Format(tmpl ?? $"Unexpected Null", args));
            return thiz;
        }
        public static string OrThrow(this string thiz, string tmpl = default, params object[] args)
        {
            if (thiz.IsEmpty()) throw new ArgumentException(string.Format(tmpl ?? "Unexpected empty string", args));
            return thiz;
        }
        public static IEnumerable<T> OrThrow<T>(this IEnumerable<T> thiz, string tmpl = default, params object[] args)
        where T : class
        {
            if (thiz.IsEmpty()) throw new ArgumentException(string.Format(tmpl ?? $"Unexpected Null", args));
            return thiz;
        }

        public static bool AndThrow(this bool thiz, string tmpl = default, params object[] args)
        {
            if (thiz) throw new ArgumentException(string.Format(tmpl ?? "Unexpected True", args));
            return thiz;
        }

        /// discards the arguments returning thiz (allowing side effects during an assignment)
        /// Particularly useful for boolish phrases or during a `tern` expression.
        public static T Let<T>(this T thiz, params object[] _) => thiz;
        /// discards the arguments returning thiz (allowing side effects during an assignment)
        /// Particularly useful for boolish phrases or during a `tern` expression.
        public static T Let<T, T1>(this T thiz, in T1 _1) => thiz;
        /// discards the arguments returning thiz (allowing side effects during an assignment)
        /// Particularly useful for boolish phrases or during a `tern` expression.
        public static T Let<T, T1, T2>(this T thiz, in T1 _1, in T2 _2) => thiz;
        /// discards the arguments returning thiz (allowing side effects during an assignment)
        /// Particularly useful for boolish phrases or during a `tern` expression.
        public static T Let<T, T1, T2, T3>(this T thiz, in T1 _1, in T2 _2, in T3 _3) => thiz;
    }
}