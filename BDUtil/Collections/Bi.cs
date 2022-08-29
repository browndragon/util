using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BDUtil
{
    namespace Bi
    {
        public interface IReadOnlyMap<K, V> : IReadOnlyDictionary<K, V>
        {
            Multi.IReadOnlyMap<V, K> Reverse { get; }
        }
        public interface IMap<K, V> : IDictionary<K, V>
        {
            Multi.IReadOnlyMap<V, K> Reverse { get; }
        }
    }

    namespace Raw.Bi
    {
        public class Map<K, V> : BDUtil.Bi.IMap<K, V>, BDUtil.Bi.IReadOnlyMap<K, V>
        {
            readonly IDictionary<K, V> _Forward;
            readonly BDUtil.Multi.IMap<V, K> _Reverse;
            public Map() { _Forward = MakeForward(); _Reverse = MakeReverse(); }
            protected virtual IDictionary<K, V> MakeForward() => new Dictionary<K, V>();
            protected virtual BDUtil.Multi.IMap<V, K> MakeReverse() => new Multi.Map<V, K>();

            public BDUtil.Multi.IReadOnlyMap<V, K> Reverse => (BDUtil.Multi.IReadOnlyMap<V, K>)_Reverse;
            public V this[K key]
            {
                get => _Forward[key];
                set
                {
                    if (_Forward.TryGetValue(key, out V old)) _Reverse.Remove(old, key).OrThrow("{0}={1} missing <-state while set {2}", key, old, value);
                    _Forward[key] = value;
                    _Reverse.TryAdd(value, key).OrThrow();
                }
            }

            public ICollection<K> Keys => _Forward.Keys;
            IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => Keys;
            public ICollection<V> Values => _Reverse.Keys;
            IEnumerable<V> IReadOnlyDictionary<K, V>.Values => Values;

            public int Count => _Forward.Count;

            public bool TryAdd(K key, V value)
            {
                if (!_Reverse.TryAdd(value, key)) return false;
                _Forward.Add(key, value);
                return true;
            }
            public bool TryAdd(KeyValuePair<K, V> kvp) => TryAdd(kvp.Key, kvp.Value);
            public void Add(K key, V value) => TryAdd(key, value).OrThrow("{0}={1} collision", key, value);
            public void Add(KeyValuePair<K, V> item) => TryAdd(item).OrThrow("{0} collision", item);

            public void Clear()
            {
                _Forward.Clear();
                _Reverse.Clear();
            }

            public bool ContainsKey(K key) => _Forward.ContainsKey(key);
            public bool Contains(KeyValuePair<K, V> item) => _Forward.Contains(item);

            public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex)
            { _Forward.CopyTo(array, arrayIndex); }

            public bool Remove(K key)
            {
                if (!_Forward.TryGetValue(key, out V value)) return false;
                _Forward.Remove(key).OrThrow("Error removing ->{0}", key);
                _Reverse.Remove(value, key).OrThrow("Error removing <-{0}={1}", key, value);
                return true;
            }

            public bool Remove(KeyValuePair<K, V> item)
            => _Reverse.Remove(item.Reverse())
            && _Forward.Remove(item.Key).OrThrow("Error removing ->{0}", item);

            public bool TryGetValue(K key, out V value) => _Forward.TryGetValue(key, out value);

            public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => _Forward.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            bool ICollection<KeyValuePair<K, V>>.IsReadOnly => _Forward.IsReadOnly;
        }
    }
}