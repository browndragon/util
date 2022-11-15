using System;
using System.Drawing;
using UnityEngine;

namespace BDUtil.Math
{
    public static class AnimationCurves
    {
        // An animation curve wrapper which supports scaling the inputs & outputs
        [Serializable]
        public struct Scaled
        {
            [Tooltip("Should be a curve [0,1]->[0,1]")]
            [SerializeField] AnimationCurve curve;
            [Tooltip("Input is -X; output is finally +Y")]
            [SerializeField] Vector2 offset;
            [Tooltip("Input is then divided by X+1; output is then multiplied by Y+1.")]
            [SerializeField] Vector2 scale0;
            public AnimationCurve Curve => curve ??= AnimationCurve.Linear(0f, 0f, 1f, 1f);
            public Vector2 Offset => offset;
            public Vector2 Scale => scale0 + Vector2.one;
            public Scaled(AnimationCurve curve, Vector2 offset = default, Vector2 scale0 = default)
            {
                this.curve = curve;
                this.offset = offset;
                this.scale0 = scale0;
            }
            public static implicit operator Scaled(AnimationCurve curve) => new() { curve = curve, offset = Vector2.zero, scale0 = Vector2.zero };
            public float Evaluate(float input)
            => Curve.Evaluate((input - Offset.x) / Scale.x) * Scale.y + Offset.y;

            public Rect Bounds
            {
                get
                {
                    Rect keyRect = curve.GetKeyRect();
                    Vector2 min = keyRect.min, max = keyRect.max;
                    min.x = min.x * (1 + scale0.x) + offset.x;
                    min.y = (min.y - offset.y) / (1 + scale0.y);
                    max.x = max.x * (1 + scale0.x) + offset.x;
                    max.y = (max.y - offset.y) / (1 + scale0.y);
                    return Rect.MinMaxRect(min.x, min.y, max.x, max.y);
                }
            }
        }

        /// Treat a curve as a distribution; pick a random point between its keyframes and get the evaluation there.
        public static float GetRandom(this AnimationCurve thiz)
        => (thiz.length <= 0)
        ? float.NaN
        : thiz.Evaluate(UnityEngine.Random.Range(thiz[0].time, thiz[thiz.length - 1].time));

        #region Flip/Squish AnimationCurve

        public static Rect GetKeyRect(this AnimationCurve thiz)
        {
            float
                xMin = float.PositiveInfinity, yMin = float.PositiveInfinity,
                xMax = float.NegativeInfinity, yMax = float.NegativeInfinity;
            for (int i = 0; i < thiz.keys?.Length; ++i)
            {
                Keyframe key = thiz.keys[i];
                xMin = Mathf.Min(xMin, key.time);
                xMax = Mathf.Max(xMax, key.time);
                yMin = Mathf.Min(yMin, key.value);
                yMax = Mathf.Max(yMax, key.value);
            }
            return Rect.MinMaxRect(xMin, yMin, xMax, yMax);
        }

