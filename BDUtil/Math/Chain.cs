using System.Runtime.CompilerServices;

namespace BDUtil.Math
{
    /// Util for hashes & comparators, both of which chain int operations.
    /// For instance, `Chain.Hash ^ someInt() ^ someOtherInt() ^ someThirdInt();`
    /// or `Chain.Cmp || MostImportantCmp(a, b) || SortaImportantCmp(a2,b2) || orElse(a3,b3);`
    /// AFAICT there's no obvious logical && here.
    public readonly ref struct Chain
    {
        public const int kHashCodePrime = 92821;
        public static Chain Cmp => default;
        public static Chain Hash => new(1);

        readonly int Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        Chain(int v) => Value = v;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator int(Chain c) => c.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Chain(int c) => new(c);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Chain(float b) => Bitcast.Int(b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static implicit operator Chain(bool? b) => b switch { true => +1, null => 0, false => -1 };
        /// Useful for a comparator: returns the first non-zero.
        /// This can short circuit: Chain.Cmp || FirstCmp(a.x, b.x) || SecondCmp(a.y, b.y) || ...;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Chain operator |(Chain a, Chain b)
        => a.Value != 0 ? a : b;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator true(Chain a)
        => a.Value != 0;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static bool operator false(Chain a)
        => a.Value == 0;

        /// Useful for a hashcode: multiplies the first by a prime, adds the second.
        /// This can't short circuit: Chain.Hash ^ a ^ b ^ c ^ ...;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Chain operator ^(Chain a, int b)
        { unchecked { return new(kHashCodePrime * a.Value + b); } }
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Chain operator ^(Chain a, Chain b)
        => a ^ b.Value;
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Chain operator ^(Chain a, float b)
        => a ^ Bitcast.Int(b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Chain operator ^(Chain a, uint b)
        => a ^ Bitcast.Int(b);
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Chain operator ^(Chain a, bool? b)
        => a ^ b switch { true => 1f, null => .5f, false => 0f };
        [MethodImpl(MethodImplOptions.AggressiveInlining)]
        public static Chain operator ^(Chain a, object b)
        => a ^ (b?.GetHashCode() ?? 0);
    }
}
