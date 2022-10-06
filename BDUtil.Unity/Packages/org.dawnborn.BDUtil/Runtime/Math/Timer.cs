using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil.Math
{
    [Serializable]
    public readonly struct Tick
    {
        public readonly float Passed;
        public readonly float Length;
        public Tick(float passed, float length)
        {
            Passed = passed;
            Length = length;
        }
        public void Deconstruct(out float passed, out float length)
        {
            passed = Passed;
            length = Length;
        }
        public bool IsStarted => float.IsFinite(Passed);
        public bool IsLive => Passed.IsInRange(0, Length);
        public static implicit operator bool(in Tick t) => t.IsLive;
        public float FullRatio => Length > 0 ? Passed / Length : float.PositiveInfinity;
        public float Ratio => Mathf.Min(1f, FullRatio);
        public static implicit operator float(in Tick t) => t.FullRatio;

        public override string ToString() => $"{Passed}/{Length}s";
    }

    [Serializable]
    public struct Timer : IEnumerable<Tick>, IEnumerator<Tick>
    {
        public float Length;
        public float Start;
        public float End => Start + Length;
        public float Delta => Time.deltaTime;
        public Tick Tick => new(Time.time - Start, Length);
        public bool IsStarted => float.IsFinite(Start);
        public static implicit operator Timer(float length) => new(length, float.NaN);
        public static implicit operator Tick(Timer timer) => timer.Tick;
        public static implicit operator float(Timer timer) => timer.Tick;
        public static implicit operator bool(Timer timer) => timer.Tick;
        public Timer(float length, float start = default)
        {
            Length = length;
            Start = (start == default) ? Time.time : start;
        }
        public IEnumerator Foreach(Action<Tick> onTick, Action onComplete = default)
        {
            foreach (var tick in this)
            {
                onTick(tick);
                yield return null;
            }
            onTick(new(Length, Length));
            onComplete?.Invoke();
        }
        public Timer Stopped()
        {
            Start = float.NaN;
            return this;
        }
        public Timer Started()
        {
            Start = Time.time;
            return this;
        }
        public void Reset() => this = Started();
        public bool MoveNext() => Tick;
        public Timer GetEnumerator() => this;
        public void Dispose() { }
        public Tick Current => this;
        object IEnumerator.Current => this;
        IEnumerator<Tick> IEnumerable<Tick>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }
    [Serializable]
    public struct FixedTimer : IEnumerable<Tick>, IEnumerator<Tick>
    {
        public float Length;
        public float Start;
        public float End => Start + Length;
        public float Delta => Time.fixedDeltaTime;
        public Tick Tick => new(Time.fixedTime - Start, Length);
        public bool IsStarted => float.IsFinite(Start);
        public static implicit operator FixedTimer(float length) => new(length, float.NaN);
        public static implicit operator Tick(FixedTimer timer) => timer.Tick;
        public static implicit operator float(FixedTimer timer) => timer.Tick;
        public static implicit operator bool(FixedTimer timer) => timer.Tick;
        public FixedTimer(float length, float start = default)
        {
            Length = length;
            Start = (start == default) ? Time.fixedTime : start;
        }
        public IEnumerator Foreach(Action<Tick> onTick, Action onComplete = default)
        {
            foreach (var tick in this)
            {
                onTick(tick);
                yield return Coroutines.Fixed;
            }
            onTick(new(Length, Length));
            onComplete?.Invoke();
        }
        public FixedTimer Stopped()
        {
            Start = float.NaN;
            return this;
        }
        public FixedTimer Started()
        {
            Start = Time.time;
            return this;
        }
        public void Reset() => this = Started();
        public bool MoveNext() => Tick;
        public FixedTimer GetEnumerator() => this;
        public void Dispose() { }
        public Tick Current => this;
        object IEnumerator.Current => this;
        IEnumerator<Tick> IEnumerable<Tick>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}
