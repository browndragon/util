using BDUtil.Bind;
using UnityEngine;

namespace BDUtil.Math
{
    [Impl]
    public struct V2Arith : IArith<Vector2>
    {
        public static readonly Vector2 NaN = new(float.NaN, float.NaN);
        Vector2 IArith<Vector2>.Add(Vector2 a, Vector2 b) => a + b;
        float IArith<Vector2>.Dot(Vector2 a, Vector2 b) => Vector2.Dot(a, b);
        Vector2 IArith<Vector2>.Scale(float a, Vector2 b) => a * b;
        bool IArith<Vector2>.IsValid(Vector2 a) => !float.IsNaN(a.x) && !float.IsNaN(a.y);

    }
    [Impl]
    public struct V3Arith : IArith<Vector3>
    {
        public static readonly Vector3 NaN = new(float.NaN, float.NaN, float.NaN);
        Vector3 IArith<Vector3>.Add(Vector3 a, Vector3 b) => a + b;
        float IArith<Vector3>.Dot(Vector3 a, Vector3 b) => Vector3.Dot(a, b);
        Vector3 IArith<Vector3>.Scale(float a, Vector3 b) => a * b;
        bool IArith<Vector3>.IsValid(Vector3 a) => !float.IsNaN(a.x) && !float.IsNaN(a.y) && !float.IsNaN(a.z);
    }
    [Impl]
    public struct V4Arith : IArith<Vector4>
    {
        public static readonly Vector4 NaN = new(float.NaN, float.NaN, float.NaN, float.NaN);
        Vector4 IArith<Vector4>.Add(Vector4 a, Vector4 b) => a + b;
        float IArith<Vector4>.Dot(Vector4 a, Vector4 b) => Vector4.Dot(a, b);
        Vector4 IArith<Vector4>.Scale(float a, Vector4 b) => a * b;
        bool IArith<Vector4>.IsValid(Vector4 a) => !float.IsNaN(a.w) && !float.IsNaN(a.x) && !float.IsNaN(a.y) && !float.IsNaN(a.z);
    }
    [Impl]
    public struct ColorArith : IArith<Color>
    {
        public static readonly Color NaN = new(float.NaN, float.NaN, float.NaN, float.NaN);

        static readonly IArith<Vector4> AsV4 = Arith<Vector4>.Default;
        Color IArith<Color>.Add(Color a, Color b) => a + b;
        float IArith<Color>.Dot(Color a, Color b) => AsV4.Dot(a, b);
        Color IArith<Color>.Scale(float a, Color b) => a * b;
        bool IArith<Color>.IsValid(Color a) => !float.IsNaN(a.r) && !float.IsNaN(a.g) && !float.IsNaN(a.b) && !float.IsNaN(a.a);
    }
}