using System;

namespace BDUtil.Math
{
    using static Arith;
    /// An Ease. maps [0f,1f]->[0f-delta,1f+delta] (sometimes they might squeak outside of the range).
    /// See BDEase.Unity/Packages/Runtime/Easer.cs for an inspector-compatible way to select an easing.
    /// See https://easings.net/ for a cheatsheet.
    public static class Easings
    {
        public static float ClampInvoke(this Func<float, float> thiz, float value)
        {
            value = Clamp01(value);
            value = thiz?.Invoke(value) ?? value;
            return value;
        }
        public static float ClampInvoke(this IEase thiz, float value)
        {
            value = Clamp01(value);
            value = thiz?.Ease(value) ?? value;
            return value;
        }
        public interface IEase { float Ease(float f); }

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
            InOutBounce
        }
        [Serializable]
        public struct EnumStruct : IEase
        {
            public Easings.Enum Enum;
            public float Ease(float f) => Enum.Apply(f);
        }
        public struct FuncStruct : IEase
        {
            public Func<float, float> Func;
            public float Ease(float f) => Func?.Invoke(f) ?? Impl.Linear(f);
        }

        /// Applies `thiz`.
        public static float Apply(this Enum thiz, float @in) => thiz switch
        {
            Enum.Linear => Impl.Linear(@in),
            Enum.InQuad => Impl.InQuad(@in),
            Enum.OutQuad => Impl.OutQuad(@in),
            Enum.InOutQuad => Impl.InOutQuad(@in),
            Enum.InCubic => Impl.InCubic(@in),
            Enum.OutCubic => Impl.OutCubic(@in),
            Enum.InOutCubic => Impl.InOutCubic(@in),
            Enum.InQuart => Impl.InQuart(@in),
            Enum.OutQuart => Impl.OutQuart(@in),
            Enum.InOutQuart => Impl.InOutQuart(@in),
            Enum.InQuint => Impl.InQuint(@in),
            Enum.OutQuint => Impl.OutQuint(@in),
            Enum.InOutQuint => Impl.InOutQuint(@in),
            Enum.InSine => Impl.InSine(@in),
            Enum.OutSine => Impl.OutSine(@in),
            Enum.InOutSine => Impl.InOutSine(@in),
            Enum.InExpo => Impl.InExpo(@in),
            Enum.OutExpo => Impl.OutExpo(@in),
            Enum.InOutExpo => Impl.InOutExpo(@in),
            Enum.InCirc => Impl.InCirc(@in),
            Enum.OutCirc => Impl.OutCirc(@in),
            Enum.InOutCirc => Impl.InOutCirc(@in),
            Enum.InBack => Impl.InBack(@in),
            Enum.OutBack => Impl.OutBack(@in),
            Enum.InOutBack => Impl.InOutBack(@in),
            Enum.InElastic => Impl.InElastic(@in),
            Enum.OutElastic => Impl.OutElastic(@in),
            Enum.InOutElastic => Impl.InOutElastic(@in),
            Enum.InBounce => Impl.InBounce(@in),
            Enum.OutBounce => Impl.OutBounce(@in),
            Enum.InOutBounce => Impl.InOutBounce(@in),
            _ => throw new NotImplementedException($"Unrecognized {thiz}"),
        };

        public static readonly Func<float, float> Linear = Impl.Linear;
        public static readonly Func<float, float> InQuad = Impl.InQuad;
        public static readonly Func<float, float> OutQuad = Impl.OutQuad;
        public static readonly Func<float, float> InOutQuad = Impl.InOutQuad;
        public static readonly Func<float, float> InCubic = Impl.InCubic;
        public static readonly Func<float, float> OutCubic = Impl.OutCubic;
        public static readonly Func<float, float> InOutCubic = Impl.InOutCubic;
        public static readonly Func<float, float> InQuart = Impl.InQuart;
        public static readonly Func<float, float> OutQuart = Impl.OutQuart;
        public static readonly Func<float, float> InOutQuart = Impl.InOutQuart;
        public static readonly Func<float, float> InQuint = Impl.InQuint;
        public static readonly Func<float, float> OutQuint = Impl.OutQuint;
        public static readonly Func<float, float> InOutQuint = Impl.InOutQuint;
        public static readonly Func<float, float> InSine = Impl.InSine;
        public static readonly Func<float, float> OutSine = Impl.OutSine;
        public static readonly Func<float, float> InOutSine = Impl.InOutSine;
        public static readonly Func<float, float> InExpo = Impl.InExpo;
        public static readonly Func<float, float> OutExpo = Impl.OutExpo;
        public static readonly Func<float, float> InOutExpo = Impl.InOutExpo;
        public static readonly Func<float, float> InCirc = Impl.InCirc;
        public static readonly Func<float, float> OutCirc = Impl.OutCirc;
        public static readonly Func<float, float> InOutCirc = Impl.InOutCirc;
        public static readonly Func<float, float> InBack = Impl.InBack;
        public static readonly Func<float, float> OutBack = Impl.OutBack;
        public static readonly Func<float, float> InOutBack = Impl.InOutBack;
        public static readonly Func<float, float> InElastic = Impl.InElastic;
        public static readonly Func<float, float> OutElastic = Impl.OutElastic;
        public static readonly Func<float, float> InOutElastic = Impl.InOutElastic;
        public static readonly Func<float, float> InBounce = Impl.InBounce;
        public static readonly Func<float, float> OutBounce = Impl.OutBounce;
        public static readonly Func<float, float> InOutBounce = Impl.InOutBounce;

        /// https://gist.github.com/Kryzarel/bba64622057f21a1d6d44879f9cd7bd4
        public static class Impl
        {
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
        }
    }
}