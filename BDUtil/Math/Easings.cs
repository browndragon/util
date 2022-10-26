using System;

namespace BDUtil.Math
{
    using static Arith;
    /// A well-known Ease.
    /// maps [0f,1f]->[0f-delta,1f+delta] (sometimes they might squeak outside of the range).
    /// See https://easings.net/ for a cheatsheet.
    public static class Easings
    {
        /// Names a well-known easing function in a unity-compatible way.
        [Flags]
        public enum Enum
        {
            Linear = 0,
            Quad = 1 << 0,
            Cubic = 1 << 1,
            Quart = 1 << 2,
            Quint = 1 << 3,
            Sine = 1 << 4,
            Expo = 1 << 5,
            Circ = 1 << 6,
            Back = 1 << 7,
            Elastic = 1 << 8,
            Bounce = 1 << 9,
            Hard = 1 << 10,

            // Modifiers (on a default Linear, if not otherwise stated).
            // If you don't specify any of In/Out, you get InOut (same as if you say both).
            In = 1 << 28,
            Out = 1 << 29,
            FlipX = 1 << 30,
            FlipY = 1 << 31,
        }
        static readonly Enum NonMetaMask = ~(Enum.In | Enum.Out | Enum.FlipX | Enum.FlipY);

        public static float ClampInvoke(this Enum thiz, float t) => thiz.Invoke(t.GetValenceInclusive(0f, 1f) switch
        {
            true => 1,
            null => t,
            false => 0
        });
        /// Applies `thiz` (as the named function).
        public static float Invoke(this Enum thiz, float t)
        {
            if (thiz.HasFlag(Enum.FlipX)) t = 1 - t;
            bool? @in = thiz.HasFlag(Enum.In) && !thiz.HasFlag(Enum.Out)
                ? true
                : thiz.HasFlag(Enum.Out) && !thiz.HasFlag(Enum.In)
                ? false
                : null;
            float value = (thiz & NonMetaMask, @in) switch
            {
                (Enum.Linear, _) => t,
                (Enum.Quad, true) => InQuad(t),
                (Enum.Quad, false) => OutQuad(t),
                (Enum.Quad, null) => InOutQuad(t),
                (Enum.Cubic, true) => InCubic(t),
                (Enum.Cubic, false) => OutCubic(t),
                (Enum.Cubic, null) => InOutCubic(t),
                (Enum.Quart, true) => InQuart(t),
                (Enum.Quart, false) => OutQuart(t),
                (Enum.Quart, null) => InOutQuart(t),
                (Enum.Quint, true) => InQuint(t),
                (Enum.Quint, false) => OutQuint(t),
                (Enum.Quint, null) => InOutQuint(t),
                (Enum.Sine, true) => InSine(t),
                (Enum.Sine, false) => OutSine(t),
                (Enum.Sine, null) => InOutSine(t),
                (Enum.Expo, true) => InExpo(t),
                (Enum.Expo, false) => OutExpo(t),
                (Enum.Expo, null) => InOutExpo(t),
                (Enum.Circ, true) => InCirc(t),
                (Enum.Circ, false) => OutCirc(t),
                (Enum.Circ, null) => InOutCirc(t),
                (Enum.Back, true) => InBack(t),
                (Enum.Back, false) => OutBack(t),
                (Enum.Back, null) => InOutBack(t),
                (Enum.Elastic, true) => InElastic(t),
                (Enum.Elastic, false) => OutElastic(t),
                (Enum.Elastic, null) => InOutElastic(t),
                (Enum.Bounce, true) => InBounce(t),
                (Enum.Bounce, false) => OutBounce(t),
                (Enum.Bounce, null) => InOutBounce(t),
                (Enum.Hard, true) => InHard(t),
                (Enum.Hard, false) => OutHard(t),
                (Enum.Hard, null) => InOutHard(t),
                _ => throw thiz.BadValue(),
            };
            if (thiz.HasFlag(Enum.FlipY)) value = 1 - value;
            return value;
        }

        /// https://gist.github.com/Kryzarel/bba64622057f21a1d6d44879f9cd7bd4
        public static float Linear(float t) => t;

        public static float InQuad(float t) => t * t;
        public static float OutQuad(float t) => 1 - InQuad(1 - t);
        public static float InOutQuad(float t)
        {
            if (t < 0.5f) return InQuad(t * 2) / 2;
            return 1 - InQuad((1 - t) * 2) / 2;
        }

        public static float InCubic(float t) => t * t * t;
        public static float OutCubic(float t) => 1 - InCubic(1 - t);
        public static float InOutCubic(float t)
        {
            if (t < 0.5f) return InCubic(t * 2) / 2;
            return 1 - InCubic((1 - t) * 2) / 2;
        }

        public static float InQuart(float t) => t * t * t * t;
        public static float OutQuart(float t) => 1 - InQuart(1 - t);
        public static float InOutQuart(float t)
        {
            if (t < 0.5f) return InQuart(t * 2) / 2;
            return 1 - InQuart((1 - t) * 2) / 2;
        }

        public static float InQuint(float t) => t * t * t * t * t;
        public static float OutQuint(float t) => 1 - InQuint(1 - t);
        public static float InOutQuint(float t)
        {
            if (t < 0.5f) return InQuint(t * 2) / 2;
            return 1 - InQuint((1 - t) * 2) / 2;
        }

