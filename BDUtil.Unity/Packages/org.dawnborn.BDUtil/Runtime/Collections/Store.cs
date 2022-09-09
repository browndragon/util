using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil
{
    [Serializable]
    public class Store<TCollection, T, TIn> : ISerializationCallbackReceiver, ICollection<T>, IReadOnlyCollection<T>, ICollection
    where TCollection : ICollection<T>, new()
    {
        public readonly TCollection AsCollection = new();
        [SuppressMessage("IDE", "IDE0044")]
        [SerializeField] List<TIn> Data = new();
        [SuppressMessage("IDE", "IDE0044")]
        [SerializeField] List<TIn> Error = new();
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            Data.Clear();
            foreach (T t in AsCollection) Data.Add(Internal(t));
            // This gets called repeatedly while in view...
            // Collection.Clear();
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            AsCollection.Clear();
            Data.AddRange(Error);
            Error.Clear();
            foreach (TIn t in Data) if (!TryAdd(External(t))) Error.Add(t);
        }
        protected virtual TIn Internal(T t) => (TIn)(object)t;
        protected virtual T External(TIn t) => (T)(object)t;
        public virtual bool TryAdd(T t)
        {
            try { AsCollection.Add(t); return true; }
            catch { return false; }
        }

        /// And then implement usual Collection methods.
        public int Count => AsCollection.Count;
        public void Add(T item) => TryAdd(item).OrThrow();

        public void Clear() => AsCollection.Clear();

        public bool Contains(T item) => AsCollection.Contains(item);

        public void CopyTo(T[] array, int arrayIndex) => AsCollection.CopyTo(array, arrayIndex);

        public IEnumerator<T> GetEnumerator() => AsCollection.GetEnumerator();

        public bool Remove(T item) => AsCollection.Remove(item);

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();

        void ICollection.CopyTo(Array array, int index) => AsCollection.WriteTo(array, index);
        bool ICollection<T>.IsReadOnly => false;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => null;
    }
}
