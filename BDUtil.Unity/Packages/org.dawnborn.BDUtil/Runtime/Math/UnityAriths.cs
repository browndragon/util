using BDUtil.Bind;
using UnityEngine;

namespace BDUtil.Math
{
    [Impl]
    public struct V2Arith : IArith<Vector2>
    {
        int IArith<Vector2>.Axes => 2;
        float IArith<Vector2>.GetAxis(in Vector2 a, int i) => a[i];
        void IArith<Vector2>.SetAxis(ref Vector2 a, int i, float value) => a[i] = value;
    }
    [Impl]
    public struct V3Arith : IArith<Vector3>
    {
        int IArith<Vector3>.Axes => 3;
        float IArith<Vector3>.GetAxis(in Vector3 a, int i) => a[i];
        void IArith<Vector3>.SetAxis(ref Vector3 a, int i, float value) => a[i] = value;
    }
    [Impl]
    public struct V4Arith : IArith<Vector4>
    {
        int IArith<Vector4>.Axes => 4;
        float IArith<Vector4>.GetAxis(in Vector4 a, int i) => a[i];
        void IArith<Vector4>.SetAxis(ref Vector4 a, int i, float value) => a[i] = value;
    }

    [Impl]
    public struct ColorArith : IArith<Color>
    {
        int IArith<Color>.Axes => 4;
        float IArith<Color>.GetAxis(in Color a, int i) => a[i];
        void IArith<Color>.SetAxis(ref Color a, int i, float value) => a[i] = value;
    }
    [Impl]
    public struct HSVAArith : IArith<HSVA>
    {
        int IArith<HSVA>.Axes => 4;
        float IArith<HSVA>.GetAxis(in HSVA a, int i) => a[i];
        void IArith<HSVA>.SetAxis(ref HSVA a, int i, float value) => a[i] = value;
    }
}