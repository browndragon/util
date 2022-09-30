using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

namespace BDUtil.Math
{
    [Serializable]
    public readonly struct Tick
    {
        public readonly float Elapsed;
        public readonly float Length;
        public Tick(float elapsed, float length)
        {
            Elapsed = elapsed;
            Length = length;
        }
        public void Deconstruct(out float elapsed, out float length)
        {
            elapsed = Elapsed;
            length = Length;
        }
        public float Ratio => Mathf.Min(1f, FullRatio);
        public float FullRatio => Elapsed / Length;
        public static implicit operator float(in Tick t) => t.Ratio;
        public override string ToString() => $"{Elapsed}/{Length}s";
    }
    public struct Timer : IEnumerable<Tick>, IEnumerator<Tick>
    {
        public float Length;
        public float Start;
        public float End => Start + Length;
        public float Delta => Time.deltaTime;
        public Tick Elapsed => new(Time.time - Start, Length);
        public static implicit operator Tick(Timer timer) => new(timer.Elapsed, timer.Length);
        public bool IsStarted => float.IsFinite(Start) && Start != default;
        public bool IsRunning => IsStarted && Elapsed < Length;
        public static implicit operator bool(Timer timer) => timer.IsRunning;
        public static implicit operator Timer(float t) => new(t, false);
        public Timer(float length, bool reset = true)
        {
            Length = length;
            if (reset) Start = Time.time;
            else Start = float.NegativeInfinity;
        }
        public IEnumerator Foreach(Action<Tick> onTick)
        {
            foreach (var tick in this)
            {
                onTick(tick);
                yield return null;
            }
            onTick(new(Length, Length));
        }
        public Timer Restart()
        {
            Reset();
            return this;
        }
        public Timer Halt()
        {
            Start = float.NegativeInfinity;
            return this;
        }

        public void Reset() => Start = Time.time;

        public bool MoveNext() => IsRunning;
        public Timer GetEnumerator() => this;
        public void Dispose() { }
        public Tick Current => this;
        object IEnumerator.Current => this;
        IEnumerator<Tick> IEnumerable<Tick>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }
    public struct FixedTimer : IEnumerable<Tick>, IEnumerator<Tick>
    {
        public float Length;
        public float Start;
        public float End => Start + Length;
        public float Delta => Time.fixedDeltaTime;
        public Tick Elapsed => new(Time.fixedTime - Start, Length);
        public static implicit operator Tick(FixedTimer timer) => new(timer.Elapsed, timer.Length);
        public bool IsStarted => float.IsFinite(Start) && Start != default;
        public bool IsRunning => IsStarted && Elapsed <= Length;
        public static implicit operator bool(FixedTimer timer) => timer.IsRunning;
        public static implicit operator FixedTimer(float t) => new(t, false);
        public FixedTimer(float length, bool reset = true)
        {
            Length = length;
            if (reset) Start = Time.fixedTime;
            else Start = float.NegativeInfinity;
        }
        static readonly WaitForFixedUpdate waitForFixedUpdate = new();
        public IEnumerator Foreach(Action<Tick> onTick)
        {
            foreach (var tick in this)
            {
                onTick(tick);
                yield return waitForFixedUpdate;
            }
            onTick(new(Length, Length));
        }

        public FixedTimer Restart()
        {
            Reset();
            return this;
        }
        public FixedTimer Halt()
        {
            Start = float.NegativeInfinity;
            return this;
        }

        public void Reset() => Start = Time.time;

        public bool MoveNext() => IsRunning;
        public FixedTimer GetEnumerator() => this;
        public void Dispose() { }
        public Tick Current => this;
        object IEnumerator.Current => this;
        IEnumerator<Tick> IEnumerable<Tick>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}
