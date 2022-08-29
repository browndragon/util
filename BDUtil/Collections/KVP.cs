using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BDUtil.Raw;

namespace BDUtil
{
    /// Utilities for getting keys & values out of enumerable `KeyValuePair` structures.
    /// This helps comply with `System.Collections.Generic` contracts.
    public static class KVP
    {
        public static KeyValuePair<K, V> New<K, V>(K k, V v) => new(k, v);
        public static KeyValuePair<K, V>[] Of<K, V>(params KeyValuePair<K, V>[] args) => args;
        public static KeyValuePair<V, K> Reverse<K, V>(this KeyValuePair<K, V> thiz)
        => new(thiz.Value, thiz.Key);

        /// A serializable standin for KeyValuePair<K,V> (which is itself not serializable).
        [Serializable]
        public struct Entry<TKey, TValue>
        {
            public TKey Key;
            public TValue Value;
            public Entry(TKey key, TValue value)
            {
                Key = key;
                Value = value;
            }
            public void Deconstruct(out TKey key, out TValue value)
            {
                key = Key;
                value = Value;
            }
            public static implicit operator KeyValuePair<TKey, TValue>(Entry<TKey, TValue> thiz) => new(thiz.Key, thiz.Value);
            public static implicit operator Entry<TKey, TValue>(KeyValuePair<TKey, TValue> thiz) => new(thiz.Key, thiz.Value);
        }

        public readonly struct Grouping<TKey, TValue> : IGrouping<TKey, TValue>, IReadOnlyCollection<TValue>
        {
            public readonly TKey Key;
            public readonly IReadOnlyCollection<TValue> Values;
            public Grouping(TKey key, IReadOnlyCollection<TValue> values)
            {
                Key = key;
                Values = values;
            }
            public void Deconstruct(out TKey key, out IReadOnlyCollection<TValue> values)
            {
                key = Key;
                values = Values;
            }
            public int Count => Values?.Count ?? 0;
            TKey IGrouping<TKey, TValue>.Key => Key;
            public IEnumerator<TValue> GetEnumerator() => Values?.GetEnumerator() ?? None<TValue>.Default;
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// Guaranteed to use only `.Count`, `.ContainsKey` (if available), and the collection `.GetEnumerator`.
        /// Contains is as efficient as the underlying dictionary's Contains.
        public readonly struct Keys<K, V> : ICollection<K>, IReadOnlyCollection<K>
        {
            readonly IReadOnlyCollection<KeyValuePair<K, V>> Thiz;
            public Keys(IReadOnlyCollection<KeyValuePair<K, V>> thiz) => Thiz = thiz;
            public int Count => Thiz.Count;
            public bool IsReadOnly => true;
            public void Add(K item) => throw new NotImplementedException();
            public void Clear() => throw new NotImplementedException();
            public bool Contains(K item) => Thiz.ContainsKey(item);

            public void CopyTo(K[] array, int arrayIndex) => Iter.WriteTo(this, array, arrayIndex);

            public IEnumerator<K> GetEnumerator() { foreach (var kvp in Thiz) yield return kvp.Key; }
            public bool Remove(K item) => throw new NotImplementedException();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        /// Warning: Inefficient contains!
        /// Guaranteed to use only `.Count` and the collection `.GetEnumerator`.
        public readonly struct Values<K, V> : ICollection<V>, IReadOnlyCollection<V>
        {
            readonly IReadOnlyCollection<KeyValuePair<K, V>> Thiz;
            public Values(IReadOnlyCollection<KeyValuePair<K, V>> thiz) => Thiz = thiz;
            public int Count => Thiz.Count;
            public bool IsReadOnly => true;
            public void Add(V item) => throw new NotImplementedException();
            public void Clear() => throw new NotImplementedException();
            public bool Contains(V item) => Thiz.ContainsValue(item);
            public void CopyTo(V[] array, int arrayIndex) => Iter.WriteTo(this, array, arrayIndex);

            public IEnumerator<V> GetEnumerator() { foreach (var kvp in Thiz) yield return kvp.Value; }
            public bool Remove(V item) => throw new System.NotImplementedException();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}