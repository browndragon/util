using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil
{
    /// A legacy, empty collection.
    public readonly struct None : ICollection, IList, IEnumerator
    {
        public static readonly None Default = default;
        public object this[int index]
        {
            get => throw new IndexOutOfRangeException();
            set => throw new NotImplementedException();
        }
        public int Count => 0;
        public object Current => default;
        public bool IsFixedSize => true;
        public bool IsReadOnly => true;
        public bool IsSynchronized => false;
        public object SyncRoot => null;
        public bool MoveNext() => false;
        public void Reset() { }
        public IEnumerator GetEnumerator() => this;
        public int Add(object value) => throw new NotImplementedException();
        public void Clear() { }
        public bool Contains(object value) => false;
        public int IndexOf(object value) => -1;
        public void Insert(int index, object value) => throw new NotImplementedException();
        public void Remove(object value) => throw new NotImplementedException();
        public void RemoveAt(int index) => throw new NotImplementedException();
        public void CopyTo(Array array, int index) { }
    }
    /// A generic empty collection.
    public readonly struct None<T> : ICollection<T>, IEnumerator<T>, Raw.IContainer<T>, IReadOnlyList<T>, IList<T>
    {
        public static readonly None<T> Default = default;
        public T this[int index] => throw new IndexOutOfRangeException();
        T IList<T>.this[int index]
        {
            get => throw new IndexOutOfRangeException();
            set => throw new IndexOutOfRangeException();
        }
        public int Count => 0;
        public T Current => default;
        object IEnumerator.Current => Current;
        public bool IsReadOnly => false;

        public bool TryAdd(T _) => false;
        public void Clear() { }
        public bool Contains(T _) => false;
        public IEnumerator<T> GetEnumerator() => this;
        public int IndexOf(T item) => -1;
        public void Insert(int index, T item) => throw new NotImplementedException();
        public bool MoveNext() => false;
        public bool Remove(T item) => false;
        public void RemoveAt(int index) => throw new NotImplementedException();
        public void Reset() { }
        public void Add(T item) => throw new NotImplementedException();
        public void CopyTo(T[] array, int arrayIndex) { }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        void IDisposable.Dispose() { }

        public static implicit operator None(None<T> _) => default;
        public static implicit operator None<T>(None _) => default;
    }
    /// A generic empty map collection.
    public readonly struct None<K, V> : IDictionary<K, V>, IReadOnlyDictionary<K, V>, IEnumerator<KeyValuePair<K, V>>
    {
        public static readonly None<K, V> Default = default;
        public V this[K key]
        {
            get => throw new IndexOutOfRangeException();
            set => throw new NotImplementedException();
        }

        public int Count => 0;
        public KeyValuePair<K, V> Current => default;
        public bool IsReadOnly => false;
        object IEnumerator.Current => Current;
        public ICollection<K> Keys => None<K>.Default;
        public ICollection<V> Values => None<V>.Default;
        IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => Keys;
        IEnumerable<V> IReadOnlyDictionary<K, V>.Values => Values;

        public void Add(K key, V value) => throw new NotImplementedException();
        public void Add(KeyValuePair<K, V> item) => throw new NotImplementedException();
        public void Clear() { }
        public bool Contains(KeyValuePair<K, V> item) => false;
        public bool ContainsKey(K key) => false;
        public void CopyTo(KeyValuePair<K, V>[] array, int arrayIndex) { }

        public IEnumerator<KeyValuePair<K, V>> GetEnumerator() => this;
        public bool Remove(K key) => false;
        public bool Remove(KeyValuePair<K, V> item) => false;
        public bool TryGetValue(K _, out V value) { value = default; return false; }
        public bool MoveNext() => false;
        public void Reset() { }
        IEnumerator IEnumerable.GetEnumerator() => this;
        void IDisposable.Dispose() { }
        public bool Contains(K _) => false;

        public static implicit operator None(None<K, V> _) => default;
        public static implicit operator None<K, V>(None _) => default;
    }

    public static class Nones
    {
        public static IEnumerable<T> OrNone<T>(this IEnumerable<T> thiz) => thiz ?? None<T>.Default;
        public static IEnumerable<KeyValuePair<K, V>> OrNone<K, V>(this IEnumerable<KeyValuePair<K, V>> thiz) => thiz ?? None<K, V>.Default;
    }
}
