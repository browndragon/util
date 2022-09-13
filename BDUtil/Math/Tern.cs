using System;
using System.Diagnostics.CodeAnalysis;

namespace BDUtil.Math
{
    // Despite https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/operators/true-false-operators
    // we *can't* use `bool?`, because their algebra is wrong for e.g. `null|true` where we want `tbd`.
    // Inspired by https://github.com/eelstork/BehaviorTrees but with more differenter algebra.
    [SuppressMessage("IDE", "IDE1006")]
    [Serializable]
    /// tern provides A "bool-but-ternary" (for completion states or filters).
    /// It's similar but with different operational semantics to `bool?` :
    /// Sometimes we'll use `yes` to mean true, `no` to mean false, and `tbd` to mean null.
    public readonly struct tern : IEquatable<tern>, IComparable<tern>
    {
        public static readonly tern @true = new(true);// new(Value.@true);
        public static readonly tern @null = default;
        public static readonly tern @false = new(false);

        [SuppressMessage("IDE", "IDE0044")]
        readonly bool? value;
        tern(bool? data) => value = data;

        /// Bools can also be explicitly converted with their `.tern()` extension method.
        public static implicit operator tern(bool? that) => new(that);
        public static implicit operator tern(int that) => new(that.Valence());
        public static implicit operator tern(float that) => new(that.Valence());
        public static explicit operator bool?(tern thiz) => thiz.value;
        public static explicit operator int(tern thiz) => thiz.@switch(+1, 0, -1);

        public bool isTrue => value.HasValue && value.Value;
        public bool isNull => !value.HasValue;
        // True _or_ null. This isn't actually truthy in all contexts, but then neither is it falsey in all contexts...
        // there's no "isFalsey" because the semantics of that are a little brain bendy; using !isTrue is easier to read.
        public bool isTruthy => !isFalse;
        public bool isFalse => value.HasValue && !value.Value;

        // Short circuit chain ops; the bread & butter...
        // https://docs.microsoft.com/en-us/dotnet/csharp/language-reference/language-specification/expressions#11133-user-defined-conditional-logical-operators

        /// Sequence/Strict: x&&y => T.false(x) ? x : T.&(x, y)
        /// => Run y iff x is `true` (if it's tbd, we need to wait; if no, we're done).
        /// You can use x&y to run both, returning the least-truthy.
        /// This does mean tbd&&no => tbd, while tbd&no => no in addition to processing both sides.
        /// But that's the nature of early exit!
        /// You can append `&no` to force side effects as part of a selector:
        /// `a() || b()&no || c()` will stop if a is yes or tbd (so only executing b on a=no);
        /// since b()&no, it will min b's value to no thus continue on to execute c!
        /// Or see % for an operator which works in either setting.
        public static tern Min(tern x, tern y) => x <= y ? x : y;
        public static tern operator &(tern x, tern y) => Min(x, y);
        public static bool operator false(tern x) => x.@switch(false, true, true);

        /// Selector/Lenient: x||y => T.true(x) ? x : T.|(x, y)
        /// => Run y iff x is `false` (if it's tbd, we need to wait; if yes, we're done).
        /// You can use x|y to run both, returning the most-truthy.
        /// This does mean tbd||yes => tbd, while tbd|yes => yes in addition to processing both sides.
        /// But that's the nature of early exit!
        /// You can append `|yes` to force side effects as part of a selector:
        /// `a() && b()|yes && c()` will stop if a is no or tbd (so only executing b on a=yes);
        /// since b()|yes, it will max b's value to yes thus continue on to execute c!
        /// Or see % for an operator which works in either setting.
        public static tern Max(tern x, tern y) => x >= y ? x : y;
        public static tern operator |(tern x, tern y) => Max(x, y);
        public static bool operator true(tern x) => x.@switch(true, true, false);

        /// Replace operator: x%y=>y's value, having evaluated and discarded x.
        /// This lets you insert side effect expressions concisely.
        /// See also: .Let() ; note that returns thiz (==x), not y!
        public static tern Replace(tern _, tern y) => y;
        public static tern operator %(tern x, tern y) => Replace(x, y);

        /// Root non-short-circuiting operator; make it easier to examine a tern.
        public T @switch<T>(T @true = default, T @null = default, T @false = default)
        => value switch { true => @true, null => @null, false => @false, };

        /// Stark Difference: True iff x & y are both done and not equal to each other.
        // In particular, if either is tbd, this is false (or if they differ).
        public static bool Stark(tern x, tern y) => x != y && !~x && !~y;
        /// The opposite of Stark, if two terns are "similar".
        public static bool Fuzzy(tern x, tern y) => !Stark(x, y);
        // Stark operator: True iff x & y are done and different from each other.
        // In particular, if either is tbd, this is false (or if they differ).
        public static bool operator ^(tern x, tern y) => Stark(x, y);
        // If only we had a `~=` fuzzy match operator...

        /// Optimistic cast: no->false, tbd,yes->true.
        public static bool operator +(tern x) => x.isTruthy;
        /// Incompletion operator: Determine if a thing is null.
        /// Think like a destructor: it's "null" afterwards [at least in unity :->].
        /// "Is this concrete" is thus `!~` ("not incomplete" or "baka operator").
        // This is also chosen so that if there *were* a fuzzy match operator,
        // `null~=false` would acceptably mesh with ~null&&~false; not that it has to,
        // but it's nice that it could...
        public static bool operator ~(tern x) => x.isNull;
        /// Pessimistic operator: Cast to a bool st no,tbd->false; yes->true.
        public static bool operator -(tern x) => x.isTrue;
        /// Invert operator: Turn true<->false (leaving tbd alone).
        public static tern operator !(tern x) => x.@switch(@false, @null, @true);

        // We consider true > null > false
        public static bool operator <(tern x, tern y) => x.CompareTo(y) < 0;
        public static bool operator >(tern x, tern y) => x.CompareTo(y) > 0;
        public static bool operator <=(tern x, tern y) => x.CompareTo(y) <= 0;
        public static bool operator >=(tern x, tern y) => x.CompareTo(y) >= 0;
        public static bool operator ==(tern x, tern y) => x.value == y.value;
        public static bool operator !=(tern x, tern y) => x.value != y.value;

        public bool Equals(tern other) => value == other.value;
        public int CompareTo(tern other) => value.AsPInt().CompareTo(other.value.AsPInt());
        public override bool Equals(object value) => value is tern other && Equals(other);
        public override int GetHashCode() => @switch(1, 0, -1);
        public override string ToString() => @switch("+", "~", "-");
    }


    [SuppressMessage("IDE", "IDE1006")]
    public static class ternsExt
    {
        /// Remap a bool? into a tern using an arbitrary truth table, whose default is identity.
        public static tern tern(
            this bool? thiz, bool? @true = true, bool? @null = null, bool? @false = false
        ) => thiz switch { true => @true, null => @null, false => @false, };
        /// Remap a bool->tern using an arbitrary truth table whose default is identity (success/fail).
        public static tern tern(this bool thiz, bool? @true = true, bool? @false = false)
        => thiz ? @true : @false;
        /// The _other_ common bool->tern mapping, success/tbd.
        public static tern orTBD(this bool thiz)
        => thiz.tern(@false: null);
    }
}