        public static float InSine(float t) => 1 - Cos(t * PI / 2);
        public static float OutSine(float t) => Sin(t * PI / 2);
        public static float InOutSine(float t) => (Cos(t * PI) - 1) / -2;

        public static float InExpo(float t) => Pow(2, 10 * (t - 1));
        public static float OutExpo(float t) => 1 - InExpo(1 - t);
        public static float InOutExpo(float t)
        {
            if (t < 0.5f) return InExpo(t * 2) / 2;
            return 1 - InExpo((1 - t) * 2) / 2;
        }

        public static float InCirc(float t) => 1 - Sqrt(1 - t * t);
        public static float OutCirc(float t) => Sqrt(1 - Pow(t - 1, 2));
        public static float InOutCirc(float t)
        {
            if (t < 0.5f) return InCirc(t * 2) / 2;
            return 1 - InCirc((1 - t) * 2) / 2;
        }

        public static float InElastic(float t) => 1 - OutElastic(1 - t);
        public static float OutElastic(float t)
        {
            float p = 0.3f;
            return Pow(2, -10 * t) * Sin((t - p / 4) * (2 * PI) / p) + 1;
        }
        public static float InOutElastic(float t)
        {
            if (t < 0.5f) return InElastic(t * 2) / 2;
            return 1 - InElastic((1 - t) * 2) / 2;
        }

        public static float InBack(float t)
        {
            float s = 1.70158f;
            return t * t * ((s + 1) * t - s);
        }
        public static float OutBack(float t) => 1 - InBack(1 - t);
        public static float InOutBack(float t)
        {
            if (t < 0.5f) return InBack(t * 2) / 2;
            return 1 - InBack((1 - t) * 2) / 2;
        }

        public static float InBounce(float t) => 1 - OutBounce(1 - t);
        public static float OutBounce(float t)
        {
            float div = 2.75f;
            float mult = 7.5625f;

            if (t < 1 / div)
            {
                return mult * t * t;
            }
            else if (t < 2 / div)
            {
                t -= 1.5f / div;
                return mult * t * t + 0.75f;
            }
            else if (t < 2.5 / div)
            {
                t -= 2.25f / div;
                return mult * t * t + 0.9375f;
            }
            else
            {
                t -= 2.625f / div;
                return mult * t * t + 0.984375f;
            }
        }
        public static float InOutBounce(float t)
        {
            if (t < 0.5f) return InBounce(t * 2) / 2;
            return 1 - InBounce((1 - t) * 2) / 2;
        }

        public static float InHard(float t) => t <= 0f ? 0f : 1f;
        public static float OutHard(float t) => t < 1f ? 0f : 1f;
        public static float InOutHard(float t) => t <= .5f ? 0f : 1f;

        // // Function cache so you don't keep creating dynamics. Is this really worth it? I dunno.
        // public static class Funcs
        // {
        //     public static readonly Func<float, float> Linear = Easings.Linear;
        //     public static readonly Func<float, float> InQuad = Easings.InQuad;
        //     public static readonly Func<float, float> OutQuad = Easings.OutQuad;
        //     public static readonly Func<float, float> InOutQuad = Easings.InOutQuad;
        //     public static readonly Func<float, float> InCubic = Easings.InCubic;
        //     public static readonly Func<float, float> OutCubic = Easings.OutCubic;
        //     public static readonly Func<float, float> InOutCubic = Easings.InOutCubic;
        //     public static readonly Func<float, float> InQuart = Easings.InQuart;
        //     public static readonly Func<float, float> OutQuart = Easings.OutQuart;
        //     public static readonly Func<float, float> InOutQuart = Easings.InOutQuart;
        //     public static readonly Func<float, float> InQuint = Easings.InQuint;
        //     public static readonly Func<float, float> OutQuint = Easings.OutQuint;
        //     public static readonly Func<float, float> InOutQuint = Easings.InOutQuint;
        //     public static readonly Func<float, float> InSine = Easings.InSine;
        //     public static readonly Func<float, float> OutSine = Easings.OutSine;
        //     public static readonly Func<float, float> InOutSine = Easings.InOutSine;
        //     public static readonly Func<float, float> InExpo = Easings.InExpo;
        //     public static readonly Func<float, float> OutExpo = Easings.OutExpo;
        //     public static readonly Func<float, float> InOutExpo = Easings.InOutExpo;
        //     public static readonly Func<float, float> InCirc = Easings.InCirc;
        //     public static readonly Func<float, float> OutCirc = Easings.OutCirc;
        //     public static readonly Func<float, float> InOutCirc = Easings.InOutCirc;
        //     public static readonly Func<float, float> InBack = Easings.InBack;
        //     public static readonly Func<float, float> OutBack = Easings.OutBack;
        //     public static readonly Func<float, float> InOutBack = Easings.InOutBack;
        //     public static readonly Func<float, float> InElastic = Easings.InElastic;
        //     public static readonly Func<float, float> OutElastic = Easings.OutElastic;
        //     public static readonly Func<float, float> InOutElastic = Easings.InOutElastic;
        //     public static readonly Func<float, float> InBounce = Easings.InBounce;
        //     public static readonly Func<float, float> OutBounce = Easings.OutBounce;
        //     public static readonly Func<float, float> InOutBounce = Easings.InOutBounce;
        // }
    }
}