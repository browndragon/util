using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BDUtil.Fluent;

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
        public static int PreIncrement<K>(this IDictionary<K, int> thiz, K key, int incr = 1)
        {
            if (!thiz.TryGetValue(key, out int value)) value = 0;
            int sum = value + incr;
            if (sum == 0) thiz.Remove(key);
            else thiz[key] = sum;
            return value;
        }
        public static int Increment<K>(this IDictionary<K, int> thiz, K key, int incr = 1)
        {
            int sum = incr + (thiz.TryGetValue(key, out int value) ? value : 0);
            if (sum == 0) thiz.Remove(key);
            else thiz[key] = sum;
            return sum;
        }
        public static int Decrement<K>(this IDictionary<K, int> thiz, K key, int decr = 1)
        => thiz.Increment(key, -decr);

        public static bool Contains<K, VColl, V>(this IDictionary<K, VColl> thiz, K key, V value)
        where VColl : ICollection<V>
        => thiz.TryGetValue(key, out VColl vs) && vs.Contains(value);

        public static IReadOnlyCollection<V> GetValueOrDefault<K, VColl, V>(this IReadOnlyDictionary<K, VColl> thiz, K key, V _)
        where VColl : ICollection<V>
        => thiz.TryGetValue(key, out VColl vs) ? (IReadOnlyCollection<V>)(object)vs : None<V>.Default;

        public static void Add<K, VColl, V>(this IDictionary<K, VColl> thiz, K key, V value)
        where VColl : ICollection<V>, new()
        {
            if (!thiz.TryGetValue(key, out VColl vs)) thiz.Add(key, vs = new());
            vs.Add(value);
        }
        public static void Add<K, VColl, V>(this IDictionary<K, VColl> thiz, KeyValuePair<K, V> kvp)
        where VColl : ICollection<V>, new()
        => thiz.Add(kvp.Key, kvp.Value);

        public static IReadOnlyCollection<V> RemoveKey<K, VColl, V>(this IDictionary<K, VColl> thiz, K key, V _)
        where VColl : ICollection<V>
        {
            if (!thiz.TryGetValue(key, out VColl vs)) return None<V>.Default;
            thiz.Remove(key).OrThrowInternal();
            return (IReadOnlyCollection<V>)vs;
        }
        public static bool Remove<K, VColl, V>(this IDictionary<K, VColl> thiz, K key, V value)
        where VColl : ICollection<V>
        {
            if (!thiz.TryGetValue(key, out VColl vs)) return false;
            if (!vs.Remove(value)) return false;
            if (vs.IsEmpty()) thiz.Remove(key).OrThrowInternal("Couldn't remove empty values for {0}", key);
            return true;
        }
        public static bool Remove<K, VColl, V>(this IDictionary<K, VColl> thiz, KeyValuePair<K, V> kvp)
        where VColl : ICollection<V>
        => thiz.Remove(kvp);

        public static bool PopKey<K, V>(this IDictionary<K, V> thiz, K key, out V value)
        {
            if (!thiz.TryGetValue(key, out value)) return false;
            return thiz.Remove(key);
        }
        public static V PopKeyOrDefault<K, V>(this IDictionary<K, V> thiz, K key, V @default = default)
        => thiz.PopKey(key, out V v) ? v : @default;

        public static IEnumerable<KeyValuePair<K, V>> Flatten<K, VColl, V>(this IEnumerable<KeyValuePair<K, VColl>> thiz, V _)
        where VColl : IEnumerable<V>
        {
            foreach (KeyValuePair<K, VColl> kpvs in thiz) foreach (V v in kpvs.Value) yield return new(kpvs.Key, v);
        }

        /// Creates a facade which upcasts an internal type to its parent during enumeration (THANKS KVP...)
        public class Upcast<K, V1, V2> : IReadOnlyDictionary<K, V2>
        where V1 : V2
        {
            readonly IReadOnlyDictionary<K, V1> Thiz;
            public Upcast(IReadOnlyDictionary<K, V1> thiz) => Thiz = thiz;

            public V2 this[K key] => Thiz[key];
            public IEnumerable<K> Keys => Thiz.Keys;
            public IEnumerable<V2> Values => Thiz.Values.Cast<V2>();
            public int Count => Thiz.Count;

            public bool ContainsKey(K key) => Thiz.ContainsKey(key);
            public IEnumerator<KeyValuePair<K, V2>> GetEnumerator()
            { foreach (KeyValuePair<K, V1> kvp in Thiz) yield return new(kvp.Key, kvp.Value); }
            public bool TryGetValue(K key, out V2 value)
            => Thiz.TryGetValue(key, out V1 v1).Let(value = v1);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        /// Creates a facade which conforms to ILookup. The underlying map is mutable; it guarantees no caching.
        public readonly struct Lookup<K, VColl, V> : ILookup<K, V>
        where VColl : ICollection<V>
        {
            readonly IReadOnlyDictionary<K, VColl> Thiz;
            public Lookup(IReadOnlyDictionary<K, VColl> thiz) => Thiz = thiz;
            public IEnumerable<V> this[K key] => Thiz.TryGetValue(key, out VColl vs) ? vs : None<V>.Default;
            /// UGH.
            public int Count => Thiz.Count;
            public bool Contains(K key) => Thiz.ContainsKey(key);
            public IEnumerator<IGrouping<K, V>> GetEnumerator() => Thiz.Select(
                kvp => new Grouping<K, V>(kvp.Key, (IReadOnlyCollection<V>)kvp.Value)
            ).Cast<IGrouping<K, V>>().GetEnumerator();

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
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

        internal class DE<K, V> : IDictionaryEnumerator
        {
            public DE(IEnumerator<KeyValuePair<K, V>> enumerator) => Enumerator = enumerator;
            readonly IEnumerator<KeyValuePair<K, V>> Enumerator;
            public object Key => Entry.Key;
            public object Value => Entry.Value;
            public DictionaryEntry Entry { get; set; } = default;
            public object Current => Entry;

            public bool MoveNext()
            {
                Entry = default;
                if (!Enumerator.MoveNext()) return false;
                Entry = new DictionaryEntry(Enumerator.Current.Key, Enumerator.Current.Value);
                return true;
            }

            public void Reset()
            {
                Entry = default;
                Enumerator.Reset();
            }
        }
        public static IDictionaryEnumerator GetDictionaryEnumerator<K, V>(IEnumerable<KeyValuePair<K, V>> thiz)
        => new DE<K, V>(thiz.GetEnumerator());

    }
}