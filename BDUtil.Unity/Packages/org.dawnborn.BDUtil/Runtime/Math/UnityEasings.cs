using System;
using BDUtil.Bind;
using UnityEngine;

namespace BDUtil.Math
{
    [Serializable]
    public struct CurveStruct : Easings.IEase
    {
        public AnimationCurve Curve;
        public float Ease(float f) => Curve.Evaluate(f);
    }

    [Serializable]
    public struct ScaleStruct : Easings.IEase
    {
        public static readonly ScaleStruct Default = new() { Impl = new Easings.EnumStruct(), In = 1f, Out = 1f };

        [SerializeReference, Subclass(Default = typeof(Easings.EnumStruct))]
        public Easings.IEase Impl;
        public float In;
        public float Out;
        public float Ease(float f) => Out * Impl.Ease(f / In);
    }
}