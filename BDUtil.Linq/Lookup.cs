using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BDUtil.Linq
{
    /// A collection of self-keyed self-keyed-groups ("a multimap" with some asterisks).
    /// The key iteration order will not match insertion order (though it will match *group* insertion order).
    /// Things like Count are the number of *keys* and not the number of total entries in all groupings!
    public readonly struct Lookup<K, V> : ILookup<K, V>
    {
        readonly IReadOnlyDictionary<K, IContainer<V>> Thiz;
        public Lookup(IReadOnlyDictionary<K, IContainer<V>> thiz) => Thiz = thiz;
        public IContainer<V> this[K key] => Thiz.GetValueOrDefault(key, None<V>.Default);
        IEnumerable<V> ILookup<K, V>.this[K key] => this[key];

        public int Count => Thiz.Count;
        public bool Contains(K key) => Thiz.ContainsKey(key);
        public IEnumerator<Grouping<K, V>> GetEnumerator()
        {
            foreach (var kvp in Thiz) yield return new(kvp.Key, kvp.Value);
        }
        IEnumerator<IGrouping<K, V>> IEnumerable<IGrouping<K, V>>.GetEnumerator()
        {
            foreach (var kvp in Thiz) yield return new Grouping<K, V>(kvp.Key, kvp.Value);
        }
        IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<IGrouping<K, V>>)this).GetEnumerator();
    }
}