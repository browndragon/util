using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDUtil.Fluent
{
    /// Extensions for anything listlike: IEnumerable, string, list, and array.
    /// Also contains extensions for working with IEnum
    public static class Iter
    {
        /// Mostly/entirely for unit testing
        public static T[] Of<T>(params T[] args) => args;
        /// start: start index, length: elements to remove from `start` (if negative, from thiz.Length), `value`: value to set.
        public static void Clear<T>(this T[] thiz, int start = 0, int length = -1, T value = default)
        {
            if (length < 0) length += thiz.Length + 1;  // So -1 -> thiz.Length, as expected.
            length += start;
            if (length > thiz.Length) length = thiz.Length;
            for (int i = start; i < length; ++i) thiz[i] = value;
        }
        public static bool IsEmpty(this string thiz) => string.IsNullOrEmpty(thiz);
        public static bool IsEmpty(this Array thiz) => thiz == null || thiz.Length <= 0;
        public static bool IsEmpty<T>(this T[] thiz) => thiz == null || thiz.Length <= 0;
        public static bool IsEmpty(this ICollection thiz) => thiz == null || thiz.Count <= 0;
        public static bool IsEmpty(this IEnumerable thiz)
        {
            switch (thiz)
            {
                case null: return true;
                case string s: return s.IsEmpty();
                case Array a: return a.IsEmpty();
                case ICollection c: return c.IsEmpty();
                default:
                    IEnumerator @enum = thiz.GetEnumerator();
                    using (@enum as IDisposable) return !@enum.MoveNext();
            }
        }

        public static void WriteTo(this IEnumerable thiz, IList array, int index)
        {
            foreach (var o in thiz) array[index++] = o;
        }
        public static void WriteTo<T>(this IEnumerable<T> thiz, IList<T> array, int arrayIndex)
        {
            foreach (var t in thiz) array[arrayIndex++] = t;
        }

        public static bool ContainsKey<K, V>(this IEnumerable<KeyValuePair<K, V>> thiz, K key)
        => thiz switch
        {
            IReadOnlyDictionary<K, V> irodict => irodict.ContainsKey(key),
            IDictionary<K, V> idict => idict.ContainsKey(key),
            _ => thiz.Select(kvp => kvp.Key).Contains(key),
        };
        public static bool ContainsValue<K, V>(this IEnumerable<KeyValuePair<K, V>> thiz, V value)
        => thiz.Select(kvp => kvp.Value).Contains(value);

        // Summarize an enumerable into something ...-able.
        public static string Summarize(this string thiz, int pre = 5, int post = 5)
        {
            if (thiz == null) return "<null>";
            if (thiz.Length <= 0) return "<nil>";
            int cut = thiz.Length - pre - post;
            if (cut <= 11) return thiz;  // we add 11+ more characters for the infix pattern.
            return new StringBuilder().Append(thiz, 0, pre)
                .Append("(..")
                .Append(cut)
                .Append(" cut..)")
                .Append(thiz, thiz.Length - post, post)
                .ToString();
        }

        // Summarize an enumerable into something ...-able.
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
            using var @enum = thiz.GetEnumerator();
            if (!@enum.MoveNext()) return "none";
            builder.Append(@enum.Current);
            int i = 1;
            while (@enum.MoveNext())
            {
                if (i++ > limit) { builder.Append(separator).Append(terminal); break; }
                builder.Append(separator).Append(@enum.Current);
            }
            return builder.ToString();
        }
        public static string Summarize(this IEnumerable thiz, int limit = 5, string separator = ", ", string terminal = default)
        {
            switch (thiz)
            {
                case null: return "null";
                case ICollection c: terminal ??= $"...(+{c.Count})"; break;
                default: terminal ??= "..."; break;
            }
            if (limit == 0) return terminal;

            StringBuilder builder = new();
            if (limit < 0) limit = int.MaxValue;
            var @enum = thiz.GetEnumerator();
            using (@enum as IDisposable)  // Just. In. Case.
            {
                if (!@enum.MoveNext()) return "none";
                builder.Append(@enum.Current);
                int i = 1;
                while (@enum.MoveNext())
                {
                    if (i++ > limit) { builder.Append(separator).Append(terminal); break; }
                    builder.Append(separator).Append(@enum.Current);
                }
                return builder.ToString();
            }
        }
        public static void Exhaust(this IEnumerator thiz) { while (thiz.MoveNext()) { } }
        public static void Exhaust(this IEnumerable thiz) { foreach (object _ in thiz) { } }
    }
}