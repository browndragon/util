using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace BDUtil.Math
{
    /// Foreachable duration.
    /// The loop returns the ratio of elapsed over total duration; the original duration object could always be inspected.
    /// `foreach (Timer t in new Timer(5f)) { fadeAlpha(t.Ratio); yield return null; }`
    [Serializable]
    public struct Timer : IEnumerable<Timer>, IEnumerator<Timer>
    {
        public float Length;
        // Weakly mutable: Reset (etc) operations change me.
        // Nothing else does! Which breaks the enumerable (etc) contracts a little:
        // Current can change if you suspend for some time; MoveNext can't be fastforwarded, etc.
        public float Start;

        public float End => Start + Length;
        public float Delta => Time.deltaTime;
        public float Elapsed => Time.time - Start;
        public float FullRatio => Elapsed / Length;
        public float Ratio => Mathf.Min(1f, FullRatio);
        public bool IsStarted => float.IsFinite(Start) && Start != default;
        public bool IsRunning => IsStarted && Elapsed <= Length;
        public static implicit operator float(in Timer t) => t.Ratio;
        public static implicit operator bool(in Timer t) => t.IsRunning;
        public static implicit operator Timer(float t) => new(t, false);

        public Timer(float length, bool reset = true)
        {
            Length = length;
            if (reset) Start = Time.time;
            else Start = float.NegativeInfinity;
        }
        public IEnumerator Foreach(Action<Timer> action)
        {
            foreach (var t in this)
            {
                try { action(t); } catch { yield break; }
                yield return null;
            }
            action(this);
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

        public Timer Current => this;
        object IEnumerator.Current => Current;
        IEnumerator<Timer> IEnumerable<Timer>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }

    /// Foreachable duration for physics.
    /// The loop returns the ratio of elapsed over total duration; the original duration object could always be inspected.
    /// `foreach (Timer t in new Timer(5f)) { fadeAlpha(t.Ratio); yield return null; }`
    public struct FixedTimer : IEnumerable<FixedTimer>, IEnumerator<FixedTimer>
    {
        public float Length;
        // Weakly mutable: Reset (etc) operations change me.
        // Nothing else does! Which breaks the enumerable (etc) contracts a little:
        // Current can change if you suspend for some time; MoveNext can't be fastforwarded, etc.
        public float Start;

        public float End => Start + Length;
        public float Delta => Time.fixedDeltaTime;
        public float Elapsed => Time.fixedTime - Start;
        public float FullRatio => Elapsed / Length;
        public float Ratio => Mathf.Min(1f, FullRatio);
        public bool IsStarted => float.IsFinite(Start) && Start != default;
        public bool IsRunning => IsStarted && Elapsed <= Length;
        public static implicit operator float(in FixedTimer t) => t.Ratio;
        public static implicit operator bool(in FixedTimer t) => t.IsRunning;
        public static implicit operator FixedTimer(float t) => new(t, false);

        public FixedTimer(float length, bool reset = true)
        {
            Length = length;
            if (reset) Start = Time.fixedTime;
            else Start = float.NegativeInfinity;
        }
        static readonly WaitForFixedUpdate waitForFixed = new();
        public IEnumerator Foreach(Action<FixedTimer> action)
        {
            foreach (var t in this)
            {
                try { action(t); } catch { yield break; }
                yield return waitForFixed;
            }
            action(this);
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

        public void Reset() => Start = Time.fixedTime;

        public bool MoveNext() => IsRunning;
        public FixedTimer GetEnumerator() => this;

        public void Dispose() { }

        public FixedTimer Current => this;
        object IEnumerator.Current => this;
        IEnumerator<FixedTimer> IEnumerable<FixedTimer>.GetEnumerator() => this;
        IEnumerator IEnumerable.GetEnumerator() => this;
    }
}