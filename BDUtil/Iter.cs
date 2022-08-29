using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using BDUtil.Traces;

namespace BDUtil
{
    /// Extensions for anything listlike: IEnumerable, string, list, and array.
    /// Also contains extensions for working with IEnum
    public static class Iter
    {
        /// Mostly/entirely for unit testing
        public static T[] Of<T>(params T[] args) => args;
        public static bool IsEmpty(this string thiz) => thiz == null || thiz.Length <= 0;
        public static bool IsEmpty<T>(this T[] thiz) => thiz == null || thiz.Length <= 0;
        public static bool IsEmpty<T>(this IEnumerable<T> thiz)
        {
            if (thiz == null) return true;
            foreach (T _ in thiz) return false;
            return true;
        }

        public static void WriteTo(this IEnumerable thiz, IList array, int index)
        {
            foreach (var o in thiz) array[index++] = o;
        }
        public static void WriteTo<T>(this IEnumerable<T> thiz, IList<T> array, int arrayIndex)
        {
            foreach (var t in thiz) array[arrayIndex++] = t;
        }

        public static bool ContainsKey<K, V>(this IReadOnlyCollection<KeyValuePair<K, V>> thiz, K key)
        => thiz switch
        {
            IReadOnlyDictionary<K, V> irodict => irodict.ContainsKey(key),
            IDictionary<K, V> idict => idict.ContainsKey(key),
            _ => thiz.Any(((Func<K, K, bool>)EqualityComparer<K>.Default.Equals).Curried(key, (KeyValuePair<K, V> kvp) => kvp.Key)),
        };
        public static bool ContainsValue<K, V>(this IReadOnlyCollection<KeyValuePair<K, V>> thiz, V value)
        => thiz.Any(((Func<V, V, bool>)EqualityComparer<V>.Default.Equals).Curried(value, (KeyValuePair<K, V> kvp) => kvp.Value));

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

        /// "adds" to a collection, sticking a remove in a disposeall.
        public static bool Subscribe<T>(this ICollection<T> thiz, T member, ref Dispose.All unsubscribe)
        {
            if (null == member) return false;
            if (thiz.Contains(member)) return false;
            thiz.Add(member);
            unsubscribe.Add(() => _ = member != null && thiz.Remove(member).OrTrace("{0}.Remove({1})", thiz, member));
            return true;
        }
        public static bool Subscribe<T>(this IList<T> thiz, T member, ref Dispose.All unsubscribe)
        {
            if (null == member) return false;
            thiz.Add(member);
            unsubscribe.Add(() => _ = member != null && thiz.Remove(member).OrTrace("{0}.Remove({1})", thiz, member));
            return true;
        }
        public static bool Subscribe<K, V>(this IDictionary<K, V> thiz, K key, V value, ref Dispose.All unsubscribe)
        {
            // This can only fail for badkey, so just check that first.
            if (null == key) return false;
            if (thiz.ContainsKey(key)) return false;
            thiz.Add(key, value);
            unsubscribe.Add(() => ((ICollection<KeyValuePair<K, V>>)thiz).Remove(new(key, value)).OrTrace("{0}.Remove({1},{2})", thiz, key, value));
            return true;
        }
        /// "adds" to a collection, sticking a remove in a disposeall.
        public static bool Subscribe<K, V>(this Multi.IMap<K, V> thiz, K key, V value, ref Dispose.All unsubscribe)
        {
            // This can only fail for badkey, so just check that first.
            if (null == key) return false;
            if (!thiz.TryAdd(key, value)) return false;
            unsubscribe.Add(() => thiz.Remove(key, value).OrTrace("{0}.Remove({1},{2})", thiz, key, value));
            return true;
        }
    }
}