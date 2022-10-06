using System;

namespace BDUtil.Math
{
    public static class Randoms
    {
        /// Returns a value in [0,1)...
        public delegate float UnitRandom();
        public static Random system0 = new(0);
        public static UnitRandom system01 = () => (float)(2 * (system0.NextDouble() - .5));

        // A general target+fuzz algorithm.
        [Serializable]
        public struct Fuzzed<T>
        {
            public Fuzzed(T pivot = default, T scale = default) { Pivot = pivot; Scale = scale; }
            public T Pivot;
            public T Scale;
            public T HalfScale => Arith<T>.Default.Scale(.5f, Scale);
            public T Min => Arith<T>.Default.Difference(Pivot, HalfScale);
            public T Max => Arith<T>.Default.Add(Pivot, HalfScale);
        }

        // Returns a value in [0, +1).
        // You can give it a wider implementation for more chaos, but if you do you MUST
        // remain centered on .5.
        public static float Unit(this UnitRandom thiz) => (thiz ?? system01).Invoke();
        /// Returns a random value in [min, max)
        public static int Range(this UnitRandom thiz, int min, int max) => (int)thiz.Range((float)min, max);
        /// Returns a random value in [min, max)
        public static float Range(this UnitRandom thiz, float min, float max) => min + (max - min) * (thiz ?? system01).Invoke();
        /// Returns a random position in the cube [min, max) on each axis.
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
        public static float RandomValue(this UnitRandom thiz, Extent extent)
                => thiz.Range(extent.min, extent.max);
        public static int RandomValue(this UnitRandom thiz, ExtentInt extent)
        => thiz.Range(extent.min, extent.max);
        public static T RandomValue<T>(this UnitRandom thiz, Fuzzed<T> fuzzed)
        => thiz.Range(fuzzed.Min, fuzzed.Max);

        // // Returns a value in [-1,+1) whose absval has been raised to the given pow.
        // public static float Power(this UnitRandom thiz, float pow)
        // {
        //     if (float.IsNaN(pow) || float.IsInfinity(pow)) return 0;
        //     if (pow == 0) return 0f;
        //     float fuzz = thiz.Range(-1f, 1f + float.Epsilon);
        //     float sign = Arith.Sign(fuzz);
        //     return sign * Arith.Pow(sign * fuzz, pow);
        // }
    }
}