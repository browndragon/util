using System;
using System.Collections.Generic;

namespace BDUtil.Math
{
    public static class Randoms
    {
        /// Returns a value in [0,1)...
        public delegate float UnitRandom();
        public static readonly Random system0 = new(0);
        public static readonly UnitRandom const_5 = () => .5f;
        // Intentionally NOT readonly! Unity remaps this, first chance it gets.
        public static UnitRandom @default = () => (float)system0.NextDouble();
        // Maps [0,1) through the given arbitrary func.
        public static UnitRandom Distribution(this UnitRandom random, Func<float, float> func) => () => func.Invoke(random.Unit());
        // Maps [0,1) through the given ease, resulting in a different value distribution.
        public static UnitRandom Distribution(this UnitRandom random, Easings.Enum ease) => () => ease.Invoke(random.Unit());
        // Maps r in [0,1) into [-1,+1] (so .5->0f), performs the given power (reapplying sign), then maps back to [0,1).
        public static float Pow01(float pow, float r)
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
        // Maps random over [0,1) into the "sharpened" version centered at .5f (see Pow01).
        public static UnitRandom DistributionPow01(this UnitRandom thiz, float pow)
        {
            if (float.IsInfinity(pow) || float.IsNaN(pow) || pow == 0) return const_5;
            if (thiz == null) thiz = @default;
            if (pow == 1f) return thiz;
            return () => Pow01(pow, thiz.Invoke());
        }
        // Generalized value + allowable fuzz radius around it.
        // Despite use of term radius, fuzzing uses a cube centered on pivot.
        [Serializable]
        public struct Fuzzy<T>
        {
            public Fuzzy(T pivot = default, T fuzz = default)
            {
                Pivot = pivot; Fuzz = fuzz;
            }
            public T Pivot;
            public T Fuzz;
        }
        // Returns a value in [0, +1) (though apparently unity is [0,+1]?!).
        // You can give it a wider implementation for more chaos, but if you do you MUST
        // remain centered on .5. Better is to weight the distribution of results (see: Easings, Distribution, etc).
        public static float Unit(this UnitRandom thiz) => (thiz ?? @default).Invoke();

        /// Returns a random value in [min, max) with the same underlying distribution as UnitRandom.
        public static float Range(this UnitRandom thiz, float min, float max)
        {
            float @return = min;
            float delta = max - min;
            if (delta < 0f) return @return;
            return @return + delta * thiz.Unit();
        }
        /// Returns a random value in [min, max) with the same underlying distribution as UnitRandom.
        public static int Range(this UnitRandom thiz, int min, int max) => (int)System.Math.Floor(min + (max - min) * thiz.Unit());

        /// Returns a random position in the cube [min, max) as per Range on each axis.
        /// This works for any Arith type.
        public static T Range<T>(this UnitRandom thiz, T min, T max)
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

        public static bool RandomTrue(this UnitRandom thiz, float odds = .5f)
        => thiz.Unit() < odds;

        /// Returns a random position in the Extent as per Range(float).
        public static float RandomValue(this UnitRandom thiz, Extent extent)
                => thiz.Range(extent.min, extent.max);
        /// Returns a random position in the ExtentInt as per Range(int).
        public static int RandomValue(this UnitRandom thiz, ExtentInt extent)
        => thiz.Range(extent.min, extent.max);

        public static T RandomValue<T>(this UnitRandom thiz, IReadOnlyList<T> items)
        => items[thiz.Range(0, items.Count)];


        public static float Fuzzed(this UnitRandom thiz, float target, float fuzz, float @base = 0f)
        {
            if (float.IsNaN(target)) target = @base;
            if (float.IsNaN(fuzz)) return target;
            return thiz.Range(target - fuzz, target + fuzz);
        }
        public static T Fuzzed<T>(this UnitRandom thiz, Fuzzy<T> fuzzed, T @base = default)
        => thiz.Fuzzed(fuzzed.Pivot, fuzzed.Fuzz, @base);
        public static T Fuzzed<T>(this UnitRandom thiz, T target, T fuzz, T @base = default)
        {
            T @return = default;
            for (int i = 0; i < Arith<T>.Default.Axes; ++i)
            {
                float baseI = Arith<T>.Default.GetAxis(@base, i);
                float targetI = Arith<T>.Default.GetAxis(target, i);
                float fuzzI = Arith<T>.Default.GetAxis(fuzz, i);
                float fuzzed = thiz.Fuzzed(targetI, fuzzI, baseI);
                Arith<T>.Default.SetAxis(ref @return, i, fuzzed);
            }
            return @return;
        }
        static readonly List<int> keeps = new();
        // Returns (non-reentrant!!!) an ordered list of indices [0,count) randomly sampled to retain odds elements (round up).
        // Takes O(n) in `count`, even if `odds` is miniscule.
        public static IEnumerable<int> RandomIndices(this UnitRandom thiz, int count, float odds)
        {
            float rem = odds * count;
            int choices = (int)rem;
            rem -= choices;
            if (thiz.RandomTrue(rem)) choices++;
            return thiz.RandomIndices(count, choices);
        }
        // Returns (non-reentrant!!!) an ordered list of indices [0,count) randomly sampled choices times
        // Takes O(n) in `count`, even if `odds` is miniscule.
        public static IEnumerable<int> RandomIndices(this UnitRandom thiz, int count, int choices)
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
    }
}