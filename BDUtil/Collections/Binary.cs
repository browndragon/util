using System;
using System.Collections.Generic;
using BDUtil.Fluent;
using BDUtil.Math;

namespace BDUtil.Raw
{
    /// Code to treat lists as binary sort structures.
    /// All assume that the list has been sorted previously and maintain that invariant;
    /// expect crazy results if you mix Add or Insert operations...
    public static class Binary
    {
        public static int BinarySearch<T>(this IReadOnlyList<T> thiz, T value, IComparer<T> comparer = default)
        {
            comparer ??= Comparer<T>.Default.OrThrow();
            if (thiz is List<T> l) return l.BinarySearch(0, l.Count, value, comparer);
            if (thiz.IsEmpty()) return -1;
            int lower = 0, upper = thiz.Count - 1;
            while (lower <= upper)
            {
                int middle = lower + (upper - lower) / 2;
                switch (comparer.Compare(value, thiz[middle]).Valence())
                {
                    case true: lower = middle + 1; break;
                    case null: return middle;
                    case false: upper = middle - 1; break;
                }
            }
            return ~lower;
        }
        public static int BinaryInsert<T>(this IList<T> thiz, T value, IComparer<T> comparer = default, bool setSemantics = false)
        {
            int index = ((IReadOnlyList<T>)thiz).BinarySearch(value, comparer);
            if (index >= 0 && setSemantics) return ~index;
            if (index < 0) index = ~index;
            thiz.Insert(index, value);
            return index;
        }
        public static int BinaryRemove<T>(this IList<T> thiz, T value, IComparer<T> comparer = default)
        {
            int index = ((IReadOnlyList<T>)thiz).BinarySearch(value, comparer);
            if (index < 0) return index;
            thiz.RemoveAt(index);
            return index;
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
        {
            comparer ??= Comparer<T>.Default.OrThrow();
            if (thiz is List<T> l) l.Sort(comparer);
            else BinarySort(thiz, 0, thiz.Count - 1, comparer);
        }
        /// Actually: quick sort
        static void BinarySort<T>(IList<T> thiz, int leftIndex, int rightIndex, IComparer<T> comparer)
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
            if (leftIndex < j) BinarySort(thiz, leftIndex, j, comparer);
            if (i < rightIndex) BinarySort(thiz, i, rightIndex, comparer);
        }
    }
}