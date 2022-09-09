using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil.Raw
{
    /// Represents a base type you can watch.
    public static class Observable
    {
        public interface ICollectionObserver<T>
        {
            void Add(T @new);
            void Clear();
            void Remove(T old);
        }
        public interface IObservableCollection<T> : ICollection<T>, IReadOnlyCollection<T>, ICollection
        {
            ICollectionObserver<T> Observer { get; set; }
        }
        public interface IIndexObserver<K, V, T> : ICollectionObserver<T>
        {
            void Insert(K key, V @new);
            void RemoveAt(K key, V old);
            void Set(K key, V @new, bool hadOld, V old);
        }
        public interface IObservableIndexedCollection<K, V, T> : IObservableCollection<T>
        /// Usually also :IDictionary, or :IList, or whatever.
        {
            new IIndexObserver<K, V, T> Observer { get; set; }
        }
        public abstract class Collection<T> : IObservableCollection<T>
        {
            public virtual ICollectionObserver<T> Observer { get; set; }
            public abstract int Count { get; }
            public abstract void Add(T item);
            public abstract void Clear();
            public abstract bool Contains(T item);
            public abstract IEnumerator<T> GetEnumerator();
            public abstract bool Remove(T item);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            bool ICollection<T>.IsReadOnly => false;
            object ICollection.SyncRoot => null;
            bool ICollection.IsSynchronized => false;
            public void CopyTo(T[] array, int arrayIndex) => this.WriteTo(array, arrayIndex);
            void ICollection.CopyTo(Array array, int index) => this.WriteTo(array, index);
        }
        public abstract class Collection<TColl, T> : Collection<T>
        where TColl : ICollection<T>, new()
        {
            protected readonly TColl Data = new();
            public override int Count => Data.Count;
            public override void Add(T item)
            {
                int count = Data.Count;
                Data.Add(item);
                if (count != Data.Count) Observer?.Add(item);
            }
            public override void Clear()
            {
                Data.Clear();
                Observer?.Clear();
            }
            public override bool Contains(T item) => Data.Contains(item);
            public override IEnumerator<T> GetEnumerator() => Data.GetEnumerator();
            public override bool Remove(T item)
            {
                if (!Data.Remove(item)) return false;
                Observer?.Remove(item);
                return true;
            }
        }
        public abstract class List<TColl, T> : Collection<TColl, T>, IObservableIndexedCollection<int, T, T>, IList<T>, IReadOnlyList<T>, IList
        where TColl : IList<T>, new()
        {
            public IIndexObserver<int, T, T> IndexObserver { get; set; }
            IIndexObserver<int, T, T> IObservableIndexedCollection<int, T, T>.Observer { get => IndexObserver; set => IndexObserver = value; }
            public override ICollectionObserver<T> Observer
            {
                get => IndexObserver;
                set => IndexObserver = (IIndexObserver<int, T, T>)value;
            }
            public T this[int index]
            {
                get => Data[index];
                set
                {
                    T oldValue = Data[index];
                    Data[index] = value;
                    IndexObserver?.Set(index, value, true, oldValue);
                }
            }
            object IList.this[int index] { get => this[index]; set => this[index] = (T)value; }
            bool IList.IsReadOnly => false;
            bool IList.IsFixedSize => false;

            public int IndexOf(T item) => Data.IndexOf(item);
            public void Insert(int index, T item)
            {
                Data.Insert(index, item);
                IndexObserver?.Insert(index, item);
            }
            public void RemoveAt(int index)
            {
                T item = Data[index];
                Data.RemoveAt(index);
                IndexObserver?.RemoveAt(index, item);
            }
            int IList.Add(object value)
            {
                if (value is not T t) return -1;
                Add(t);
                return Data.Count - 1;
            }

            bool IList.Contains(object value) => value is T t && Contains(t);
            int IList.IndexOf(object value) => value is T t ? IndexOf(t) : -1;
            void IList.Insert(int index, object value) => Insert(index, (T)value);
            void IList.Remove(object value) => Remove((T)value);
        }
        public abstract class Dictionary<KVColl, K, V> : Collection<KVColl, KeyValuePair<K, V>>, IObservableIndexedCollection<K, V, KeyValuePair<K, V>>, IDictionary<K, V>, IReadOnlyDictionary<K, V>, IDictionary
        where KVColl : IDictionary<K, V>, new()
        {
            public IIndexObserver<K, V, KeyValuePair<K, V>> IndexObserver { get; set; }
            IIndexObserver<K, V, KeyValuePair<K, V>> IObservableIndexedCollection<K, V, KeyValuePair<K, V>>.Observer { get => IndexObserver; set => IndexObserver = value; }
            public override ICollectionObserver<KeyValuePair<K, V>> Observer
            {
                get => IndexObserver;
                set => IndexObserver = (IIndexObserver<K, V, KeyValuePair<K, V>>)value;
            }
            public ICollection<K> Keys => Data.Keys;
            public ICollection<V> Values => Data.Values;
            IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => Keys;
            IEnumerable<V> IReadOnlyDictionary<K, V>.Values => Values;

            public V this[K key]
            {
                get => Data[key];
                set
                {
                    bool hadOld = Data.TryGetValue(key, out V old);
                    Data[key] = value;
                    IndexObserver?.Set(key, value, hadOld, old);
                }
            }
            public bool ContainsKey(K key) => Data.ContainsKey(key);
            public void Add(K key, V value)
            {
                Data.Add(key, value);
                IndexObserver?.Insert(key, value);
            }
            public bool Remove(K key)
            {
                if (!Data.TryGetValue(key, out V v)) return false;
                Data.Remove(key).OrThrowInternal();
                IndexObserver?.RemoveAt(key, v);
                return Data.Remove(key);
            }
            public bool TryGetValue(K key, out V value) => Data.TryGetValue(key, out value);

            bool IDictionary.Contains(object key) => key is K k && Data.ContainsKey(k);

            void IDictionary.Add(object key, object value) => Add((K)key, (V)value);
            IDictionaryEnumerator IDictionary.GetEnumerator() => KVP.GetDictionaryEnumerator(this);
            void IDictionary.Remove(object key) => Remove((K)key);
            ICollection IDictionary.Keys => (ICollection)(object)Keys;
            ICollection IDictionary.Values => (ICollection)(object)Values;
            bool IDictionary.IsReadOnly => false;
            bool IDictionary.IsFixedSize => false;
            object IDictionary.this[object key]
            {
                get => this[(K)key];
                set => this[(K)key] = (V)value;
            }
        }

        public class Set<T> : Collection<HashSet<T>, T> { }
        public class Deque<T> : List<Raw.Deque<T>, T> { }
        /// Will I ever use this? Probably not...
        public class List<T> : List<System.Collections.Generic.List<T>, T> { }
        public class Dictionary<K, V> : Dictionary<System.Collections.Generic.Dictionary<K, V>, K, V> { }
    }
}
