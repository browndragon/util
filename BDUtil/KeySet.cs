using System.Collections.Generic;

namespace BDUtil
{
    /// A set which provides an index into its elements.
    /// They're basically dictionaries, but the key is often embedded or derivable from the data:
    /// the hash code, an object reference, etc.
    /// This is the correct class to use with unity objects, whose GetInstanceIDs are not
    /// guaranteed stable between runs; depending on serialization, this might last longer?
    public abstract class KeySet<K, T> : Collection<K, T>, IReadOnlyDictionary<K, T>, ITryGetValue<K, T>, IRemoveKey<K, T>
    {
        public T this[K key] => TryGetValue(key, out T value) ? value : throw new KeyNotFoundException($"Missing {key}");
        public KVP.Keys<KeySet<K, T>, K, T> Keys => new(this);
        IEnumerable<K> IReadOnlyDictionary<K, T>.Keys => Keys;
        IEnumerable<T> IReadOnlyDictionary<K, T>.Values => this;
        public bool ContainsKey(K key) => GetEntry(key).HasValue;
        public bool Remove(K key) => RemoveKey(key, out var _);
        public bool RemoveKey(K key, out T value)
        {
            var entry = GetEntry(key);
            if (!entry.HasValue)
            {
                value = default;
                return false;
            }
            value = entry.Value;
            RemoveEntry(entry);
            return true;
        }

        public bool TryGetValue(K key, out T value)
        {
            var entry = GetEntry(key);
            if (entry.HasValue)
            {
                value = entry.Value;
                return true;
            }
            value = default;
            return false;
        }
        IEnumerator<KeyValuePair<K, T>> IEnumerable<KeyValuePair<K, T>>.GetEnumerator()
        {
            foreach (T t in this) yield return new(GetKey(t), t);
        }
    }
}