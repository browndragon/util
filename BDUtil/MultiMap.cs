using System.Collections.Generic;

namespace BDUtil
{
    /// A 1:N Key/Value store.
    public class MultiMap<K, V> : Collection<KeyValuePair<K, V>, KeyValuePair<K, V>>, IReadOnlyMultiMap<K, V>
    {
        protected override KeyValuePair<K, V> GetKey(KeyValuePair<K, V> item) => item;

        readonly Map<K, Set<V>> Index = new();
        public IContainer<V> this[K key] => Index.TryGetValue(key, out var set) ? set : None<V>.Default;

        public KVP.Keys<Map<K, Set<V>>, K, Set<V>> Keys => Index.Keys;
        /// Maybe this would be better as "entries" -- the point is that these damn things are assoc with keys.
        public IContainer<KeyValuePair<K, IContainer<V>>> Values
        => (IContainer<KeyValuePair<K, IContainer<V>>>)Index;

        IEnumerable<K> IReadOnlyDictionary<K, IContainer<V>>.Keys => Keys;
        IEnumerable<IContainer<V>> IReadOnlyDictionary<K, IContainer<V>>.Values => Index.Values;

        IEnumerator<KeyValuePair<K, IContainer<V>>> IEnumerable<KeyValuePair<K, IContainer<V>>>.GetEnumerator() => ((IReadOnlyDictionary<K, IContainer<V>>)Index).GetEnumerator();

        public bool ContainsKey(K key) => Index.ContainsKey(key);
        // Killme?
        public bool Contains(K key) => ContainsKey(key);
        public bool TryGetValue(K key, out IContainer<V> value)
        {
            bool had = Index.TryGetValue(key, out var set);
            value = had ? set : None<V>.Default;
            return had;
        }

        /// Pulls a magic trick: pretends all of the entries of k1 and k2 had been entered in the other.
        /// This does modify insertion iteration order of the keys.
        public void Swap(K k1, K k2)
        {
            switch (RemoveKey(k1, out var v1), RemoveKey(k2, out var v2))
            {
                case (true, true):
                    foreach (V v in v1) Add(new(k2, v));
                    foreach (V v in v2) Add(new(k1, v));
                    return;
                case (true, false):
                    foreach (V v in v1) Add(new(k2, v));
                    return;
                case (false, true):
                    foreach (V v in v2) Add(new(k1, v));
                    return;
                case (false, false): // Fallthrough
                default:
                    break;
            }
        }

        public bool RemoveKey(K key, out IContainer<V> was)
        {
            if (!Index.RemoveKey(key, out var set))
            {
                was = None<V>.Default;
                return false;
            }
            was = set;
            foreach (V v in set) Remove(new(key, v));
            return true;
        }
        public bool RemoveKey(K key) => RemoveKey(key, out var _);

        public override void Clear() { base.Clear(); Index.Clear(); }
        protected override void RemoveEntry(Entry entry)
        {
            base.RemoveEntry(entry);
            if (Index.TryGetValue(entry.Value.Key, out Set<V> values))
            {
                values.Remove(entry.Value.Value).OrThrow("Index corruption {0} missing", entry.Value);
                if (values.Count <= 0) Index.Remove(entry.Value.Key).OrThrow("Index corruption {0} missing", entry.Value);
            }
        }
        protected override Entry TryAddEntry(KeyValuePair<K, V> item)
        {
            var entry = base.TryAddEntry(item);
            if (!entry.HasValue) return entry;
            if (!Index.TryGetValue(entry.Value.Key, out Set<V> values))
            {
                values = new();
                Index.Add(item.Key, values);
            }
            values.Add(item.Value);
            return entry;
        }
    }
}