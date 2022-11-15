using System;
using System.Collections;
using UnityEngine;
namespace BDUtil.Math
{
    public static class UnityClocks
    {
        static UnityClocks() => Init();
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        public static void Init() => Clocks.Init(Now, FixedNow, Realtime);
        static readonly Func<float> Now = () => Time.time;
        static readonly Func<float> FixedNow = () => Time.fixedTime;
        static readonly Func<float> Realtime = () => Time.realtimeSinceStartup;
        public static YieldInstruction GetYield(this Clock thiz) => thiz switch
        {
            Clock.Now => Coroutines.Next,
            Clock.FixedNow => Coroutines.Fixed,
            Clock.Realtime => Coroutines.Next,
            Clock.Test => Coroutines.Next,
            _ => throw thiz.BadValue(),
        };
        public static IEnumerator Foreach(this Delay thiz, Action<Delay.Tick> onTick, Action onComplete = default)
        {
            if (!thiz.HasStart) thiz.Reset();
            foreach (var tick in thiz)
            {
                onTick(tick);
                yield return thiz.Now.GetYield();
            }
            // And one more to get the value @ clamped 1 (technically a little past).
            onTick(thiz);
            onComplete?.Invoke();
        }
    }
}