using System;
using System.Collections.Generic;

namespace BDUtil.Math
{
    public static class Randoms
    {
        /// Returns a value in [0,1)...
        public delegate float UnitRandom();
        public static Random system0 = new(0);
        public static UnitRandom const_5 = () => .5f;
        public static UnitRandom @default = () => (float)system0.NextDouble();
        // Maps [0,1) through the given ease.
        public static UnitRandom Distribution(this UnitRandom random, Easings.Enum ease) => () => ease.Invoke(random.Unit());
        // Maps [0,1) through the given arbitrary func.
        public static UnitRandom Distribution(this UnitRandom random, Func<float, float> func) => () => func.Invoke(random.Unit());
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
        // Maps random over [0,1) into the "sharpened" version (see Pow01).
        public static UnitRandom DistributionPow01(this UnitRandom thiz, float pow)
        {
            if (float.IsInfinity(pow) || float.IsNaN(pow) || pow == 0) return const_5;
            if (thiz == null) thiz = @default;
            if (pow == 1f) return thiz;
            return () => Pow01(pow, thiz.Invoke());
        }
        // A general target+fuzz algorithm.
        // Returns a point in the prism `scale` wide centered on `pivot`, with each term
        // sharpened/blunted by being raised to the sign-respecting `pow`.
        [Serializable]
        public struct Fuzzed<T>
        {
            public Fuzzed(T pivot = default, T scale = default)
            {
                Pivot = pivot; Scale = scale;
            }
            public T Pivot;
            public T Scale;
            public T HalfScale => Arith<T>.Default.Scale(.5f, Scale);
            public T Min => Arith<T>.Default.Difference(Pivot, HalfScale);
            public T Max => Arith<T>.Default.Add(Pivot, HalfScale);
        }

        // Returns a value in [0, +1).
        // You can give it a wider implementation for more chaos, but if you do you MUST
        // remain centered on .5.
        public static float Unit(this UnitRandom thiz) => (thiz ?? @default).Invoke();

        /// Returns a random value in [min, max) with the same underlying distribution as UnitRandom.
        public static float Range(this UnitRandom thiz, float min, float max) => min + (max - min) * thiz.Unit();
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
        /// Returns a random position in the Extent as per Range(float).
        public static float RandomValue(this UnitRandom thiz, Extent extent)
                => thiz.Range(extent.min, extent.max);
        /// Returns a random position in the ExtentInt as per Range(int).
        public static int RandomValue(this UnitRandom thiz, ExtentInt extent)
        => thiz.Range(extent.min, extent.max);
        /// Returns a random position in the Fuzzed as per Range<T>.
        public static T RandomValue<T>(this UnitRandom thiz, Fuzzed<T> fuzzed)
        => thiz.Range(fuzzed.Min, fuzzed.Max);

        public static T RandomValue<T>(this UnitRandom thiz, IReadOnlyList<T> items)
        => items[thiz.Range(0, items.Count)];
    }
}