
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using BDUtil.Fluent;

namespace BDUtil
{
    /// Creates a collection out of another which throws rather than modify.
    public readonly struct ReadOnly<T> : ICollection<T>, IReadOnlyCollection<T>
    {
        readonly IReadOnlyCollection<T> Thiz;
        public ReadOnly(IReadOnlyCollection<T> thiz) => Thiz = thiz;
        public int Count => Thiz.Count;
        public bool IsReadOnly => true;
        public void Add(T item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(T item) => Thiz.Contains(item);
        public void CopyTo(T[] array, int arrayIndex) => Iter.WriteTo(this, array, arrayIndex);
        public bool Remove(T item) => throw new NotImplementedException();

        public IEnumerator<T> GetEnumerator() => Thiz.GetEnumerator();
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}