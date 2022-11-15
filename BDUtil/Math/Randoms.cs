using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Fluent;

namespace BDUtil.Math
{
    public static class Randoms
    {
        public interface IRandom
        {
            /// Returns a value in [0,1)
            float Unit { get; }
        }
        /// Returns a random value in [min, max) with the same underlying distribution as IRandom.
        /// This is equivalent to [min, max]; you can't prove I'd ever generate _exactly_ `max`.
        /// But you might (but you won't) see exactly `min`.
        public static float Range(this IRandom thiz, float min, float max)
        => min + thiz.Unit * (max - min);
        /// Returns a random value in [min, max) with the same underlying distribution as IRandom.
        public static int Range(this IRandom thiz, int min, int max)
        => (int)global::System.Math.Floor(thiz.Range((float)min, max));
        /// Generates `true` with `odds` out of 1f; else false.
        public static bool Odds(this IRandom thiz, float odds = .5f)
        => thiz.Unit < odds;

        /// Fixed/testing value.
        [Serializable]
        public struct Value : IRandom
        {
            public float value;
            public Value(float value) => this.value = value;
            public float Unit => value;
        }
        /// Arbitrary smuggler of probability function.
        public struct Func : IRandom
        {
            public global::System.Func<float> func;
            public Func(global::System.Func<float> func) => this.func = func.OrThrow();
            public float Unit => func.Invoke();
        }
        /// Sharpens or blunts the distribution of values by mapping [0,1]->[-1,+1]^(1+value0)->[0,1].
        [Serializable]
        public struct Pow : IRandom
        {
            public float value0;
            public IRandom source;
            public IRandom Source { get => source ??= Randoms.main; set => source = value; }
            public float Unit => Sharpen(Source.Unit, 1f + value0);
            public Pow(float value0, IRandom source = default) { this.value0 = value0; this.source = source; }
            public static implicit operator Pow(float f) => new(f - 1f);
            public static Pow operator +(Pow a, Pow b) => new(a.value0 + b.value0, a.source);
            public static Pow operator -(Pow a) => new(-a.value0, a.source);
            public static Pow operator -(Pow a, Pow b) => new(a.value0 - b.value0, a.source);
            public static float operator *(Pow a, float b) => (1f + a.value0) * b;
            public static float operator *(float b, Pow a) => (1f + a.value0) * b;
            public static float operator /(Pow a, float b) => (1f + a.value0) / b;
            public static float operator /(float b, Pow a) => b / (1f + a.value0);
            // Maps r in [0,1] into [-1,+1] (so .5~>0f), performs the given power (reapplying sign), then maps back to [0,1).
            public static float Sharpen(float r, float pow)
            {
                // Adjust to be in (-1,+1) instead of [0,1).
                r *= 2f;
                r -= 1f;

                // Take the power in absval, then return the sign so we're still in (-1,1)
                float sign = Arith.Sign(r);
                r *= sign;
                r = Arith.Pow(r, pow);
                r *= sign;

                // Adjust back to [0,1).
                r += 1f;
                r /= 2f;
                return r;
            }
        }
        /// Real actual system random function.
        public class System : IRandom
        {
            public const float Max = int.MaxValue;
            public global::System.Random Random;
            public System(int seed) : this(new global::System.Random(seed)) { }
            public System(global::System.Random random = default) => Random = random ?? new global::System.Random(0);
            public System(IRandom source) : this(source switch
            {
                System system => system.Random.Next(),
                _ => (int)(int.MaxValue * source.Unit),
            })
            { }
            public float Unit => Random.Next() / Max;

            public void Reseed(int seed) => Random = new(seed);
        }
        [SuppressMessage("IDE", "IDE1006")]
        public static System main { get; private set; } = new();
        /// Create a new domain under which the old main is set aside and the new one used.
        /// This is great for re-seeding main, random generating in a domain, etc.
        public ref struct Scoped
        {
            public readonly System was;
            internal Scoped(int seed = -1)
            {
                if (seed == -1) seed = main.Random.Next();
                was = main;
                Randoms.main = new(seed);
            }
            public void Dispose() => Randoms.main = was;
        }
        public static Scoped Scope(int seed = -1) => new(seed);

