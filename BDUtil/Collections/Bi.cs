using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BDUtil
{
    namespace Bi
    {
        public interface IReadOnlyMap<K, V> : IReadOnlyDictionary<K, V>
        {
            IReadOnlyDictionary<V, IReadOnlyCollection<K>> Reverse { get; }
        }
        public interface IMap<K, V> : IDictionary<K, V>
        {
            IReadOnlyDictionary<V, IReadOnlyCollection<K>> Reverse { get; }
        }
    }

    namespace Raw.Bi
    {
        public class Map<K, V, KColl> : BDUtil.Bi.IMap<K, V>, BDUtil.Bi.IReadOnlyMap<K, V>
        where KColl : ICollection<K>, IReadOnlyCollection<K>, new()
        {
            readonly Dictionary<K, V> _Forward = new();
            readonly Dictionary<V, KColl> _Reverse = new();
            public KVP.Upcast<V, KColl, IReadOnlyCollection<K>> Reverse;
            public Map() => Reverse = new(_Reverse);
            IReadOnlyDictionary<V, IReadOnlyCollection<K>> BDUtil.Bi.IReadOnlyMap<K, V>.Reverse => Reverse;
            IReadOnlyDictionary<V, IReadOnlyCollection<K>> BDUtil.Bi.IMap<K, V>.Reverse => Reverse;
            public V this[K key]
            {
                get => _Forward[key];
                set
                {
                    if (_Forward.TryGetValue(key, out V old)) _Reverse.Remove(old, key).OrThrow("{0}={1} missing <-state while set {2}", key, old, value);
                    _Forward[key] = value;
                    _Reverse.Add(value, key);
                }
            }

            public ICollection<K> Keys => _Forward.Keys;
            IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => Keys;
            public ICollection<V> Values => _Reverse.Keys;
            IEnumerable<V> IReadOnlyDictionary<K, V>.Values => Values;

            public int Count => _Forward.Count;

            public void Add(K key, V value)
            {
                _Forward.Add(key, value);  // Throws if already had a value...
                _Reverse.Add(value, key);
            }
            public void Add(KeyValuePair<K, V> item) => Add(item.Key, item.Value);

            public void Clear()
            {
                _Forward.Clear();
                _Reverse.Clear();
            }

            public bool ContainsKey(K key) => _Forward.ContainsKey(key);
            public bool Contains(KeyValuePair<K, V> item) => _Forward.Contains(item);

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
            bool ICollection<KeyValuePair<K, V>>.IsReadOnly => ((ICollection<KeyValuePair<K, V>>)_Forward).IsReadOnly;
            void ICollection<KeyValuePair<K, V>>.CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) => _Forward.WriteTo(array, arrayIndex);
        }
        public class Map<K, V> : Map<K, V, HashSet<K>> { }
    }
}