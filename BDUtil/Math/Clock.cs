using System;

namespace BDUtil.Math
{
    /// A unity-specific clock that masquerades as not-that.
    /// We need it here because if we want Delay and Cooldown and TokenBucket (etc) to serialize,
    /// the notion of a clock needs to serialize. But those are such basic math! Alas, alas.
    public enum Clock
    {
        Now = default,
        FixedNow,
        Realtime,
        Test,
    }
    public static class Clocks
    {
        public static void Init(Func<float> now, Func<float> fixedNow, Func<float> realtime)
        {
            Now = now;
            FixedNow = fixedNow;
            Realtime = realtime;
        }
        static Func<float> Now;
        static Func<float> FixedNow;
        static Func<float> Realtime;
        public static float Test { get; set; } = 1234f;
        public static float GetTime(this Clock thiz) => thiz switch
        {
            Clock.Now => Now.Invoke(),
            Clock.FixedNow => FixedNow.Invoke(),
            Clock.Realtime => Realtime.Invoke(),
            Clock.Test => Test,
            _ => throw thiz.BadValue(),
        };
        public static Delay StoppedDelayOf(this Clock thiz, float length) => new(length, float.NaN, thiz);
    }
}