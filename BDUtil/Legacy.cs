using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;

namespace BDUtil
{
    public static class Legacies
    {
        public static Legacy<T> AsLegacy<T>(this Raw.IContainer<T> thiz)
        => new(thiz, thiz.Contains);
        public static Legacy<T> AsLegacy<T>(this IReadOnlyCollection<T> thiz, Func<T, bool> containz = default)
        => new(thiz, containz);
    }
    public readonly struct Legacy<T> : ICollection<T>
    {
        readonly IReadOnlyCollection<T> Thiz;
        readonly Func<T, bool> Containz;
        public Legacy(IReadOnlyCollection<T> thiz, Func<T, bool> containz)
        {
            Thiz = thiz;
            Containz = containz ?? (thiz.Contains);
        }
        public int Count => Thiz.Count;
        public bool IsReadOnly => true;

        public void Add(T item) => throw new NotImplementedException();
        public void Clear() => throw new NotImplementedException();
        public bool Contains(T item) => Containz(item);
        public void CopyTo(T[] array, int arrayIndex) => Arrays.CopyTo(Thiz, array, arrayIndex);
        public IEnumerator<T> GetEnumerator() => Thiz.GetEnumerator();
        public bool Remove(T item) => throw new NotImplementedException();

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}