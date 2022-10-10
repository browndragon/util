using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil.Fluent;

namespace BDUtil
{
    /// An empty container for T.
    public class None<T> : ICollection<T>, IReadOnlyCollection<T>, IReadOnlyList<T>, IEnumerator<T>, ICollection, IList
    {
        public static None<T> Default = new();
        private None() { }
        int IReadOnlyCollection<T>.Count => 0;
        int ICollection<T>.Count => 0;
        bool ICollection<T>.IsReadOnly => true;
        T IEnumerator<T>.Current => default;
        object IEnumerator.Current => default;

        int ICollection.Count => 0;
        object ICollection.SyncRoot => null;
        bool ICollection.IsSynchronized => false;
        bool IList.IsReadOnly => true;
        bool IList.IsFixedSize => true;

        public bool HasValue => false;

        T IReadOnlyList<T>.this[int index] => throw new IndexOutOfRangeException($"{index} <> [0, 0)");
        object IList.this[int index]
        {
            get => throw new IndexOutOfRangeException($"{index} <> [0, 0)");
            set => throw new NotImplementedException();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        bool IEnumerator.MoveNext() => false;
        void IEnumerator.Reset() { }
        bool ICollection<T>.Contains(T item) => false;
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) { }
        void ICollection<T>.Add(T item) => throw new NotImplementedException();
        void ICollection<T>.Clear() => throw new NotImplementedException();
        bool ICollection<T>.Remove(T item) => throw new NotImplementedException();
        void IDisposable.Dispose() { }
        void ICollection.CopyTo(Array array, int index) { }
        int IList.Add(object value) => throw new NotImplementedException();
        bool IList.Contains(object value) => false;
        void IList.Clear() => throw new NotImplementedException();
        int IList.IndexOf(object value) => -1;
        void IList.Insert(int index, object value) => throw new NotImplementedException();
        void IList.Remove(object value) => throw new NotImplementedException();
        void IList.RemoveAt(int index) => throw new NotImplementedException();
    }
    /// An empty container for Key/Value pairs.
    public class None<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>, IEnumerator<KeyValuePair<TKey, TValue>>, IDictionary<TKey, TValue>, IDictionary, IDictionaryEnumerator
    {
        public static None<TKey, TValue> Default = new();
        private None() { }
        KeyValuePair<TKey, TValue> IEnumerator<KeyValuePair<TKey, TValue>>.Current => default;
        object IEnumerator.Current => default;
        object IDictionaryEnumerator.Key => default;
        object IDictionaryEnumerator.Value => default;
        DictionaryEntry IDictionaryEnumerator.Entry => default;
        ICollection IDictionary.Keys => None<TKey>.Default;
        ICollection IDictionary.Values => None<TValue>.Default;
        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => None<TKey>.Default;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => None<TValue>.Default;
        ICollection<TKey> IDictionary<TKey, TValue>.Keys => None<TKey>.Default;
        ICollection<TValue> IDictionary<TKey, TValue>.Values => None<TValue>.Default;
        bool IDictionary.IsReadOnly => true;
        bool ICollection<KeyValuePair<TKey, TValue>>.IsReadOnly => true;
        bool IDictionary.IsFixedSize => true;
        int IReadOnlyCollection<KeyValuePair<TKey, TValue>>.Count => 0;
        int ICollection<KeyValuePair<TKey, TValue>>.Count => 0;
        int ICollection.Count => 0;
        object ICollection.SyncRoot => null;
        bool ICollection.IsSynchronized => false;

        TValue IDictionary<TKey, TValue>.this[TKey key]
        {
            get => throw new IndexOutOfRangeException($"Empty dict");
            set => throw new NotImplementedException();
        }
        object IDictionary.this[object key]
        {
            get => throw new IndexOutOfRangeException($"Empty dict");
            set => throw new NotImplementedException();
        }
        TValue IReadOnlyDictionary<TKey, TValue>.this[TKey key]
        => throw new IndexOutOfRangeException($"Empty dict");

        IEnumerator<KeyValuePair<TKey, TValue>> IEnumerable<KeyValuePair<TKey, TValue>>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
        bool IEnumerator.MoveNext() => false;
        void IEnumerator.Reset() { }
        bool ICollection<KeyValuePair<TKey, TValue>>.Contains(KeyValuePair<TKey, TValue> item) => false;
        void ICollection<KeyValuePair<TKey, TValue>>.CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) { }
        void ICollection<KeyValuePair<TKey, TValue>>.Add(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();
        void ICollection<KeyValuePair<TKey, TValue>>.Clear() => throw new NotImplementedException();
        bool ICollection<KeyValuePair<TKey, TValue>>.Remove(KeyValuePair<TKey, TValue> item) => throw new NotImplementedException();
        void IDisposable.Dispose() { }

        bool IDictionary.Contains(object key) => false;
        void IDictionary.Add(object key, object value) => throw new NotImplementedException();
        void IDictionary.Clear() => throw new NotImplementedException();
        IDictionaryEnumerator IDictionary.GetEnumerator() => this;
        void IDictionary.Remove(object key) => throw new NotImplementedException();
        void ICollection.CopyTo(Array array, int index) { }
        bool IReadOnlyDictionary<TKey, TValue>.ContainsKey(TKey key) => false;
        bool IReadOnlyDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => false.Let(value = default);
        bool IDictionary<TKey, TValue>.ContainsKey(TKey key) => false;
        bool IDictionary<TKey, TValue>.TryGetValue(TKey key, out TValue value) => false.Let(value = default);
        void IDictionary<TKey, TValue>.Add(TKey key, TValue value) => throw new NotImplementedException();
        bool IDictionary<TKey, TValue>.Remove(TKey key) => false;
    }
}