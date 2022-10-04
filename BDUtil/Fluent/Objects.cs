using System;
using System.Collections;
using System.Runtime.CompilerServices;

namespace BDUtil.Fluent
{
    /// Object/reference utilties.
    public static class Objects
    {
        public static readonly string NullFalseyLabel = "`null`";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetFalseyLabel(this bool thiz) => thiz ? null : "`false`";
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetFalseyLabel(this float thiz) => float.IsNaN(thiz) ? "`NaN`f" : null;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetFalseyLabel(this double thiz) => double.IsNaN(thiz) ? "`NaN`d" : null;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetFalseyLabel(this string thiz) => thiz.IsEmpty() ? "''" : null;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetFalseyLabel(this Array thiz) => thiz.IsEmpty() ? "[]" : null;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        static string GetFalseyLabel(this IEnumerable thiz) => thiz.IsEmpty() ? "{}" : null;

        /// Returns "" or else the falsey label for the case we hit.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
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
            _ => typeof(T) == typeof(object) ? null
                : Converter<T, bool>.Default?.Convert(thiz) ?? true ? null : ("Falsey=" + thiz.GetType()),
        };

        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T OrThrow<T>(this T thiz, string tmpl = default, params object[] args)
        {
            string label = thiz.GetFalseyLabel();
            if (label != null) throw new ArgumentException($"Unexpected {label}:{string.Format(tmpl ?? "", args)}");
            return thiz;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T OrThrowInternal<T>(this T thiz, string tmpl = default, params object[] args)
        {
            string label = thiz.GetFalseyLabel();
            if (label != null) throw new InvalidOperationException($"Unexpected {label}:{string.Format(tmpl ?? "", args)}");
            return thiz;
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static void AndThrow<T>(this T thiz, string tmpl = default, params object[] args)
        {
            string label = thiz.GetFalseyLabel();
            if (label == null) throw new InvalidOperationException($"Unexpectedly truthy {thiz}:{string.Format(tmpl ?? "", args)}");
        }
        /// Okay, it's trivial, but symmetry demands!
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TOut Upcast<TIn, TOut>(this TIn thiz)
        where TIn : TOut
        => thiz;
        /// ClassCastExceptions are fucking useless.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TOut Downcast<TIn, TOut>(this TIn thiz, TOut _ = default)
        where TOut : TIn
        {
            try { return (TOut)thiz; }
            catch { throw new InvalidCastException($"Can't upcast actual {thiz?.GetType()?.Name ?? "null"}->{typeof(TOut)}"); }
        }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static TOut Anycast<TOut>(this object thiz, TOut _ = default)
        {
            try { return (TOut)thiz; }
            catch { throw new InvalidCastException($"Can't anycast actual {thiz?.GetType()?.Name ?? "null"}->{typeof(TOut)}"); }
        }

        /// Lets you write "new Timer(1f).Let(out var start, 12).Let(out var end, 14).Foreach(t=>tween(start, end, t))".
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Let<T, T1>(this T thiz, out T1 bindee, in T1 value)
        {
            bindee = value;
            return thiz;
        }

        /// discards the arguments returning thiz (allowing side effects during an assignment)
        /// Particularly useful for boolish phrases or during a `tern` expression.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Let<T>(this T thiz, params object[] _) => thiz;
        /// discards the arguments returning thiz (allowing side effects during an assignment)
        /// Particularly useful for boolish phrases or during a `tern` expression.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Let<T, T1>(this T thiz, in T1 _1) => thiz;
        /// discards the arguments returning thiz (allowing side effects during an assignment)
        /// Particularly useful for boolish phrases or during a `tern` expression.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Let<T, T1, T2>(this T thiz, in T1 _1, in T2 _2) => thiz;
        /// discards the arguments returning thiz (allowing side effects during an assignment)
        /// Particularly useful for boolish phrases or during a `tern` expression.
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static T Let<T, T1, T2, T3>(this T thiz, in T1 _1, in T2 _2, in T3 _3) => thiz;
    }
}