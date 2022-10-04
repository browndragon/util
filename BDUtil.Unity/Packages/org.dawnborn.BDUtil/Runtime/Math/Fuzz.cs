using UnityEngine;

namespace BDUtil.Math
{
    /// Returns values near 0 which can be used to fuzz other values.
    public static class Fuzz
    {
        /// Generates a symmetric fuzz value from -1->+1.
        public static float Value => 2f * (UnityEngine.Random.value - .5f);
        /// Generates a "squared fuzz", a parabola centered around 0 -- fuzz value^2, negative if the original was.
        public static float Value2 => GetPowFuzz(2f);
        /// Generates a linear, quadratic, sqrt, etc shaped -1->+1 distribution about 0.
        public static float GetPowFuzz(float pow)
        {
            if (!float.IsFinite(pow)) return 0;
            if (pow == 0) return Value;
            float fuzz = Value;
            float sign = Mathf.Sign(fuzz);
            return sign * Mathf.Pow(sign * fuzz, pow);
        }
        public static float Float(float scale, float pow = 1f)
        => scale * GetPowFuzz(pow);

        public static Vector2 Vector2(Vector2 scale, Vector2 pow = default) => new(
            Float(scale.x, pow.x),
            Float(scale.y, pow.y)
        );
        public static Vector3 Vector3(Vector3 scale, Vector3 pow = default) => new(
            Float(scale.x, pow.x),
            Float(scale.y, pow.y),
            Float(scale.z, pow.z)
        );
        public static Vector4 Vector4(Vector4 scale, Vector4 pow = default) => new(
            Float(scale.x, pow.x),
            Float(scale.y, pow.y),
            Float(scale.z, pow.z),
            Float(scale.w, pow.w)
        );
        public static HSVA HSVA(HSVA scale, HSVA pow = default) => new(
            Float(scale.h, pow.h),
            Float(scale.s, pow.s),
            Float(scale.v, pow.v),
            Float(scale.a, pow.a)
        );

        public static float RandomPoint(this Extent thiz) => Extent.NormalizedToPoint(thiz, UnityEngine.Random.value);
        public static float RandomValue(this AnimationCurve thiz)
        => thiz.length <= 0 ? float.NaN : thiz.Evaluate(UnityEngine.Random.Range(thiz[0].time, thiz[thiz.length - 1].time));
        public static float RandomValue(this AnimationCurves.Scaled thiz)
        => thiz.Curve.RandomValue() * thiz.Scale.y + thiz.Offset.y;
        public static Vector2 RandomPoint(this Rect thiz) => Rect.NormalizedToPoint(thiz, new(UnityEngine.Random.value, UnityEngine.Random.value));
        public static Vector3 RandomPoint(this Bounds thiz) => thiz.min + UnityEngine.Vector3.Scale(thiz.size, new(
            UnityEngine.Random.value,
            UnityEngine.Random.value,
            UnityEngine.Random.value
        ));
        // There doesn't seem to be a bounds4. Weird.
    }
}