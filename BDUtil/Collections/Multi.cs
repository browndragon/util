using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BDUtil
{
    namespace Multi
    {
        public interface IReadOnlySet<T> : IReadOnlyDictionary<T, int> { }
        public interface ISet<T> : IDictionary<T, int> { }

        public interface IReadOnlyMap<TKey, TValue> :
            IReadOnlyCollection<KeyValuePair<TKey, TValue>>,
            ILookup<TKey, TValue>
        {
            IReadOnlyCollection<KeyValuePair<TKey, TValue>> AsKVPs { get; }
            ILookup<TKey, TValue> AsLookup { get; }

            /// Repeated operations...
            new IReadOnlyCollection<TValue> this[TKey key] { get; }
            ICollection<TKey> Keys { get; }
            bool TryGetValue(TKey key, out IReadOnlyCollection<TValue> value);
            bool Contains(TKey key, TValue value);
            bool ContainsKey(TKey key);
        }
        public interface IMap<TKey, TValue> : ICollection<KeyValuePair<TKey, TValue>>
        {
            bool TryAdd(TKey key, TValue value);
            IReadOnlyCollection<TValue> RemoveKey(TKey key);
            bool Remove(TKey key, TValue value);

            /// Repeated operations...
            IReadOnlyCollection<KeyValuePair<TKey, TValue>> AsKVPs { get; }
            ILookup<TKey, TValue> AsLookup { get; }
            IReadOnlyCollection<TValue> this[TKey key] { get; }
            ICollection<TKey> Keys { get; }
            bool TryGetValue(TKey key, out IReadOnlyCollection<TValue> value);
            bool Contains(TKey key, TValue value);
            bool ContainsKey(TKey key);
        }
    }
    namespace Raw.Multi
    {
        [Serializable]
        public class Set<T> : BDUtil.Multi.IReadOnlySet<T>, BDUtil.Multi.ISet<T>
        {
            readonly IDictionary<T, int> Index;
            public int Count => Index.Count;
            public int Total { get; private set; } = 0;
            public ICollection<T> Keys => Index.Keys;
            IEnumerable<T> IReadOnlyDictionary<T, int>.Keys => Keys;
            ICollection<int> IDictionary<T, int>.Values => Index.Values;
            IEnumerable<int> IReadOnlyDictionary<T, int>.Values => Index.Values;
            bool ICollection<KeyValuePair<T, int>>.IsReadOnly => false;

            public int this[T key]
            {
                get => Index.TryGetValue(key, out int had) ? had : 0;
                set => Reset(key, value);
            }

            public Set() => Index = MakeNewIndex();
            protected virtual IDictionary<T, int> MakeNewIndex() => new Dictionary<T, int>();

            public bool ContainsKey(T key) => Index.ContainsKey(key);
            public bool TryGetValue(T key, out int value) => Index.TryGetValue(key, out value);
            public int Reset(T key, int value = 0)
            {
                int had = this[key];
                if (value == 0) Index.Remove(key);
                else Index[key] = value;
                Total += value - had;
                return had;
            }
            public int Add(T key, int value = 1) => Reset(key, this[key] + value);
            void IDictionary<T, int>.Add(T key, int value) => Add(key, value);
            void ICollection<KeyValuePair<T, int>>.Add(KeyValuePair<T, int> item) => Add(item.Key, item.Value);

            public int RemoveKey(T key)
            {
                if (!Index.TryGetValue(key, out int had)) return 0;
                Index.Remove(key);
                Total -= had;
                return had;
            }
            public int Remove(T key, int value = 1) => Add(key, -value);
            bool IDictionary<T, int>.Remove(T key) => RemoveKey(key) != 0;
            bool ICollection<KeyValuePair<T, int>>.Remove(KeyValuePair<T, int> item) => Remove(item.Key, item.Value) != 0;
            public void Clear() { Index.Clear(); Total = 0; }
            bool ICollection<KeyValuePair<T, int>>.Contains(KeyValuePair<T, int> item) => Index.Contains(item);
            void ICollection<KeyValuePair<T, int>>.CopyTo(KeyValuePair<T, int>[] array, int arrayIndex) => Index.CopyTo(array, arrayIndex);
            public IEnumerator<KeyValuePair<T, int>> GetEnumerator() => Index.GetEnumerator();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }

        /// A 1:N Key/Value store.
        /// The value sets are stored by key (so the iteration order is key and value set impl dependent).
        [Serializable]
        public class Map<TKey, TValue, TIndex, TVSet> :
            BDUtil.Multi.IReadOnlyMap<TKey, TValue>,
            BDUtil.Multi.IMap<TKey, TValue>
        where TIndex : IDictionary<TKey, TVSet>, new()
        where TVSet : ICollection<TValue>, IReadOnlyCollection<TValue>, new()
        {
            protected virtual TIndex Index { get; set; }
            public int Count { get; private set; } = 0;
            public ICollection<TKey> Keys => Index.Keys;
            public IReadOnlyCollection<TValue> this[TKey key] => TryGetValue(key, out var had) ? had : None<TValue>.Default;

            public Map() => Index = new();
            /// Does the actual returned set allow duplicates?
            protected virtual bool SetAllowsDupes => false;

            public IReadOnlyCollection<KeyValuePair<TKey, TValue>> AsKVPs => this;
            public ILookup<TKey, TValue> AsLookup => this;


            public bool TryAdd(TKey key, TValue value)
            {
                if (!Index.TryGetValue(key, out var has)) Index[key] = has = new();
                ICollection<TValue> had = (ICollection<TValue>)has;
                if (!SetAllowsDupes && had.Contains(value)) return false;
                had.Add(value);
                Count += 1;
                return true;
            }
            public bool TryAdd(KeyValuePair<TKey, TValue> kvp) => TryAdd(kvp.Key, kvp.Value);
            public void Add(TKey key, TValue value) => TryAdd(key, value).OrThrow();
            public void Add(KeyValuePair<TKey, TValue> kvp) => TryAdd(kvp).OrThrow();
            public bool TryGetValue(TKey key, out IReadOnlyCollection<TValue> had)
            => Index.TryGetValue(key, out var has).Let(had = has);
            public bool ContainsKey(TKey key) => Index.ContainsKey(key);
            public bool Contains(TKey key, TValue value)
            => Index.TryGetValue(key, out var had) && had is ICollection<TValue> has && has.Contains(value);
            public bool Contains(KeyValuePair<TKey, TValue> kvp) => Contains(kvp.Key, kvp.Value);
            public IReadOnlyCollection<TValue> RemoveKey(TKey key)
            {
                if (!Index.TryGetValue(key, out var had)) return None<TValue>.Default;
                Index.Remove(key).OrThrow();
                Count -= ((IReadOnlyCollection<TValue>)had).Count;
                return had;
            }
            IReadOnlyCollection<TValue> BDUtil.Multi.IMap<TKey, TValue>.RemoveKey(TKey key) => RemoveKey(key);
            public bool Remove(KeyValuePair<TKey, TValue> item) => Remove(item.Key, item.Value);
            public bool Remove(TKey key, TValue value)
            {
                if (!Index.TryGetValue(key, out var had)) return false;
                ICollection<TValue> has = (ICollection<TValue>)had;
                if (!has.Remove(value)) return false;
                if (has.IsEmpty()) Index.Remove(key).OrThrow();
                Count--;
                return true;
            }
            public void Clear() { Index.Clear(); Count = 0; }
            public IEnumerator<KVP.Grouping<TKey, TValue>> GetEnumerator()
            { foreach (TKey key in Index.Keys) yield return new(key, Index[key]); }

            IEnumerator<IGrouping<TKey, TValue>> IEnumerable<IGrouping<TKey, TValue>>.GetEnumerator()
            => (IEnumerator<IGrouping<TKey, TValue>>)GetEnumerator();

            IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator()
            {
                foreach (
                    (TKey key, IReadOnlyCollection<TValue> values) in this
                ) foreach (
                    TValue value in values
                ) yield return new(key, value);
            }
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable<KeyValuePair<TKey, TValue>>)this).GetEnumerator();
            bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => false;
            IEnumerable<TValue> ILookup<TKey, TValue>.this[TKey key] => this[key];
            bool ILookup<TKey, TValue>.Contains(TKey key) => ContainsKey(key);
            void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => Add(item);

            bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => Contains(item);

            void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex)
            => Iter.WriteTo(this, array, arrayIndex);

            bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => Remove(item);
        }

        /// Generic hash/hash multimap
        public class Map<TKey, TValue> : Map<TKey, TValue, Dictionary<TKey, HashSet<TValue>>, HashSet<TValue>> { }

        /// Hash/Ordered multimap.
        public class MapSorted<TKey, TValue> : Map<TKey, TValue, Dictionary<TKey, SortedSet<TValue>>, SortedSet<TValue>>
        where TValue : IComparable<TValue>
        { }
        /// Hash/List multimap.
        public class MapList<TKey, TValue> : Map<TKey, TValue, Dictionary<TKey, List<TValue>>, List<TValue>>
        {
            public new IReadOnlyList<TValue> this[TKey key] => (IReadOnlyList<TValue>)base[key];
            public bool TryGetValue(TKey key, out IReadOnlyList<TValue> value)
            => base.TryGetValue(key, out var had).Let(value = (IReadOnlyList<TValue>)had);
            public new IReadOnlyList<TValue> RemoveKey(TKey key) => (IReadOnlyList<TValue>)base.RemoveKey(key);
        }
    }
}