using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace BDUtil
{
    [Serializable]
    public struct Delay : IEnumerable<Delay.Tick>, IEnumerator<Delay.Tick>
    {
        public Clock Now;
        public float Start;
        public float Length;
        public readonly struct Tick
        {
            public readonly Delay Delay;
            internal Tick(in Delay delay) => Delay = delay;

            public bool IsStarted => Delay.Now.GetTime() >= Delay.Start;
            public bool IsOver => Delay.End < Delay.Now.GetTime();
            public float RatioUnclamped => Delay.Length > 0f ? Elapsed / Delay.Length : float.PositiveInfinity;
            public float Elapsed => Delay.Now.GetTime() - Delay.Start;
            public float Ratio => Mathf.Clamp(RatioUnclamped, 0f, 1f);
            public static implicit operator float(in Tick thiz) => thiz.Ratio;
            public static implicit operator bool(in Tick thiz) => thiz.IsStarted && !thiz.IsOver;
        }

        public float End => Start + Length;
        public bool IsEnded => End < Now.GetTime();
        public Tick Ratio => new(this);
        public static implicit operator Delay(float length) => new(length, float.NaN, default);
        public static implicit operator Tick(Delay timer) => timer.Ratio;
        public static implicit operator bool(Delay timer) => timer.Ratio;
        public Delay(float length, float start = default, Clock now = default)
        {
            Now = now;
            Length = length;
            Start = start;
            if (start == default) Reset();
        }
        public IEnumerator Foreach(Action<Tick> onTick, Action onComplete = default)
        {
            if (!HasStart) Reset();
            foreach (var tick in this)
            {
                onTick(tick);
                yield return Now.GetYield();
            }
            // And one more to get the value @ clamped 1.
            onTick(new(this));
            onComplete?.Invoke();
        }
        public bool HasStart => !float.IsNaN(Start);
        public void Stop() => Start = float.NaN;
        public void Reset() => Start = Now.GetTime();
        public void Reset(float length) { Length = length; Reset(); }

        public bool MoveNext() => Ratio;
        public Delay GetEnumerator() => this;
        public void Dispose() { }
        public Tick Current => Ratio;
        object IEnumerator.Current => Ratio;
        IEnumerator<Tick> IEnumerable<Tick>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}
