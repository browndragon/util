using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil.Raw
{
    public class None<T> : ICollection<T>, IContainer<T>, IEnumerator<T>
    {
        public static None<T> Default = new();
        private None() { }
        int IReadOnlyCollection<T>.Count => 0;
        int ICollection<T>.Count => 0;
        bool ICollection<T>.IsReadOnly => true;
        T IEnumerator<T>.Current => default;
        object IEnumerator.Current => default;
        bool IContainer<T>.Contains(T t) => false;
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
    }
}