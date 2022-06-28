using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Text;
using System.Threading.Tasks;

namespace BDUtil
{
    public static class Enumerables
    {
        public static bool TryGetNext<TEnum, TT>(ref this TEnum thiz, out TT current)
        where TEnum : struct, IEnumerator<TT>
        {
            bool hasNext = thiz.MoveNext();
            current = hasNext ? thiz.Current : default;
            return hasNext;
        }
        public static bool TryGetNext<TEnum, TT>(this TEnum thiz, out TT current)
         where TEnum : class, IEnumerator<TT>
        {
            bool hasNext = thiz.MoveNext();
            current = hasNext ? thiz.Current : default;
            return hasNext;
        }

        public static TT GetNextOrDefault<TEnum, TT>(ref this TEnum thiz, TT @default = default)
        where TEnum : struct, IEnumerator<TT>
        => thiz.TryGetNext(out TT value) ? value : @default;

        public static TT GetNextOrDefault<TEnum, TT>(this TEnum thiz, TT @default = default)
        where TEnum : class, IEnumerator<TT>
        => thiz.TryGetNext(out TT value) ? value : @default;


        // Summarize an enumerable
        public static string Summarize<T>(this IEnumerable<T> thiz, int limit = 5, string separator = ", ", string terminal = default)
        {
            switch (thiz)
            {
                case null: return "null";
                case IReadOnlyCollection<T> rot: terminal ??= $"...(+{rot.Count})"; break;
                case ICollection<T> c: terminal ??= $"...(+{c.Count})"; break;
                default: terminal ??= "..."; break;
            }
            if (limit == 0) return terminal;

            StringBuilder builder = new();
            if (limit < 0) limit = int.MaxValue;
            using IEnumerator<T> @enum = thiz.GetEnumerator();
            if (!@enum.TryGetNext(out T value)) return "none";
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