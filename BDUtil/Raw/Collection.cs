using System.Collections;
using System.Collections.Generic;

namespace BDUtil.Raw
{
    /// IReadOnlyCollection doesn't declare Contains, and it's very reasonable to ask this of e.g. keysets.
    public interface IContainer<T> : IReadOnlyCollection<T> { public bool Contains(T t); }

    /// The primary iteration order is as a KeyValuePair<K, V> collection,
    /// but I'm experiencing crashes when using interface default implementations.
    public interface IReadOnlyMultiMap<K, V> : IContainer<KeyValuePair<K, V>>, IReadOnlyDictionary<K, IContainer<V>>
    {
        new IContainer<V> this[K key] { get; }
    }

    /// A map which maintains a reverse-map.
    public interface IReadOnlyBiMap<K, V> : IReadOnlyDictionary<K, V>
    {
        IReadOnlyDictionary<K, V> Forward { get; }
        IReadOnlyMultiMap<V, K> Reverse { get; }
    }

    /// An abstract keyed-set-like implementation of a collection.
    /// This has O(1) add/remove/contains, and iterates based on insertion order.
    public abstract class Collection<K, T> : ICollection<T>, IContainer<T>
    {
        /// Returns the storage-unique "key" for the object.
        // This logically implies we should provide some way to do Remove(K), but no:
        // that would get complicated with e.g. multimaps.
        protected abstract K GetKey(T item);

        readonly IEqualityComparer<T> Comparer;
        readonly LinkedList<T> Elems = new();
        readonly Dictionary<K, LinkedListNode<T>> Index;
        public Collection() : this(default, default) { }
        public Collection(IEqualityComparer<K> kcomparer, IEqualityComparer<T> tcomparer)
        {
            Comparer = tcomparer ?? EqualityComparer<T>.Default;
            Index = new(kcomparer ?? EqualityComparer<K>.Default);
        }
        public int Count => Elems.Count;
        bool ICollection<T>.IsReadOnly => false;

        public bool TryAdd(T item) => TryAddEntry(item).HasValue;
        public void Add(T item) => TryAdd(item).OrThrow();
        public void AddRange(IEnumerable<T> items) { foreach (T item in items) Add(item); }
        public bool Contains(T item) => Index.TryGetValue(GetKey(item), out var node) && Comparer.Equals(item, node.Value);
        public void CopyTo(T[] array, int arrayIndex) => Elems.CopyTo(array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => Elems.GetEnumerator();
        /// Removes the item iff it's the stored K=>item entry.
        public bool Remove(T item)
        {
            K key = GetKey(item);
            var entry = GetEntry(key);
            if (!entry.HasValue) return false;
            if (!Comparer.Equals(item, entry.Value)) return false;
            RemoveEntry(entry);
            return true;
        }

        public virtual void Clear() { Elems.Clear(); Index.Clear(); }

        protected virtual void RemoveEntry(Entry entry)
        {
            if (!entry.HasValue) throw new KeyNotFoundException($"Index corruption missing {entry.Key}=>{entry.Node}");
            Elems.Remove(entry.Node);
            Index.Remove(entry.Key);
        }
        protected virtual Entry TryAddEntry(T item)
        {
            K key = GetKey(item);
            if (Index.ContainsKey(key)) return default;
            var node = Index[key] = Elems.AddLast(item);
            return new Entry(key, node);
        }

        /// Internal API for locating specific data during operations.
        /// It's backed by a LinkedListNode<T>, but it definitely doesn't *need* to be.
        protected readonly struct Entry
        {
            public Entry(K key, LinkedListNode<T> node) { Key = key; Node = node; }

            public readonly K Key;
            internal readonly LinkedListNode<T> Node;
            public bool HasValue => Node != null;
            /// NOTE! YOU are responsible for not changing the value K of T!
            /// For instance, a KeyValuePair<K, V> can have its V changed without modifying K.
            /// But for set-type Entries, this is NOT legal!
            public T Value
            {
                get => Node.Value;
                set => Node.Value = value;
            }
            public static implicit operator bool(Entry entry) => entry.HasValue;
        }
        protected Entry GetEntry(K key) => new(key, Index.TryGetValue(key, out var value) ? value : default);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}