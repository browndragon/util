using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil.Raw
{
    public interface IReadOnlyOrdered<T> : IReadOnlyList<T>
    {
        /// While dirty degrades into using standard list semantics; while !IsDirty, uses binary search for its ops.
        /// Fixing dirty requires either externally fixing it and asserting that it is clean, or calling Sort.
        public Lock IsDirty { get; }
        /// Uses List BinarySearch semantics, `~i` when value absent.
        public int IndexOf(T item);
        public bool Contains(T item);
    }
    // "fun" fact; the stdlib doesn't provide Sort on IList, it's an extension!
    public interface IOrdered<T> : IList<T>, IList
    {
        /// See discussion @ IReadOnlyOrdered<T>.IsDirty.
        /// Insert is only usable if you're already dirty (so set IsDirty first!).
        Lock IsDirty { get; set; }
        bool AllowDupes { get; set; }
        IComparer<T> Comparer { get; set; }
        // Applies the current sorting scheme: Comparer & AllowDupes iff IsDirty.
        void Sort();
        // The semantics of IOrdered.Add are to BinarySearch & insert if clean; otherwise, add @ end while dirty.
        // void Add(T item);
        // The semantics of IOrdered.Remove are to BinarySearch & remove if clean; otherwise, swap with end & delete that if dirty.
        // bool Remove(T item);
        // THROWS IF YOU ARE CLEAN. Otherwise, List.Insert.
        // void Insert(int index, T item);
        // Perfectly polite, no special notes.
        // void RemoveAt(int index);
    }

    // Operates in Dirty mode: Once dirty, adds go to the end of the list, IndexOf is linear.
    // Sorting (or setting IsDirty=false) makes it clean again.
    // This isn't a System.Collections.Generic.SortedDictionary/SortedList: those use dictionary semantics, but we might sort with either dupes in values, or dupes in priorities!
    // This isn't a System.Collections.Generic.SortedSet: that can't look at elements less than/more than this element, and don't offer a re-sort functionality!
    public abstract class Ordered<TList, T> : IOrdered<T>, IReadOnlyOrdered<T>
    where TList : IList<T>, /*IReadOnlyList<T>, */ new()
    {
        protected readonly TList Data = new();
        /// Explicitly does a binary search.
        /// If IsDirty (and any modifications have been made), the retval is poop.
        /// Most users should just call IndexOf.
        protected abstract int BinarySearch(T item);
        /// Actually probably a quicksort, but the point is, makes the list sorted.
        /// This doesn't affect IsDirty; see Sort.
        protected abstract void BinarySort();

        public Lock IsDirty { get; set; } = default;
        bool allowDupes = true;
        public bool AllowDupes
        {
            get => allowDupes;
            set
            {
                IsDirty += allowDupes && !value;
                allowDupes = value;
            }
        }
        IComparer<T> comparer = Comparer<T>.Default;
        public IComparer<T> Comparer
        {
            get => comparer;
            set
            {
                IsDirty += comparer != value;
                comparer = value;
            }
        }

        public T this[int index]
        {
            get => Data[index];
            set
            {
                IsDirty.OrThrowInternal();
                Data[index] = value;
            }
        }
        public int Count => Data.Count;
        public int Add(T item)
        {
            if (IsDirty) { Data.Add(item); return -1; }
            int index = BinarySearch(item);
            if (index < 0) index = ~index;
            else if (!AllowDupes) throw new ArgumentException($"Duplicate {item}");
            Data.Insert(index, item);
            return index;
        }
        void ICollection<T>.Add(T item) => Add(item);

        public void Clear()
        {
            Data.Clear();
            IsDirty = comparer == null;
        }

        public bool Contains(T item)
        {
            if (IsDirty) return Data.Contains(item);
            int index = BinarySearch(item);
            return index >= 0;
        }

        public void CopyTo(T[] array, int arrayIndex) => Data.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => Data.GetEnumerator();

        public int IndexOf(T item)
        {
            if (IsDirty) return Data.IndexOf(item);
            int index = BinarySearch(item);
            return index;
        }

        public void Insert(int index, T item)
        {
            IsDirty.OrThrowInternal();
            Data.Insert(index, item);
        }

        public bool Remove(T item)
        {
            if (Count <= 0) return false;
            if (IsDirty)
            {
                int index = Data.IndexOf(item);
                if (index < 0) return false;
                (Data[index], Data[Data.Count - 1]) = (Data[Data.Count - 1], Data[index]);
                Data.RemoveAt(Data.Count - 1);
                return true;
            }
            else
            {
                int index = BinarySearch(item);
                if (index < 0) return false;
                Data.RemoveAt(index);
                return true;
            }
        }
        public void RemoveAt(int index) => Data.RemoveAt(index);
        public void Sort()
        {
            if (!IsDirty) return;
            BinarySort();
            IsDirty = default;
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IList.Add(object value) => value is not T t ? -1 : Add(t);
        bool IList.Contains(object value) => value is T t && Contains(t);
        int IList.IndexOf(object value) => IndexOf((T)value);
        void IList.Insert(int index, object value) => Insert(index, (T)value);
        void IList.Remove(object value) => Remove((T)value);
        void ICollection.CopyTo(Array array, int index) => Data.WriteTo(array, index);
        bool ICollection<T>.IsReadOnly => ((ICollection<T>)Data).IsReadOnly;
        bool IList.IsReadOnly => false;
        bool IList.IsFixedSize => Data switch
        {
            IList t => t.IsFixedSize,
            _ => true,
        };

        object ICollection.SyncRoot => null;
        bool ICollection.IsSynchronized => false;
        object IList.this[int index]
        {
            get => this[index];
            set => this[index] = (T)value;
        }
    }

    public class OrderedList<T> : Ordered<List<T>, T>
    {
        protected override int BinarySearch(T item) => Data.BinarySearch(item);
        protected override void BinarySort() => Data.Sort(Comparer);
    }

    public class OrderedDeque<T> : Ordered<Deque<T>, T>, IReadOnlyDeque<T>, IDeque<T>
    {
        protected override int BinarySearch(T item) => Data.BinarySearch(item);
        protected override void BinarySort() => Data.BinarySort(Comparer);

        public int Capacity => Data.Capacity;
        public bool PopBack(out T value) => Data.PopBack(out value);
        public IEnumerable<T> PopEnumerable() => Data.PopEnumerable();
        public bool PopFront(out T value) => Data.PopFront(out value);

        void IDeque<T>.PushBack(T value) => Insert(Count, value);
        void IDeque<T>.PushFront(T value) => Insert(0, value);
    }
}