
using System;
using System.Collections;
using UnityEngine;

namespace BDUtil
{
    public static class Coroutines
    {
        // How horrible! But... it works...
        public static Coroutine StartCoroutine(IEnumerator coroutine) => Ticker.main.StartCoroutine(coroutine);
        // How horrible! But... it works...
        public static Coroutine StartCoroutine(IEnumerable coroutine) => Ticker.main.StartCoroutine(coroutine.GetEnumerator());

        public static WaitForSeconds Seconds(float seconds) => new(seconds);
        public static WaitForSecondsRealtime SecondsRealtime(float seconds) => new(seconds);
        public static WaitUntil Until(Func<bool> predicate) => new(predicate);
        public static WaitWhile While(Func<bool> predicate) => new(predicate);
        public const YieldInstruction Next = null;
        public static readonly WaitForFixedUpdate Fixed = new();
        public static readonly WaitForEndOfFrame End = new();

        public static Coroutine Schedule(Action action, YieldInstruction instruction = Next)
        {
            IEnumerator afterDelay()
            {
                yield return instruction;
                action();
            }
            return StartCoroutine(afterDelay());
        }
    }
}