        /// Returns a random position in the cube [min, max) as per Range on each axis.
        /// This works for any Arith type.
        public static T Range<T>(this IRandom thiz, T min, T max)
        {
            IArith<T> arith = Arith<T>.Default;
            T accum = default;
            for (int i = 0; i < arith.Axes; ++i)
            {
                float random = thiz.Range(arith.GetAxis(min, i), arith.GetAxis(max, i));
                arith.SetAxis(ref accum, i, random);
            }
            return accum;
        }

        /// Returns a random position in the Extent as per Range(float).
        public static float Range(this IRandom thiz, Interval extent)
        => thiz.Range(extent.min, extent.max);
        /// Returns a random position in the ExtentInt -- including the endpoint.
        public static int Range(this IRandom thiz, IntervalInt extent)
        => thiz.Range(extent.min, extent.max + 1);

        /// Chooses one item from the list at even odds.
        public static T Range<T>(this IRandom thiz, IReadOnlyList<T> items)
        => items[thiz.Index(items)];
        /// Chooses one item from the list at the given odds.
        public static T Range<T>(this IRandom thiz, IReadOnlyList<T> items, float totalOdds, IEnumerable<float> odds)
        => items[thiz.Index(totalOdds, odds)];

        public static int Index<T>(this IRandom thiz, IReadOnlyList<T> items)
        => items?.Count > 0 ? thiz.Range(0, items.Count) : -1;
        /// Generates a random number in [0, totalOdds), then decrements each `odds` from that value until <=0.
        /// IOW, gets an index from odds with the given probabilities.
        /// If you use totalOdds < sum(odds), you can ensure later values can't be selected.
        /// It's an error to use totalOdds > sum(odds).
        public static int Index(this IRandom thiz, float totalOdds, IEnumerable<float> odds)
        {
            float odd = thiz.Range(0, totalOdds);
            int i = 0;
            foreach (float o in odds)
            {
                if ((odd -= o) <= 0f) return i;
                i++;
            }
            throw new NotSupportedException();
        }

        // Returns (non-reentrant & reused!!!) an ordered list of indices [0,count) randomly sampled to retain odds elements (round up, no replacement).
        // Takes O(n) in `count`, even if `odds` is miniscule.
        public static IReadOnlyList<int> Deal(this IRandom thiz, int count, float odds)
        {
            float rem = odds * count;
            int choices = (int)rem;
            rem -= choices;
            if (thiz.Odds(rem)) choices++;
            return thiz.Deal(count, choices);
        }
        // Returns (non-reentrant & reused!!!) an ordered list of indices [0,count) randomly sampled choices times without replacement.
        // Takes O(n) in `count`, even if `odds` is miniscule.
        public static IReadOnlyList<int> Deal(this IRandom thiz, int count, int choices)
        {
            keeps.Clear();
            for (
                int i = 0; i < count; ++i
            ) keeps.Add(i);
            for (
                int i = 0, lose = count - choices; i < lose; ++i
            ) keeps.RemoveAt(thiz.Range(0, keeps.Count));
            return keeps;
        }

        /// Returns target|base + x where x in [-fuzz,+fuzz], and |base means "override":"target, or base if target is NaN".
        public static float Fuzz(this IRandom thiz, float target, float fuzz, float @base = 0f)
        {
            if (float.IsNaN(target)) target = @base;
            if (float.IsNaN(fuzz) || fuzz == 0f) return target;
            return thiz.Range(target - fuzz, target + fuzz);
        }
        /// Does Fuzz(float) per-axis of `T` (which is the purpose of the override semantic).
        public static T Fuzz<T>(this IRandom thiz, T target, T fuzz, T @base = default)
        {
            T @return = default;
            for (int i = 0; i < Arith<T>.Default.Axes; ++i)
            {
                float baseI = Arith<T>.Default.GetAxis(@base, i);
                float targetI = Arith<T>.Default.GetAxis(target, i);
                float fuzzI = Arith<T>.Default.GetAxis(fuzz, i);
                float fuzzed = thiz.Fuzz(targetI, fuzzI, baseI);
                Arith<T>.Default.SetAxis(ref @return, i, fuzzed);
            }
            return @return;
        }
        static readonly List<int> keeps = new();
    }
}