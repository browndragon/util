using System.Collections.Generic;
using System.Text;

namespace BDUtil
{
    public static class Enumerables
    {
        public static bool TryGetValue<TEnum, TT>(ref this TEnum thiz, out TT current)
        where TEnum : struct, IEnumerator<TT>
        {
            bool hasNext = thiz.MoveNext();
            current = hasNext ? thiz.Current : default;
            return hasNext;
        }
        public static bool TryGetValue<TEnum, TT>(this TEnum thiz, out TT current)
         where TEnum : class, IEnumerator<TT>
        {
            bool hasNext = thiz.MoveNext();
            current = hasNext ? thiz.Current : default;
            return hasNext;
        }

        public static TT GetValueOrDefault<TEnum, TT>(ref this TEnum thiz, TT @default = default)
        where TEnum : struct, IEnumerator<TT>
        => thiz.TryGetValue(out TT value) ? value : @default;
        public static TT GetValueOrDefault<TEnum, TT>(this TEnum thiz, TT @default = default)
        where TEnum : class, IEnumerator<TT>
        => thiz.TryGetValue(out TT value) ? value : @default;


        public static string Summarize<T>(this IEnumerable<T> thiz, int limit = 5, string separator = ", ", string terminal = default)
        {
            terminal ??= "...";
            if (thiz == null) return "null";
            if (limit == 0) return "...";

            StringBuilder builder = new();
            if (limit < 0) limit = int.MaxValue;
            using IEnumerator<T> @enum = thiz.GetEnumerator();
            if (!@enum.TryGetValue(out T value)) return "none";
            builder.Append(value);
            int i = 1;
            while (@enum.MoveNext())
            {
                if (i++ > limit) { builder.Append(separator).Append(terminal); break; }
                builder.Append(separator).Append(@enum.Current);
            }
            return builder.ToString();
        }
    }
}