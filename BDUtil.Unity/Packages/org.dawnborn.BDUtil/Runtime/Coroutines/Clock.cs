using System;
using System.Collections;
using UnityEngine;
namespace BDUtil
{
    [Serializable]
    public enum Clock
    {
        Now = default,
        FixedNow,
        Realtime,
    }
    public static class Clocks
    {
        public static float GetTime(this Clock thiz) => thiz switch
        {
            Clock.Now => Time.time,
            Clock.FixedNow => Time.fixedTime,
            Clock.Realtime => Time.realtimeSinceStartup,
            _ => throw thiz.BadValue(),
        };
        public static YieldInstruction GetYield(this Clock thiz) => thiz switch
        {
            Clock.Now => Coroutines.Next,
            Clock.FixedNow => Coroutines.Fixed,
            Clock.Realtime => Coroutines.Next,
            _ => throw thiz.BadValue(),
        };

        public static Delay StoppedDelayOf(this Clock thiz, float delay)
        => new(delay, float.NaN, thiz);
    }
}