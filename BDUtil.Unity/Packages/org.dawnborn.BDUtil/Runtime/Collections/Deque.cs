using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil
{
    /// Okay, runtime they're _very_ different. But raw, they're just a list.
    /// So in the same way we expose a set-but-it's-really-a-hash-set, do the same with Deque.
    [Serializable]
    public sealed class Deque<T> : Store<Raw.Deque<T>, T, T>, IList<T>, IReadOnlyList<T>, IList
    {
        protected override T Internal(T t) => t;
        protected override T External(T t) => t;
        public T this[int index] { get => AsCollection[index]; set => AsCollection[index] = value; }
        public int IndexOf(T item) => AsCollection.IndexOf(item);
        public void Insert(int index, T item) => AsCollection.Insert(index, item);
        public void RemoveAt(int index) => AsCollection.RemoveAt(index);

        int IList.Add(object value)
        {
            if (value is not T t) return -1;
            Add(t);
            return Count - 1;
        }

        bool IList.Contains(object value) => value is T t && Contains(t);
        int IList.IndexOf(object value) => value is T t ? IndexOf(t) : -1;
        void IList.Insert(int index, object value) => Insert(index, (T)value);
        void IList.Remove(object value) => Remove((T)value);
        void ICollection.CopyTo(Array array, int index) => AsCollection.WriteTo(array, index);
        bool IList.IsFixedSize => false;
        bool IList.IsReadOnly => false;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => null;
        object IList.this[int index] { get => this[index]; set => this[index] = (T)value; }
    }
}
