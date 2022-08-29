using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil
{
    [Serializable]
    public sealed class BiMap<K, V> : StoreMap<Raw.Bi.Map<K, V>, K, V>, Bi.IReadOnlyMap<K, V>, Bi.IMap<K, V>
    {
        public Multi.IReadOnlyMap<V, K> Reverse => AsCollection.Reverse;
        public V this[K key] { get => AsCollection[key]; set => AsCollection[key] = value; }
        public ICollection<K> Keys => AsCollection.Keys;
        IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => ((IReadOnlyDictionary<K, V>)AsCollection).Keys;
        public ICollection<V> Values => AsCollection.Values;
        IEnumerable<V> IReadOnlyDictionary<K, V>.Values => ((IReadOnlyDictionary<K, V>)AsCollection).Values;

        public void Add(K key, V value) => AsCollection.Add(key, value);
        public bool ContainsKey(K key) => AsCollection.ContainsKey(key);
        public bool Remove(K key) => AsCollection.Remove(key);
        public bool Remove(K key, out V value) => AsCollection.TryGetValue(key, out value) && AsCollection.Remove(key);
        bool ICollection<KeyValuePair<K, V>>.Remove(KeyValuePair<K, V> item) => ((IDictionary<K, V>)AsCollection).Remove(item);
        public bool TryGetValue(K key, out V value) => AsCollection.TryGetValue(key, out value);
        public bool TryAdd(K key, V value) => AsCollection.TryAdd(key, value);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        bool ICollection<KeyValuePair<K, V>>.IsReadOnly => false;
    }
}
