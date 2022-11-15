using System;

namespace BDUtil.Math
{
    /// A source of tokens with a rate.
    /// Usually, the rate refills and you atomically Take.
    /// However, you might see this go the other way where the rate drains and you atomically Put.
    /// This class doesn't support events or anything, so you'd need to poll in that case.
    [Serializable]
    public struct TokenBucket
    {
        public static readonly TokenBucket Empty = 0f;
        public static implicit operator TokenBucket(float f) => new() { Bounds = new(0f, float.PositiveInfinity), Rate = f, PrevTime = float.NaN };
        public static implicit operator TokenBucket((float init, float rate) t) => new() { Bounds = new(0f, float.PositiveInfinity), Rate = t.rate, PrevValue = t.init, PrevTime = float.NaN };

        public Interval Bounds;
        public Clock Clock;
        public float Rate;
        /// Also "initial value", fyi.
        public float PrevValue;
        public float PrevTime;
        public float PrevAvailable => System.Math.Max(PrevValue - Bounds.min, 0);

        public float GetValueAt(float time)
        {
            if (float.IsNaN(PrevTime)) return Bounds.GetClampedPoint(PrevValue);
            float elapsed = time - PrevTime;
            return Bounds.GetClampedPoint(PrevValue + Rate * elapsed);
        }
        public float UpdateValueAt(float time)
        {
            PrevValue = GetValueAt(time);
            PrevTime = time;
            return PrevValue;
        }
        public float UpdateValue() => UpdateValueAt(Clock.GetTime());
        public void ResetValues(float time, float velocity = float.NaN, float value = float.NaN)
        {
            if (float.IsNaN(time)) time = Clock.GetTime();
            PrevTime = time;
            if (!float.IsNaN(velocity)) Rate = velocity;
            if (!float.IsNaN(value)) PrevValue = value;
        }
        public float Put(float amount) => PrevValue = Bounds.GetClampedPoint(UpdateValue() + amount);
        public bool Take(float amount) => TakeMaxMin(amount, amount) >= amount;
        /// Takes up to max, but at least min, from `this` (returning real taken value, which is also capped by the Bounds.min).
        public float TakeMaxMin(float max, float min = 0f)
        {
            if (min < 0) min = 0f;
            max = System.Math.Min(max, UpdateValue() - Bounds.min);
            if (max < min) return 0f;
            PrevValue -= max;
            return max;
        }
        public static float TakeMaxMin(float max, float min, ref TokenBucket a, ref TokenBucket b)
        {
            if (min < 0) min = 0f;
            a.UpdateValue();
            b.UpdateValue();
            max = System.Math.Min(max, a.PrevAvailable);
            max = System.Math.Min(max, b.PrevAvailable);
            if (max < min) return 0f;
            a.PrevValue -= max;
            b.PrevValue -= max;
            return max;
        }
        public void SetRateNow(float rate)
        {
            UpdateValue();
            Rate = rate;
        }
    }
}