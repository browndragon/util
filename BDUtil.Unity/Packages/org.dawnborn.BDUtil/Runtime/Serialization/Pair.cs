using System;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil.Serialization
{
    /// KeyValuePair isn't Unity-serializable. This is!
    [Serializable]
    public struct Pair<T1, T2> : IEquatable<Pair<T1, T2>>
    {
        public T1 D1; public T2 D2;

        public Pair(T1 d1, T2 d2) { D1 = d1; D2 = d2; }
        public void Deconstruct(out T1 key, out T2 value) { key = D1; value = D2; }

        public bool Equals(Pair<T1, T2> other) => D1.EqualsT(other.D1) && D2.EqualsT(other.D2);
        public override bool Equals(object o) => this.AsEqual(o);
        public override int GetHashCode() => HashCode.Default.And(D1).And(D2);
        public override string ToString() => $"({D1},{D2})";
        public static implicit operator KeyValuePair<T1, T2>(Pair<T1, T2> thiz) => Pair.ToKVP(thiz);
        public static implicit operator Pair<T1, T2>(KeyValuePair<T1, T2> thiz) => Pair.ToPair(thiz);
        public static implicit operator (T1, T2)(Pair<T1, T2> thiz) => Pair.ToTuple(thiz);
        public static implicit operator Pair<T1, T2>((T1, T2) thiz) => Pair.ToPair(thiz);
    }
    /// Utilities for working with Pair.
    public static class Pair
    {
        public static KeyValuePair<T1, T2> ToKVP<T1, T2>(Pair<T1, T2> thiz) => new(thiz.D1, thiz.D2);
        public static Pair<T1, T2> ToPair<T1, T2>(KeyValuePair<T1, T2> thiz) => new(thiz.Key, thiz.Value);
        public static Pair<T1, T2> ToPair<T1, T2>((T1 t1, T2 t2) thiz) => new(thiz.t1, thiz.t2);
        public static (T1 D1, T2 D2) ToTuple<T1, T2>(Pair<T1, T2> thiz) => (thiz.D1, thiz.D2);
        public static KeyValuePair<(R, C), V> ToTupleKVP<R, C, V>(Pair<Pair<R, C>, V> thiz) => new((thiz.D1.D1, thiz.D1.D2), thiz.D2);
        public static Pair<Pair<R, C>, V> ToNestPair<R, C, V>(KeyValuePair<(R, C), V> thiz) => new(thiz.Key, thiz.Value);
    }
}
