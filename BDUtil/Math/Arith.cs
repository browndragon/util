using BDUtil.Bind;
using BDUtil.Fluent;

namespace BDUtil.Math
{
    public interface IArith { }
    /// Static types & classes for doing arithmetic through generics, similar to I/EqualityComparer.
    /// Particularly, see BDEase.Unity/.../UnityArith.cs, where VectorX & color are supported.
    public interface IArith<T> : IArith
    {
        int Axes { get; }
        /// Cheaper way to get Dot(Axes[i], a)
        float GetAxis(in T a, int i);
        void SetAxis(ref T a, int i, float f);
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

        public static float Round(float a) => (float)System.Math.Round(a);
        public static float Sqrt(float a) => (float)System.Math.Sqrt(a);
        public static float Clamp(float a, float min, float max) => System.Math.Min(System.Math.Max(a, min), max);
        public static float Clamp01(float a) => Clamp(a, 0f, 1f);
        public static float Repeat(float a, float length) => Clamp((float)(a - System.Math.Floor(a / length) * length), 0.0f, length);

        public static float Sign(float x) => System.Math.Sign(x);
        public static float Pow(float x, float y) => (float)System.Math.Pow(x, y);
        public static float Cos(float x) => (float)System.Math.Cos(x);
        public static float Sin(float x) => (float)System.Math.Sin(x);

        #endregion

        #region Arithmetic in terms of existing arithmetic.
        public static float Dot<T>(this IArith<T> thiz, T a, T b)
        {
            float dot = 0f;
            for (int i = 0; i < thiz.Axes; ++i) dot += thiz.GetAxis(a, i) * thiz.GetAxis(b, i);
            return dot;
        }
        public static T Add<T>(this IArith<T> thiz, T a, T b)
        {
            T summed = default;
            for (int i = 0; i < thiz.Axes; ++i)
            {
                thiz.SetAxis(ref summed, i, thiz.GetAxis(a, i) + thiz.GetAxis(b, i));
            }
            return summed;
        }

        public static T Scale<T>(this IArith<T> thiz, float scale, T a)
        {
            T scaled = default;
            for (int i = 0; i < thiz.Axes; ++i)
            {
                thiz.SetAxis(ref scaled, i, scale * thiz.GetAxis(a, i));
            }
            return scaled;
        }
        public static T Scale<T>(this IArith<T> thiz, T a, T b)
        {
            T scaled = default;
            for (int i = 0; i < thiz.Axes; ++i)
            {
                thiz.SetAxis(ref scaled, i, thiz.GetAxis(a, i) * thiz.GetAxis(b, i));
            }
            return scaled;
        }
        public static bool IsValid<T>(this IArith<T> thiz, T a)
        {
            for (int i = 0; i < thiz.Axes; ++i)
            {
                if (float.IsNaN(thiz.GetAxis(a, i))) return false;
            }
            return true;
        }
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

        int IArith<T>.Axes => Default.Axes;
        float IArith<T>.GetAxis(in T a, int i) => Default.GetAxis(a, i);
        void IArith<T>.SetAxis(ref T a, int i, float value) => Default.SetAxis(ref a, i, value);
    }
    [Impl]
    public struct IntArith : IArith<int>
    {
        int IArith<int>.Axes => 1;
        float IArith<int>.GetAxis(in int a, int _) => a;
        void IArith<int>.SetAxis(ref int a, int _, float value) => a = (int)value;
    }
    [Impl]
    public struct FloatArith : IArith<float>
    {
        int IArith<float>.Axes => 1;
        float IArith<float>.GetAxis(in float a, int _) => a;
        void IArith<float>.SetAxis(ref float a, int _, float value) => a = (float)value;
    }
}