using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil
{
    /// Object/reference utilties.
    public static class Objects
    {
        public static readonly string NullFalseyLabel = "`null`";
        static string GetFalseyLabel(this bool thiz) => thiz ? null : "`false`";
        static string GetFalseyLabel(this float thiz) => float.IsNaN(thiz) ? "`NaN`f" : null;
        static string GetFalseyLabel(this double thiz) => double.IsNaN(thiz) ? "`NaN`d" : null;
        static string GetFalseyLabel(this string thiz) => thiz.IsEmpty() ? "''" : null;
        static string GetFalseyLabel(this Array thiz) => thiz.IsEmpty() ? "[]" : null;
        static string GetFalseyLabel(this IEnumerable thiz) => thiz.IsEmpty() ? "{}" : null;

        /// Returns "" or else the falsey label for the case we hit.
        static string GetFalseyLabel<T>(this T thiz)
        => thiz switch
        {
            null => NullFalseyLabel,
            bool x => x.GetFalseyLabel(),
            float x => x.GetFalseyLabel(),
            double x => x.GetFalseyLabel(),
            string x => x.GetFalseyLabel(),
            Array x => x.GetFalseyLabel(),
            IEnumerable x => x.GetFalseyLabel(),
            _ => Converter<T, bool>.Default?.Convert(thiz) ?? true ? null : ("Falsey=" + thiz.GetType()),
        };

        public static T OrThrow<T>(this T thiz, string tmpl = default, params object[] args)
        {
            string label = thiz.GetFalseyLabel();
            if (label != null) throw new ArgumentException($"Unexpected {label}:{string.Format(tmpl ?? "", args)}");
            return thiz;
        }
        public static T OrThrowInternal<T>(this T thiz, string tmpl = default, params object[] args)
        {
            string label = thiz.GetFalseyLabel();
            if (label != null) throw new InvalidOperationException($"Unexpected {label}:{string.Format(tmpl ?? "", args)}");
            return thiz;
        }
        public static void AndThrow<T>(this T thiz, string tmpl = default, params object[] args)
        {
            string label = thiz.GetFalseyLabel();
            if (label == null) throw new InvalidOperationException($"Unexpectedly truthy {thiz}:{string.Format(tmpl ?? "", args)}");
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