using System;
using UnityEngine;

namespace BDUtil.Math
{
    /// Returns values near 0 which can be used to fuzz other values.
    public static class UnityRandoms
    {
        public static float GetUnityRandomValue() => UnityEngine.Random.value;
        public static readonly Randoms.UnitRandom main = GetUnityRandomValue;

        public static float RandomValue(this Randoms.UnitRandom thiz, AnimationCurve curve)
        => curve.length <= 0
        ? float.NaN
        : curve.Evaluate(thiz.Range(curve[0].time, curve[curve.length - 1].time));
        public static float RandomValue(this Randoms.UnitRandom thiz, AnimationCurves.Scaled curve)
        => thiz.RandomValue(curve.Curve) * curve.Scale.y + curve.Offset.y;
        public static Vector2 RandomValue(this Randoms.UnitRandom thiz, Rect rect)
        => thiz.Range(rect.min, rect.max);
        public static Vector3 RandomValue(this Randoms.UnitRandom thiz, Bounds bounds)
        => thiz.Range(bounds.min, bounds.max);
        // There doesn't seem to be a bounds4. Weird.
    }
}