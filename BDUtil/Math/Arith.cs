using System;
using System.Collections;
using System.Reflection;
using BDUtil.Bind;

namespace BDUtil.Math
{
    public interface IArith { }
    /// Static types & classes for doing arithmetic through generics, similar to I/EqualityComparer.
    /// Particularly, see BDEase.Unity/.../UnityArith.cs, where VectorX & color are supported.
    public interface IArith<T> : IArith
    {
        /// Returns a+b
        T Add(T a, T b);
        /// Returns a*b
        T Scale(float a, T b);
        /// Returns a * b.
        float Dot(T a, T b);
        /// NaN-detector.
        bool IsValid(T a);
    }

    /// Extension methods, constants, and type registration.
    /// Extensions generalize IArith<T> with operations implied by the existing ones (like dotproduct gets us length).
    public static class Arith
    {
        // Used for (very fuzzy) matching.
        public const float Epsilon = 1e-06f;

        #region Unity "Mathf" imports
        public const float PI = (float)System.Math.PI;
        public const float TAU = 2 * PI;
        public const float HALF_PI = PI / 2f;

        public static float Sqrt(float a) => (float)System.Math.Sqrt(a);
        public static float Clamp(float a, float min, float max) => System.Math.Min(System.Math.Max(a, min), max);
        public static float Clamp01(float a) => Clamp(a, 0f, 1f);
        public static float Repeat(float a, float length) => Clamp((float)(a - System.Math.Floor(a / length) * length), 0.0f, length);

        public static float Pow(float x, float y) => (float)System.Math.Pow(x, y);
        public static float Cos(float x) => (float)System.Math.Cos(x);
        public static float Sin(float x) => (float)System.Math.Sin(x);

        #endregion

        #region Arithmetic in terms of existing arithmetic.
        public static float Length2<T>(this IArith<T> thiz, T a) => thiz.Dot(a, a);
        public static float Length<T>(this IArith<T> thiz, T a) => Sqrt(thiz.Length2(a));
        public static T Clamp<T>(this IArith<T> thiz, T a, float length)
        {
            float len = thiz.Length2(a);
            if (len <= (length * length)) return a;
            if (len == 0) return default;
            return thiz.Scale(length / Sqrt(len), a);
        }
        public static float Normalize<T>(this IArith<T> thiz, T a, out T b)
        {
            float len = thiz.Length(a);
            b = thiz.Scale(1f / len, a);
            return len;
        }
        public static bool Approximately<T>(this IArith<T> thiz, T a, float epsilon) => thiz.Length2(a) < (epsilon * epsilon);
        public static bool Approximately<T>(this IArith<T> thiz, T a) => thiz.Approximately(a, Epsilon);
        public static bool Approximately<T>(this IArith<T> thiz, T a, T b, float epsilon) => thiz.Approximately(thiz.Difference(b, a), epsilon);
        public static T Negate<T>(this IArith<T> thiz, T a) => thiz.Scale(-1f, a);
        public static T Difference<T>(this IArith<T> thiz, T a, T b) => thiz.Add(a, thiz.Negate(b));
        public static T LerpUnclamped<T>(this IArith<T> thiz, T a, T b, float ratio) => thiz.Add(a, thiz.Scale(ratio, thiz.Difference(b, a)));
        public static T Lerp<T>(this IArith<T> thiz, T a, T b, float ratio) => thiz.LerpUnclamped(a, b, Clamp01(ratio));
        public static T Project<T>(this IArith<T> thiz, T a, T b)
        {
            float length2 = thiz.Length2(b);
            if (length2 < Epsilon) return default;
            float dot = thiz.Dot(a, b);
            return thiz.Scale(dot / length2, b);
        }
        public static T FirstValid<T>(this IArith<T> thiz, T a, T b) => thiz.IsValid(a) ? a : b;
        #endregion
    }
    /// VERY similar to EqualityComparer, provides the generic type dispatch through Arith<T>.Default.
    public class Arith<T> : IArith<T>
    {
        public static IArith<T> Default { get; private set; } = Bindings<ImplAttribute>.Default.GetBestInstance<IArith<T>>()
            .OrThrow("Couldn't find `IArith<T=[{0}]>`; remember to register one!", typeof(T));

        T IArith<T>.Add(T a, T b) => Default.Add(a, b);
        float IArith<T>.Dot(T a, T b) => Default.Dot(a, b);
        T IArith<T>.Scale(float a, T b) => Default.Scale(a, b);
        bool IArith<T>.IsValid(T a) => Default.IsValid(a);
    }
    [Impl]
    public struct IntArith : IArith<int>
    {
        int IArith<int>.Add(int a, int b) => a + b;
        float IArith<int>.Dot(int a, int b) => a * b;
        int IArith<int>.Scale(float a, int b) => (int)(a * b);
        bool IArith<int>.IsValid(int a) => false;
    }
    [Impl]
    public struct FloatArith : IArith<float>
    {
        float IArith<float>.Add(float a, float b) => a + b;
        float IArith<float>.Dot(float a, float b) => a * b;
        float IArith<float>.Scale(float a, float b) => a * b;
        bool IArith<float>.IsValid(float a) => !float.IsNaN(a);
    }

    /// Provides circular arithmetic (for angle measures).
    public struct CircleArith : IArith<float>
    {
        public readonly float Half;
        public readonly float Full;
        public CircleArith(float half, float full)
        {
            Half = half;
            Full = full;
        }
        /// Represents left & right handed rotations of up to PI (making a circle of TAU).
        public static readonly CircleArith PI = new(Arith.PI, Arith.TAU);
        /// Represents right-handed rotations only, 0->TAU.
        public static readonly CircleArith TAU = new(0f, Arith.TAU);
        /// Represents left & right handed rotations of up to 180 (making a circle of 360).
        public static readonly CircleArith DEGREE = new(180f, 360f);
        /// right-handed rotations only, 0->TAU.
        public static readonly CircleArith ABS_DEGREE = new(0f, 360f);

        /// Given a "wound up circle" that goes past +/-Tau, returns the equivalent angle.
        public float Shortest(float a) => Arith.Repeat(Arith.Repeat(a + Half, Full) - Half, Full);

        float IArith<float>.Add(float a, float b) => Shortest(a + b);
        /// This is only physically meaningful as an area or something.
        /// But: it's required for length to work correctly!
        float IArith<float>.Dot(float a, float b) => Shortest(a) * Shortest(b);
        float IArith<float>.Scale(float a, float b) => Shortest(a * b);
        bool IArith<float>.IsValid(float a) => !float.IsNaN(a);
    }
}