        public static Keyframe FlipX(this Keyframe thiz)
        {
            thiz.weightedMode = thiz.weightedMode switch
            {
                WeightedMode.In => WeightedMode.Out,
                WeightedMode.Out => WeightedMode.In,
                _ => thiz.weightedMode
            };
            (thiz.inTangent, thiz.outTangent) = (thiz.outTangent, thiz.inTangent);
            (thiz.inWeight, thiz.outWeight) = (thiz.outWeight, thiz.inWeight);
            return thiz;
        }
        public static Keyframe FlipY(this Keyframe thiz)
        {
            thiz.inTangent = -thiz.inTangent;
            thiz.outTangent = -thiz.outTangent;
            return thiz;
        }
        /// Keep the same footprint, but play this curve backwards.
        public static void FlipX(this AnimationCurve thiz)
        {
            Keyframe[] keys = thiz.keys;
            if (!(keys?.Length > 0)) return;
            float start = keys[0].time, end = keys[thiz.length - 1].time;
            for (int i = 0; i < keys.Length / 2; ++i)
            {
                Keyframe front = keys[i].FlipX(), back = keys[^(i + 1)].FlipX();
                front.time = end - (front.time - start);
                back.time = end - (back.time - start);
                // But not *value*, which stays.
                (keys[i], keys[^(i + 1)]) = (back, front);
            }
            if (keys.Length % 2 == 1)
            {
                int index = thiz.length / 2; // Earlier we required less than this.
                Keyframe middle = keys[index].FlipX();
                middle.time = end - (middle.time - start);
                keys[index] = middle;
            }
            thiz.keys = keys;
            (thiz.preWrapMode, thiz.postWrapMode) = (thiz.postWrapMode, thiz.preWrapMode);
        }
        public static AnimationCurve FlippedX(this AnimationCurve thiz)
        {
            thiz.FlipX();
            return thiz;
        }
        /// Keep the same footprint, but have each point reverse monotonicity
        public static void FlipY(this AnimationCurve thiz)
        {
            if (thiz.length < 0) return;
            Rect keyRect = thiz.GetKeyRect();
            for (int i = 0; i < thiz.keys?.Length; ++i)
            {
                Keyframe k = thiz.keys[i].FlipY();
                k.value = keyRect.yMax - (k.value - keyRect.yMin);
                thiz.keys[i] = k;
            }
        }
        public static AnimationCurve FlippedY(this AnimationCurve thiz)
        {
            thiz.FlipY();
            return thiz;
        }
        public static void Transform(this AnimationCurve thiz, Rect toRect)
        {
            if (toRect.width == 0f || toRect.height == 0f)
            {
                for (int i = thiz.length - 1; i >= 0; --i) thiz.RemoveKey(i);
                return;
            }
            Rect keyRect = thiz.GetKeyRect();
            Vector2 scale = new(toRect.width / keyRect.width, toRect.height / keyRect.height);
            Vector2 offset = new(toRect.x - keyRect.x, toRect.y - keyRect.y);
            Keyframe[] keys = thiz.keys;
            for (int i = 0; i < keys.Length; ++i)
            {
                Keyframe oldKey = keys[i];
                oldKey.time = scale.x * (oldKey.time - keyRect.x) + offset.x;
                oldKey.value = scale.y * (oldKey.value - keyRect.y) + offset.y;
                keys[i] = oldKey;
            }
            thiz.keys = keys;
        }
        public static AnimationCurve Transformed(this AnimationCurve thiz, Rect toRect)
        {
            thiz.Transform(toRect);
            return thiz;
        }

        // public static void Concatenate(this AnimationCurve thiz, AnimationCurve that)
        // {
        //     if (!(thiz?.length > 1)) return;
        //     if (!(that?.length > 1)) return;
        //     Keyframe myLast = thiz[thiz.length - 1];
        //     Keyframe theirFirst = that[0];
        //     myLast.outTangent = theirFirst.outTangent;
        //     myLast.outWeight = theirFirst.outWeight;
        //     thiz.keys[thiz.length - 1] = myLast;
        //     for (int j = 1; j < that?.keys.Length; ++j)
        //     {
        //         Keyframe theirs = that.keys[j];
        //         theirs.time = theirs.time - theirFirst.time + myLast.time;
        //         theirs.value = theirs.value - theirFirst.value + myLast.value;
        //         that.keys[j] = theirs;
        //     }
        //     thiz.postWrapMode = that.postWrapMode;
        // }
        // public static AnimationCurve Concatenated0101(AnimationCurve a, AnimationCurve b)
        // {
        //     AnimationCurve c = new(a.keys);
        //     c.Concatenate(b);
        //     c.Transform(new(0, 0, 1, 1));
        //     return c;
        // }
        public static void AddInterpolated(this AnimationCurve thiz, Func<float, float> func, float start = 0f, float step = 1 / 16f, float length = 1f)
        {
            for (float i = 0f; i < length; i += step) thiz.AddKey(start + i, func(i / length));
            thiz.AddKey(start + length, func(1f));
        }
        public static AnimationCurve Interpolated0101(Func<float, float> func, float step = 1 / 16f)
        {
            AnimationCurve a = new();
            a.AddInterpolated(func, 0, step, 1);
            a.Transform(new(0, 0, 1, 1));
            return a;
        }

        #endregion  // Flip/Squish AnimationCurve
    }
}