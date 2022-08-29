using System;
using System.Collections.Generic;
using System.Linq;

namespace BDUtil
{
    [Serializable]
    public sealed class MultiMap<K, V> : StoreMap<Raw.Multi.Map<K, V>, K, V>, Multi.IReadOnlyMap<K, V>, Multi.IMap<K, V>
    {
        public IReadOnlyCollection<V> this[K key] => AsCollection[key];
        IEnumerable<V> ILookup<K, V>.this[K key] => ((ILookup<K, V>)AsCollection)[key];

        public IReadOnlyCollection<KeyValuePair<K, V>> AsKVPs => AsCollection.AsKVPs;
        public ILookup<K, V> AsLookup => AsCollection.AsLookup;
        public ICollection<K> Keys => AsCollection.Keys;
        public bool Contains(K key, V value) => AsCollection.Contains(key, value);
        public bool ContainsKey(K key) => AsCollection.ContainsKey(key);
        bool ILookup<K, V>.Contains(K key) => AsCollection.ContainsKey(key);

        public bool Remove(K key, V value) => AsCollection.Remove(key, value);
        public IReadOnlyCollection<V> RemoveKey(K key) => AsCollection.RemoveKey(key);

        public bool TryAdd(K key, V value) => AsCollection.TryAdd(key, value);
        public bool TryGetValue(K key, out IReadOnlyCollection<V> value) => AsCollection.TryGetValue(key, out value);

        IEnumerator<IGrouping<K, V>> IEnumerable<IGrouping<K, V>>.GetEnumerator()
        => ((IEnumerable<IGrouping<K, V>>)AsCollection).GetEnumerator();
    }
}
