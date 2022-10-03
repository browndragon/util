using System;
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
            [Tooltip("Input is then divided by X; output is then multiplied by Y.")]
            [SerializeField] Vector2 scale;
            public AnimationCurve Curve => curve ??= AnimationCurve.Linear(0f, 0f, 1f, 1f);
            public Vector2 Offset => offset;
            public Vector2 Scale => scale == default ? Vector2.one : scale;
            public static implicit operator Scaled(AnimationCurve curve) => new() { curve = curve, scale = Vector2.one };
            public float Evaluate(float input)
            => Curve.Evaluate(Scale.x * (input - Offset.x)) * Scale.y + Offset.y;
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
            if (thiz.length < 0) return;
            float start = thiz[0].time, end = thiz[thiz.length - 1].time;
            for (int i = 0; i < thiz.keys?.Length / 2; ++i)
            {
                Keyframe front = thiz.keys[i].FlipX(), back = thiz.keys[^i].FlipX();
                front.time = end - (front.time - start);
                back.time = end - (back.time - start);
                (thiz.keys[i], thiz.keys[^i]) = (back, front);
            }
            if (thiz.keys?.Length % 2 == 1)
            {
                int index = 1 + thiz.length / 2;
                Keyframe middle = thiz.keys[index].FlipX();
                middle.time = end - (middle.time - start);
                thiz.keys[index] = middle;
            }
            (thiz.preWrapMode, thiz.postWrapMode) = (thiz.postWrapMode, thiz.preWrapMode);
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
            for (int i = 0; i < thiz.keys.Length; ++i)
            {
                Keyframe oldKey = thiz.keys[i];
                oldKey.time = scale.x * (oldKey.time - keyRect.x) + offset.x;
                oldKey.value = scale.y * (oldKey.value - keyRect.y) + offset.y;
                thiz.keys[i] = oldKey;
            }
        }
        public static void Concatenate(this AnimationCurve thiz, AnimationCurve that)
        {
            if (!(thiz?.length > 1)) return;
            if (!(that?.length > 1)) return;
            Keyframe myLast = thiz[thiz.length - 1];
            Keyframe theirFirst = that[0];
            myLast.outTangent = theirFirst.outTangent;
            myLast.outWeight = theirFirst.outWeight;
            thiz.keys[thiz.length - 1] = myLast;
            for (int j = 1; j < that?.keys.Length; ++j)
            {
                Keyframe theirs = that.keys[j];
                theirs.time = theirs.time - theirFirst.time + myLast.time;
                theirs.value = theirs.value - theirFirst.value + myLast.value;
                that.keys[j] = theirs;
            }
            thiz.postWrapMode = that.postWrapMode;
        }
        public static void AddInterpolated(this AnimationCurve thiz, Func<float, float> func, float start = 0f, float step = 1 / 16f, float length = 1f)
        {
            for (float i = 0f; i < length; i += step) thiz.AddKey(start + i, func(i / length));
            thiz.AddKey(start + length, func(1f));
        }
        #endregion  // Flip/Squish AnimationCurve
    }
}