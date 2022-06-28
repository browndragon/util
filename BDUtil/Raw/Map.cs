using System.Collections.Generic;

namespace BDUtil.Raw
{
    /// A 1:1 Dictionary<K,V>.
    public class Map<K, V> : Collection<K, KeyValuePair<K, V>>, IDictionary<K, V>, IReadOnlyDictionary<K, V>, ITryGetValue<K, V>, IRemoveKey<K, V>
    {
        protected override K GetKey(KeyValuePair<K, V> item) => item.Key;

        public V this[K key]
        {
            get => TryGetValue(key, out V v) ? v : throw new KeyNotFoundException($"Missing {key}");
            set => Replace(key, value, out var _);
        }
        V IReadOnlyDictionary<K, V>.this[K key] => this[key];
        public KVP.Keys<Map<K, V>, K, V> Keys => new(this);
        public KVP.Values<Map<K, V>, K, V> Values => new(this);
        ICollection<K> IDictionary<K, V>.Keys => Keys;
        IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => Keys;
        ICollection<V> IDictionary<K, V>.Values => Values;
        IEnumerable<V> IReadOnlyDictionary<K, V>.Values => Values;

        public bool TryAdd(K key, V value) => TryAdd(new(key, value));
        public void Add(K key, V value) => Add(new(key, value));
        public bool Replace(K key, V value, out V old)
        {
            var entry = GetEntry(key);
            if (entry.HasValue)
            {
                old = entry.Value.Value;
                entry.Value = new(key, value);
                return true;
            }
            Add(key, value);
            old = default;
            return false;
        }
        public bool ContainsKey(K key) => GetEntry(key).HasValue;
        public bool RemoveKey(K key, out V value)
        {
            var entry = GetEntry(key);
            if (!entry.HasValue)
            {
                value = default;
                return false;
            }
            value = entry.Value.Value;
            RemoveEntry(entry);
            return true;
        }
        public bool RemoveKey(K key) => RemoveKey(key, out var _);
        bool IDictionary<K, V>.Remove(K key) => RemoveKey(key);
        public bool Remove(K key, V value) => Remove(new(key, value));

        public bool TryGetValue(K key, out V value)
        {
            var entry = GetEntry(key);
            if (!entry.HasValue)
            {
                value = default;
                return false;
            }
            value = entry.Value.Value;
            return true;
        }
    }
}