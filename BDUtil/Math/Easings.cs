using System;

namespace BDUtil.Math
{
    using static Arith;
    /// A well-known Ease.
    /// maps [0f,1f]->[0f-delta,1f+delta] (sometimes they might squeak outside of the range).
    /// See BDEase.Unity/Packages/Runtime/Easer.cs for an inspector-compatible way to select an easing.
    /// See https://easings.net/ for a cheatsheet.
    public static class Easings
    {
        /// Names a well-known easing function in a unity-compatible way.
        public enum Enum
        {
            Linear = default,
            InQuad,
            OutQuad,
            InOutQuad,
            InCubic,
            OutCubic,
            InOutCubic,
            InQuart,
            OutQuart,
            InOutQuart,
            InQuint,
            OutQuint,
            InOutQuint,
            InSine,
            OutSine,
            InOutSine,
            InExpo,
            OutExpo,
            InOutExpo,
            InCirc,
            OutCirc,
            InOutCirc,
            InBack,
            OutBack,
            InOutBack,
            InElastic,
            OutElastic,
            InOutElastic,
            InBounce,
            OutBounce,
            InOutBounce,
            None,
        }

        public static float ClampInvoke(this Enum thiz, float @in) => thiz.Invoke(@in switch
        {
            var x when x < 0 => 0,
            var x when x > 1 => 1,
            var x => x
        });
        /// Applies `thiz` (as the named function).
        public static float Invoke(this Enum thiz, float @in) => thiz switch
        {
            Enum.Linear => Linear(@in),
            Enum.InQuad => InQuad(@in),
            Enum.OutQuad => OutQuad(@in),
            Enum.InOutQuad => InOutQuad(@in),
            Enum.InCubic => InCubic(@in),
            Enum.OutCubic => OutCubic(@in),
            Enum.InOutCubic => InOutCubic(@in),
            Enum.InQuart => InQuart(@in),
            Enum.OutQuart => OutQuart(@in),
            Enum.InOutQuart => InOutQuart(@in),
            Enum.InQuint => InQuint(@in),
            Enum.OutQuint => OutQuint(@in),
            Enum.InOutQuint => InOutQuint(@in),
            Enum.InSine => InSine(@in),
            Enum.OutSine => OutSine(@in),
            Enum.InOutSine => InOutSine(@in),
            Enum.InExpo => InExpo(@in),
            Enum.OutExpo => OutExpo(@in),
            Enum.InOutExpo => InOutExpo(@in),
            Enum.InCirc => InCirc(@in),
            Enum.OutCirc => OutCirc(@in),
            Enum.InOutCirc => InOutCirc(@in),
            Enum.InBack => InBack(@in),
            Enum.OutBack => OutBack(@in),
            Enum.InOutBack => InOutBack(@in),
            Enum.InElastic => InElastic(@in),
            Enum.OutElastic => OutElastic(@in),
            Enum.InOutElastic => InOutElastic(@in),
            Enum.InBounce => InBounce(@in),
            Enum.OutBounce => OutBounce(@in),
            Enum.InOutBounce => InOutBounce(@in),
            Enum.None => None(@in),
            _ => throw new NotImplementedException($"Unrecognized {thiz}"),
        };

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

        public static float None(float t) => t <= 0f ? 0f : 1f;

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