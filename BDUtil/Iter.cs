using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace BDUtil
{
    /// Extensions for anything listlike: IEnumerable, string, list, and array.
    /// Also contains extensions for working with IEnum
    public static class Iter
    {
        /// Mostly/entirely for unit testing
        public static T[] Of<T>(params T[] args) => args;
        public static bool IsEmpty(this string thiz) => thiz == null || thiz.Length <= 0;
        public static bool IsEmpty(this Array thiz) => thiz == null || thiz.Length <= 0;
        public static bool IsEmpty(this ICollection thiz) => thiz == null || thiz.Count <= 0;
        public static bool IsEmpty(this IEnumerable thiz)
        {
            switch (thiz)
            {
                case null: return true;
                case Array a: return a.IsEmpty();
                case ICollection c: return c.IsEmpty();
                default:
                    IEnumerator @enum = null;
                    try
                    {
                        @enum = thiz.GetEnumerator();
                        return !@enum.MoveNext();
                    }
                    finally { if (@enum is IDisposable d) d.Dispose(); }
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
            _ => thiz.Any(((Func<K, K, bool>)EqualityComparer<K>.Default.Equals).Curried(key, (KeyValuePair<K, V> kvp) => kvp.Key)),
        };
        public static bool ContainsValue<K, V>(this IEnumerable<KeyValuePair<K, V>> thiz, V value)
        => thiz.Any(((Func<V, V, bool>)EqualityComparer<V>.Default.Equals).Curried(value, (KeyValuePair<K, V> kvp) => kvp.Value));

        public static int BinarySearch<T>(this IReadOnlyList<T> thiz, T value, IComparer<T> comparer = default)
        {
            if (thiz.IsEmpty()) return -1;
            comparer ??= Comparer<T>.Default.OrThrow();
            int lower = 0, upper = thiz.Count - 1;
            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                switch ((bool?)(tern)comparer.Compare(value, thiz[middle]))
                {
                    case true: lower = middle + 1; break;
                    case null: return middle;
                    case false: upper = middle - 1; break;
                }
            }
            return ~lower;
        }
        public static int BinarySearchIndexOf<T>(this IReadOnlyList<T> thiz, Func<T, int> comparer = default)
        {
            comparer.OrThrow();
            if (thiz.IsEmpty()) return -1;
            int lower = 0, upper = thiz.Count - 1;
            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                switch ((bool?)(tern)comparer.Invoke(thiz[middle]))
                {
                    case true: lower = middle + 1; break;
                    case null: return middle;
                    case false: upper = middle - 1; break;
                }
            }
            return ~lower;
        }
        public static void BinarySort<T>(this IList<T> thiz, IComparer<T> comparer = default)
        => thiz.BinarySort(
            0,
            thiz.Count - 1,
            comparer ?? Comparer<T>.Default ?? throw new ArgumentException($"No default Comparer<{typeof(T)}>")
        );
        /// Actually: quick sort
        public static void BinarySort<T>(this IList<T> thiz, int leftIndex, int rightIndex, IComparer<T> comparer)
        {
            var i = leftIndex;
            var j = rightIndex;
            var pivot = thiz[leftIndex];
            while (i <= j)
            {
                while (comparer.Compare(thiz[i], pivot) < 0) i++;
                while (comparer.Compare(thiz[j], pivot) > 0) j--;
                if (i <= j)
                {
                    (thiz[j], thiz[i]) = (thiz[i], thiz[j]);
                    i++;
                    j--;
                }
            }
            if (leftIndex < j) thiz.BinarySort(leftIndex, j, comparer);
            if (i < rightIndex) thiz.BinarySort(i, rightIndex, comparer);
        }

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
        public static void Exhaust(this IEnumerator thiz) { while (thiz.MoveNext()) { } }
        public static void Exhaust(this IEnumerable thiz) { foreach (object _ in thiz) { } }
    }
}