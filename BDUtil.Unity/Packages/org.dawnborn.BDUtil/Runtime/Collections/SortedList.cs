using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil.Raw
{
    public class SortedList<T> : IReadOnlyList<T>, IList<T>
    {
        readonly List<T> Data = new();
        public bool IsDirty { get; private set; }
        public void SurgicallyRestore() => IsDirty = false;

        public bool AllowDupes { get; set; }
        IComparer<T> comparer = Comparer<T>.Default;
        public IComparer<T> Comparer
        {
            get => comparer;
            set
            {
                IsDirty |= comparer != value;
                comparer = value;
            }
        }
        public void Sort()
        {
            if (!IsDirty) return;
            Data.Sort(comparer);
            IsDirty = false;
        }

        public T this[int index]
        {
            get => Data[index];
            set
            {
                IsDirty.OrThrow();
                Data[index] = value;
            }
        }

        public int Count => Data.Count;

        public void Add(T item)
        {
            if (IsDirty) { Data.Add(item); return; }
            int index = Data.BinarySearch(item, Comparer);
            if (index < 0) index = ~index;
            else if (!AllowDupes) throw new ArgumentException($"Duplicate {item}");
            Data.Insert(index, item);
        }

        public void Clear()
        {
            Data.Clear();
            IsDirty = comparer == null;
        }

        public bool Contains(T item)
        {
            if (IsDirty) return Data.Contains(item);
            int index = Data.BinarySearch(item, Comparer);
            return index >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex) => Data.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => Data.GetEnumerator();

        public int IndexOf(T item)
        {
            if (IsDirty) return Data.IndexOf(item);
            return Data.BinarySearch(item, Comparer);
        }

        public void Insert(int index, T item)
        {
            IsDirty.OrThrow();
            Data.Insert(index, item);
        }

        public bool Remove(T item)
        {
            if (IsDirty) return Data.Remove(item);
            int index = Data.BinarySearch(item, Comparer);
            if (index < 0) return false;
            Data.RemoveAt(index);
            return true;
        }

        public void RemoveAt(int index)
        {
            IsDirty.OrThrow();
            Data.RemoveAt(index);
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        bool ICollection<T>.IsReadOnly => ((ICollection<T>)Data).IsReadOnly;
    }
}
