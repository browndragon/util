using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Linq;

namespace BDUtil.Linq
{
    public static class Grouping
    {
        public static Grouping<K, T> AsGrouping<TT, K, T>(this KeyValuePair<K, TT> kvp)
        where TT : IEnumerable<T>
        => new(kvp.Key, kvp.Value);
    }
    public readonly struct Grouping<K, T> : IGrouping<K, T>
    {
        readonly K Key;
        readonly IEnumerable<T> Enumerable;
        public Grouping(K key, IEnumerable<T> enumerable) { Key = key; Enumerable = enumerable; }

        public IEnumerator<T> GetEnumerator() => Enumerable.GetEnumerator();
        K IGrouping<K, T>.Key => Key;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}