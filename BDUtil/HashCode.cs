using System;
using System.Collections.Generic;
using System.Runtime.CompilerServices;

namespace BDUtil
{
    public readonly struct HashCode : IEquatable<HashCode>
    {
        public const int kHashCodePrime = 92821;
        public static readonly HashCode Default = new(1);

        public HashCode Of<T>(T t) => Default.And(t);
        public HashCode OfRaw(int i) => Default.AndRaw(i);

        readonly int V;
        HashCode(int v) => V = v;

        public HashCode And<T>(T t) => AndRaw(EqualityComparer<T>.Default.GetHashCode(t));
        public HashCode AndRaw(int t) => new(unchecked(kHashCodePrime * (V switch { 0 => 1, _ => V }) + t));

        public bool Equals(HashCode other) => V == other.V;
        public override bool Equals(object obj) => this.AsEqual(obj);
        public override int GetHashCode() => V;
        public override string ToString() => V.ToString();

        public static implicit operator int(HashCode hc) => hc.V;
    }
}
