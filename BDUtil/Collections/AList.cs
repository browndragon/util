using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil.Raw
{
    /// List has a lot of annoying fields. Abstractly implement!
    public abstract class AList<T> : IList<T>, IReadOnlyList<T>, IList
    {
        protected static readonly IEqualityComparer<T> Eq = EqualityComparer<T>.Default;

        /// Total data currently stored.
        public int Count { get; protected set; } = 0;
        public abstract T this[int i] { get; set; }
        object IList.this[int index] { get => this[index]; set => this[index] = (T)value; }

        /// Given no other information, you'll have to go O(n).
        public int IndexOf(Predicate<T> predicate)
        {
            for (int i = 0; i < Count; ++i) if (predicate(this[i])) return i;
            return -1;
        }
        /// This might be able to binary sort or something.
        public virtual int IndexOf(T t, IEqualityComparer<T> comparer)
        {
            comparer ??= Eq;
            for (int i = 0; i < Count; ++i) if (comparer.Equals(t, this[i])) return i;
            return -1;
        }
        public int IndexOf(T t) => IndexOf(t, null);
        public bool Contains(T t) => IndexOf(t) > 0;
        public void Add(T t) => Insert(Count, t);
        public bool Remove(T t)
        {
            int index = IndexOf(t);
            if (index < 0) return false;
            RemoveAt(index);
            return true;
        }
        public abstract void RemoveAt(int index);
        // Insert so that `this[index]==item` afterwards.
        public abstract void Insert(int index, T item);
        public abstract IEnumerator<T> GetEnumerator();

        public abstract void Clear();
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => Iter.WriteTo(this, array, arrayIndex);
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        int IList.Add(object value)
        {
            if (value is not T t) return -1;
            Add(t);
            return Count - 1;
        }
        bool IList.Contains(object value)
        {
            if (value is not T t) return false;
            return Contains(t);
        }
        int IList.IndexOf(object value)
        {
            if (value is not T t) return -1;
            return IndexOf(t);
        }
        void IList.Insert(int index, object value) => Insert(index, (T)value);
        void IList.Remove(object value)
        {
            if (value is not T t) return;
            Remove(t);
        }
        void ICollection.CopyTo(Array array, int index) => Iter.WriteTo(this, array, index);
        bool ICollection<T>.IsReadOnly => false;
        bool IList.IsReadOnly => false;
        bool IList.IsFixedSize => false;
        object ICollection.SyncRoot => null;
        bool ICollection.IsSynchronized => false;
    }
}
