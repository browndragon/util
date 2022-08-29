using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil
{
    /// Map from U<->T with compiletime fixed key sets.
    [Serializable]
    public class EnumArray<TKey, TValue> : IReadOnlyDictionary<TKey, TValue>
    where TKey : Enum
    {
        // This would be readonly, etc -- but unity makes that frustrating from otherwise C#able code.
        // So! Pretend that this is a hidden readonly member please.
        public TValue[] Data = new TValue[Enums<TKey>.Span];

        public TValue this[TKey key]
        {
            get => Data[Enums<TKey>.GetOffset(key)];
            set => Data[Enums<TKey>.GetOffset(key)] = value;
        }

        public ICollection<TKey> Keys => new ReadOnly<TKey>(Enums<TKey>.Entries);
        public ICollection<TValue> Values => new ReadOnly<TValue>(Data);
        public int Count => Data.Length;

        IEnumerable<TKey> IReadOnlyDictionary<TKey, TValue>.Keys => Enums<TKey>.Entries;
        IEnumerable<TValue> IReadOnlyDictionary<TKey, TValue>.Values => Data;

        public void Add(TKey key, TValue value) => this[key] = value;
        public void Add(KeyValuePair<TKey, TValue> item) => Add(item.Key, item.Value);
        public void Clear() { for (int i = 0; i < Data.Length; ++i) Data[i] = default; }
        public bool Contains(KeyValuePair<TKey, TValue> item) => EqualityComparer<TValue>.Default.Equals(this[item.Key], item.Value);
        public bool ContainsKey(TKey key) => Enums<TKey>.HasValue(key);
        public void CopyTo(KeyValuePair<TKey, TValue>[] array, int arrayIndex) => Iter.WriteTo(this, array, arrayIndex);
        public IEnumerator<KeyValuePair<TKey, TValue>> GetEnumerator() { foreach (TKey u in Enums<TKey>.Entries) yield return new(u, this[u]); }
        public bool RemoveKey(TKey key) { Add(key, default); return true; }
        public bool RemoveKey(TKey key, out TValue value)
        {
            int index = Enums<TKey>.GetOffset(key);
            value = Data[index];
            Data[index] = default;
            return true;
        }
        public bool TryGetValue(TKey key, out TValue value)
        {
            int index = Enums<TKey>.GetOffset(key);
            value = Data[index];
            return true;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}