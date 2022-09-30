using System;
using UnityEngine;
using UnityEngine.Serialization;

namespace BDUtil.Math
{
    [Serializable]
    public struct MinMax : IEquatable<MinMax>
    {
        public static readonly MinMax Unit = new(0f, 1f);
        public static readonly MinMax Symmetric = new(-1f, 1f);

        [FormerlySerializedAs("x")]
        public float Min;

        [FormerlySerializedAs("y")]
        public float Max;

        public float Width => Max - Min;
        public float Center => (Max + Min) / 2f;
        public bool IsValid => Min <= Max;
        public MinMax Ordered
        {
            get
            {
                MinMax @new = this;
                if (@new.Min > @new.Max)
                {
                    (@new.Min, @new.Max) = (@new.Max, @new.Min);
                }
                return @new;
            }
        }
        public MinMax(float min, float max)
        {
            Min = min;
            Max = max;
        }
        public static implicit operator Vector2(MinMax thiz) => new(thiz.Min, thiz.Max);
        public static implicit operator MinMax(Vector2 that) => new(that.x, that.y);

        public void Deconstruct(out float min, out float max)
        {
            min = Min;
            max = Max;
        }
        public bool Contains(MinMax other) => Min <= other.Min && other.Max <= Max;
        public bool Overlaps(MinMax other) => Min <= other.Max || other.Min <= Max;
        public bool Contains(float value) => value.IsInRange(Min, Max);

        public MinMax Union(MinMax value) => new(Mathf.Min(Min, value.Min), Mathf.Max(Max, value.Max));
        public MinMax Intersect(MinMax value) => new(Mathf.Max(Min, value.Min), Mathf.Min(Max, value.Max));
        /// Warning: these areas have to be contiguous, so this can surprise you!
        /// [-2,+2].Difference([-1,+1]) => [+1, -1], which is invalid, because there would be wingies left over.
        /// But: [-2, +2].Difference([-3, -1]) => [-1, +2], which is what you expect.
        public MinMax Difference(MinMax value) => new(Mathf.Max(Min, value.Max), Mathf.Min(Max, value.Min));
        public MinMax Union(float value) => new(Mathf.Min(Min, value), Mathf.Max(Max, value));

        public MinMax Offset(MinMax value) => new(Min + value.Min, Max + value.Max);
        public MinMax Offset(float value) => new(Min + value, Max + value);
        public static MinMax operator +(MinMax a, float b) => new(a.Min + b, a.Max + b);
        public static MinMax operator -(MinMax a, float b) => new(a.Min - b, a.Max - b);
        public static MinMax operator +(float b, MinMax a) => new(a.Min + b, a.Max + b);
        public MinMax Scale(MinMax value) => new(Min * value.Min, Max * value.Max);
        public MinMax Scale(float value) => new(Min * value, Max * value);
        public static MinMax operator *(MinMax a, float b) => new(a.Min * b, a.Max * b);
        public static MinMax operator /(MinMax a, float b) => new(a.Min / b, a.Max / b);
        public static MinMax operator *(float b, MinMax a) => new(a.Min * b, a.Max * b);

        public float Lerp(float value) => Min + value * Width;
        public float Unlerp(float value) => (value - Min) / Width;

        public float Random => UnityEngine.Random.Range(Min, Max);

        public bool Equals(MinMax other) => Min == other.Min && Max == other.Max;
        public override bool Equals(object obj) => obj is MinMax other && Equals(other);
        public static bool operator ==(MinMax a, MinMax b) => a.Equals(b);
        public static bool operator !=(MinMax a, MinMax b) => !a.Equals(b);

        public override int GetHashCode() => Chain.Hash ^ Min.GetHashCode() ^ Max.GetHashCode();
        public override string ToString() => $"[{Min},{Max}]";

        /// Controls this thing's layout.
        public class RangeAttribute : PropertyAttribute
        {
            public enum Displays
            {
                Slider = default,
                LogSlider,
                Vector2,
            }
            public Displays Display;
            public float Min = Unit.Min;
            public float Max = Unit.Max;
        }
    }
}