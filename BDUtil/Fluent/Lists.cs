using System.Collections;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BDUtil.Fluent
{
    public static class Lists
    {
        // Not even enumerable, since we'll use `this[]` to build the enumerator...
        public interface IMicroList<T>
        {
            int Count { get; }
            T this[int i] { get; }
        }
        /// Since we had to provide an adapter it mightn't be quite as efficient as raw access, but should be close...
        public struct Enumerator<TMicroList, T> : IEnumerable<T>, IEnumerator<T>
        where TMicroList : IMicroList<T>
        {
            readonly TMicroList Indexable;
            public int i;
            public T Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Indexable[i];
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator(TMicroList indexable) { Indexable = indexable; i = -1; }
            object IEnumerator.Current
            {
                [MethodImpl(MethodImplOptions.AggressiveInlining)]
                get => Current;
            }
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public bool MoveNext() => ++i < Indexable.Count;

            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public Enumerator<TMicroList, T> GetEnumerator() => this;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator<T> IEnumerable<T>.GetEnumerator() => this;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            IEnumerator IEnumerable.GetEnumerator() => this;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Reset() => i = -1;
            [MethodImpl(MethodImplOptions.AggressiveInlining)]
            public void Dispose() { }
        }
    }
}