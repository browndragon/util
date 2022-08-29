using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BDUtil
{
    namespace BiMulti
    {
        public interface IReadOnlyMap<K, V> : Multi.IReadOnlyMap<K, V> { }
        public interface IMap<K, V> : Multi.IMap<K, V>
        {
            IReadOnlyCollection<K> RemoveValue(V v);
        }
    }
    namespace Raw.BiMulti
    {
        public class Map<K, V> : BDUtil.BiMulti.IReadOnlyMap<K, V>, BDUtil.BiMulti.IMap<K, V>
        {
            readonly BDUtil.Multi.IMap<K, V> _Forward;
            readonly BDUtil.Multi.IMap<V, K> _Reverse;
            public Map() { _Forward = MakeForward(); _Reverse = MakeReverse(); }
            protected virtual BDUtil.Multi.IMap<K, V> MakeForward() => new Multi.Map<K, V>();
            protected virtual BDUtil.Multi.IMap<V, K> MakeReverse() => new Multi.Map<V, K>();

            public IReadOnlyCollection<V> this[K key] => _Forward[key];
            IEnumerable<V> ILookup<K, V>.this[K key] => this[key];
            public int Count => _Forward.Count;
            public ICollection<K> Keys => _Forward.Keys;
            public BDUtil.Multi.IReadOnlyMap<V, K> Reverse => (BDUtil.Multi.IReadOnlyMap<V, K>)_Reverse;
            public IReadOnlyCollection<KeyValuePair<K, V>> AsKVPs => this;
            public ILookup<K, V> AsLookup => this;

            public bool TryAdd(K key, V value)
            => _Reverse.TryAdd(value, key)
            && _Forward.TryAdd(key, value).OrThrow("{0}={1} invalid state", key, value);
            public bool TryAdd(KeyValuePair<K, V> item) => TryAdd(item.Key, item.Value);
            public void Add(K key, V value) => TryAdd(key, value).OrThrow();
            public void Add(KeyValuePair<K, V> item) => TryAdd(item).OrThrow();

            public IReadOnlyCollection<V> RemoveKey(K key)
            {
                IReadOnlyCollection<V> removed = _Forward.RemoveKey(key);
                foreach (V v in removed) _Reverse.Remove(v, key).OrThrow("{0}={1} invalid <-state", key, v);
                return removed;
            }
            public IReadOnlyCollection<K> RemoveValue(V value)
            {
                IReadOnlyCollection<K> removed = _Reverse.RemoveKey(value);
                foreach (K k in removed) _Forward.Remove(k, value).OrThrow("{0}={1} invalid ->state", k, value);
                return removed;
            }

            public bool Remove(K key, V value)
            => _Forward.Remove(key, value)
            && _Reverse.Remove(value, key).OrThrow("{0}={1} invalid <-state", key, value);

            public bool TryGetValue(K key, out IReadOnlyCollection<V> value) => _Forward.TryGetValue(key, out value);

            public bool Contains(K key, V value) => _Forward.Contains(key, value);
            public bool Contains(KeyValuePair<K, V> item) => _Forward.Contains(item);
            bool ILookup<K, V>.Contains(K key) => ContainsKey(key);
            public bool ContainsKey(K key) => _Forward.ContainsKey(key);

            public void Clear()
            {
                _Forward.Clear();
                _Reverse.Clear();
            }

            public bool Remove(KeyValuePair<K, V> item)
            => _Forward.Remove(item)
            && _Reverse.Remove(item.Reverse()).OrThrow("{0} missing <-state", item);

            public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => _Forward.GetEnumerator();
            IEnumerator<IGrouping<K, V>> IEnumerable<IGrouping<K, V>>.GetEnumerator()
            => ((IEnumerable<IGrouping<K, V>>)_Forward).GetEnumerator();
            public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) => _Forward.CopyTo(array, arrayIndex);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            bool ICollection<KeyValuePair<K, V>>.IsReadOnly => _Forward.IsReadOnly;
        }
    }
}
