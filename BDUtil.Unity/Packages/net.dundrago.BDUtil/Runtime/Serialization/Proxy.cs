using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil.Serialization
{
    /// A serializable object which surfaces as an ordered list<T>.
    /// During edit cycles, any errors of reinsertion are stored in Errors.
    /// Your [Serializable] type should have a field exactly:
    /// [SerializeField, SuppressMessage("IDE", "IDE0044"), Proxy]
    ///   Serialization.Proxy<T> Proxy = new();
    /// and call its Before and After during ISerializationCallback's before & after.
    [Serializable]
    internal class Proxy<TIn, TOut>
    {
        readonly Func<TIn, TOut> Serialize;
        readonly Func<TOut, TIn> Deserialize;

        public Proxy(Func<TIn, TOut> serialize, Func<TOut, TIn> deserialize)
        {
            Serialize = serialize;
            Deserialize = deserialize;
        }

        [SerializeField]
        [SuppressMessage("IDE", "IDE0044")]
        internal List<TOut> Data = new();

        [SerializeField]
        [SuppressMessage("IDE", "IDE0044")]
        internal List<TOut> Errors = new();

        public void Before(ICollection<TIn> source)
        {
            // This is necessary to prevent infinite elements.
            // But it seems like there's a race; if a human clicks [+], then:
            // After needs to be called to see it before this Before.
            // But maybe that's guaranteed...?
            Data.Clear();
            foreach (TIn t in source) Data.Add(Serialize(t));
            // You cannot clear here:
            // Before is called *many* times while inspector is open, to snapshot the data.
        }
        public void After(ICollection<TIn> source)
        {
            source.Clear();
            List<TOut> oldErrors = Errors;
            Errors = new();
            foreach (TOut t in Data) try { source.Add(Deserialize(t)); } catch { Errors.Add(t); }
            foreach (TOut t in oldErrors) try { source.Add(Deserialize(t)); } catch { Errors.Add(t); }
            Data.Clear();
        }
    }
    [Serializable]
    internal class Proxy<T> : Proxy<T, T>
    {
        static T Identity(T t) => t;
        public Proxy() : base(Identity, Identity) { }
    }
}