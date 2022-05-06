using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Runtime.CompilerServices;

namespace BDUtil
{
    /// Map from U<->T
    [Serializable]
    public class EnumArray<U, T> : IDictionary<U, T>, IReadOnlyDictionary<U, T>, ITryGetValue<U, T>, IRemoveKey<U, T>
    where U : Enum
    {
        public readonly T[] Data = new T[EnumData<U>.Span];

        public T this[U key]
        {
            get => Data[EnumData<U>.GetOffset(key)];
            set => Data[EnumData<U>.GetOffset(key)] = value;
        }

        public ICollection<U> Keys => EnumData<U>.Entries.AsLegacy();
        public ICollection<T> Values => Data.AsLegacy();
        public int Count => Data.Length;
        bool ICollection<KeyValuePair<U, T>>.IsReadOnly => false;

        IEnumerable<U> IReadOnlyDictionary<U, T>.Keys => EnumData<U>.Entries;
        IEnumerable<T> IReadOnlyDictionary<U, T>.Values => Data;

        public void Add(U key, T value) => this[key] = value;
        public void Add(KeyValuePair<U, T> item) => Add(item.Key, item.Value);
        public void Clear() { for (int i = 0; i < Data.Length; ++i) Data[i] = default; }
        public bool Contains(KeyValuePair<U, T> item) => this[item.Key].EqualsT(item.Value);
        public bool ContainsKey(U key) => EnumData<U>.HasValue(key);
        public void CopyTo(KeyValuePair<U, T>[] array, int arrayIndex) => Arrays.CopyTo(this, array, arrayIndex);
        public IEnumerator<KeyValuePair<U, T>> GetEnumerator() { foreach (U u in EnumData<U>.Entries) yield return new(u, this[u]); }
        public bool Remove(U key) { Add(key, default); return true; }
        public bool Remove(KeyValuePair<U, T> item)
        {
            int index = EnumData<U>.GetOffset(item.Key);
            T was = Data[index];
            if (!was.EqualsT(item.Value)) return false;
            Data[index] = default;
            return true;
        }
        public bool RemoveKey(U key, out T value)
        {
            int index = EnumData<U>.GetOffset(key);
            value = Data[index];
            Data[index] = default;
            return true;
        }
        public bool TryGetValue(U key, out T value)
        {
            int index = EnumData<U>.GetOffset(key);
            value = Data[index];
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}