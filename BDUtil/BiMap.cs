using System.Collections.Generic;

namespace BDUtil
{
    public class BiMap<K, V> : Map<K, V>, IReadOnlyBiMap<K, V>
    {
        readonly MultiMap<V, K> _Reverse = new();

        public IReadOnlyDictionary<K, V> Forward => this;
        public IReadOnlyMultiMap<V, K> Reverse => _Reverse;

        protected override void RemoveEntry(Entry entry)
        {
            base.RemoveEntry(entry);
            if (!entry.HasValue) return;
            _Reverse.Remove(new(entry.Value.Value, entry.Value.Key));
        }
        protected override Entry TryAddEntry(KeyValuePair<K, V> item)
        {
            var entry = base.TryAddEntry(item);
            if (!entry.HasValue) return entry;
            _Reverse.Add(new(entry.Value.Value, entry.Value.Key));
            return entry;
        }
    }
}