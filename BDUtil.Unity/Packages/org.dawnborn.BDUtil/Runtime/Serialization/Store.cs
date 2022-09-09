using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil.Serialization
{
    /// Adapts a collection st it's serialized & visible in the inspector.
    /// The idea is that you need to specify the underlying collection<T> & T values; we'll try to help with internalized representation(s).
    /// The `T` is the type in code; the `TIn` is the type in the inspector.
    [Serializable]
    public class Store<TCollection, T, TIn> : ISerializationCallbackReceiver, IHasCollection<TCollection>
    where TCollection : ICollection<T>, new()
    {
        static readonly Converter<T, TIn> Inwards = Converter<T, TIn>.Default;
        static readonly Converter<TIn, T> Outwards = Converter<TIn, T>.Default;

        public readonly TCollection Collection = new();
        TCollection IHasCollection<TCollection>.Collection => Collection;
        IEnumerable IHasCollection.Collection => Collection;

        [SuppressMessage("IDE", "IDE0044")]
        [SerializeField] List<TIn> Data = new();
        [SuppressMessage("IDE", "IDE0044")]
        [SerializeField] List<TIn> Error = new();
        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            Data.Clear();
            foreach (T t in Collection) Data.Add(Inwards.Convert(t));
            // This gets called repeatedly while in view...
            // Collection.Clear();
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize()
        {
            Collection.Clear();
            Data.AddRange(Error);
            Error.Clear();
            foreach (TIn t in Data)
                try { Collection.Add(Outwards.Convert(t)); }
                catch { Error.Add(t); }
        }
    }
    [Serializable]
    public class Store<TColl, T> : Store<TColl, T, T>
    where TColl : ICollection<T>, new()
    { }
    [Serializable]
    public class StoreMap<TColl, K, V, KIn, VIn> : Store<TColl, KeyValuePair<K, V>, KVP.Entry<KIn, VIn>>
    where TColl : ICollection<KeyValuePair<K, V>>, new()
    { }
    [Serializable]
    public class StoreMap<TColl, K, V> : StoreMap<TColl, K, V, K, V>
    where TColl : ICollection<KeyValuePair<K, V>>, new()
    { }
    [Serializable]
    public class StoreMap<K, V> : StoreMap<Dictionary<K, V>, K, V>
    { }
}
