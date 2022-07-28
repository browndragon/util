using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil;

namespace BDUtil
{
    [Serializable]
    public class RingBuffer<T> : ICollection<T>
    {
        readonly T[] Data;
        int Head;  // Points at first readable space (can be -1)
        public int Count { get; private set; }
        public int Capacity => Data.Length;
        public bool IsFull => Count == Capacity;

        bool ICollection<T>.IsReadOnly => false;
        public RingBuffer(int size)
        {
            Data = new T[size];
            Head = 0;
            Count = 0;
        }
        int Internalize(int i)
        {
            if (i < 0) i = Count + i;
            if (i >= Count) return -1;
            return (i + Head) % Data.Length;
        }

        public T this[int i]
        {
            get => Data[Internalize(i)];
            set => Data[Internalize(i)] = value;
        }
        public bool TryGetValue(int i, out T t)
        {
            i = Internalize(i);
            if (i < 0)
            {
                t = default;
                return false;
            }
            t = Data[i];
            return true;
        }
        public T GetHeadOrDefault(T @default = default) => TryGetValue(0, out T t) ? t : @default;
        public T GetTailOrDefault(T @default = default) => TryGetValue(-1, out T t) ? t : @default;
        public void Add(T t) => PushBack(t);
        public int Find(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; ++i)
            {
                if (predicate(Data[Internalize(i)])) return i;
            }
            return -1;
        }
        public int Find(T t, IEqualityComparer<T> comparer = default)
        {
            comparer ??= EqualityComparer<T>.Default;
            return Find(e => comparer.Equals(t, e));
        }
        public bool Contains(T t) => Find(t) > 0;
        bool ICollection<T>.Remove(T t)
        {
            int index = Find(t);
            if (index < 0) return false;
            return RemoveIndex(index);
        }
        public bool RemoveIndex(int index)
        {
            if (index < 0) index += Count;
            if (Count == 0) return false;
            for (int i = Count - 2; i >= index; --i)
            {
                Data[Internalize(i)] = Data[Internalize(i + 1)];
            }
            Data[Internalize(Count - 1)] = default;
            if (--Count == 0) Head = 0;
            return true;
        }
        public void Clear()
        {
            for (int i = 0; i < Count; ++i)
            {
                Data[Internalize(i)] = default;
            }
            Head = 0;
            Count = 0;
        }

        public void PushBack(T t)
        {
            int tail = (Head + Count) % Data.Length;
            Data[tail] = t;
            if (++Count > Data.Length)
            {
                Count = Data.Length;
                Head = (Head + 1) % Data.Length;
            }
        }
        public void PushFront(T t)
        {
            if (Head == 0) Head = Data.Length;
            Data[--Head] = t;
            if (++Count > Data.Length) Count = Data.Length;
        }
        public bool TryPopFront(out T t)
        {
            if (Count <= 0) { t = default; return false; }
            t = Data[Head];
            if (--Count <= 0) Head = 0;
            else Head = (Head + 1) % Data.Length;
            return true;
        }
        public bool TryPopBack(out T t)
        {
            if (Count <= 0) { t = default; return false; }
            t = Data[Internalize(-1)];
            if (--Count <= 0) Head = 0;
            return true;
        }

        public IEnumerator<T> GetEnumerator()
        {
            for (int i = 0; i < Count; ++i)
            {
                yield return Data[Internalize(i)];
            }
        }

        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => Arrays.CopyTo(this, array, arrayIndex);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}