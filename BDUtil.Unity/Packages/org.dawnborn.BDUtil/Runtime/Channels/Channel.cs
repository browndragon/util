using System;
using System.Diagnostics.CodeAnalysis;
using BDUtil;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Channels
{
    [CreateAssetMenu(menuName = "BDUtil/Channel")]
    public class Channel : ScriptableObject
    {
        public event Action Action;
        [SuppressMessage("IDE", "IDE0051")]
        void OnEnable() => Action = null;
        [SuppressMessage("IDE", "IDE0051")]
        void OnDisable() => Action = null;
        public void Invoke() => Action?.Invoke();
    }
    public abstract class Channel<T> : ScriptableObject
    {
        public event Action<T> Action;
        [SuppressMessage("IDE", "IDE0051")]
        void OnEnable() => Action = null;
        [SuppressMessage("IDE", "IDE0051")]
        void OnDisable() => Action = null;
        public void Invoke(T t) => Action?.Invoke(t);
    }
    public abstract class Channel<T1, T2> : ScriptableObject
    {
        public event Action<T1, T2> Action;
        [SuppressMessage("IDE", "IDE0051")]
        void OnEnable() => Action = null;
        [SuppressMessage("IDE", "IDE0051")]
        void OnDisable() => Action = null;
        public void Invoke(T1 t1, T2 t2) => Action?.Invoke(t1, t2);
    }

    /// Convenience methods that make it easier to sub/unsub from a channel.
    public static class Channels
    {
        public static void Subscribe(this Channel thiz, Action subscriber, Disposes.All unsubscribe)
        {
            thiz.Action += subscriber;
            unsubscribe.Add(() => thiz.Action -= subscriber);
        }
        public static void Subscribe(this Channel thiz, UnityEvent subscriber, Disposes.All unsubscribe)
        {
            thiz.Action += subscriber.Invoke;
            unsubscribe.Add(() => thiz.Action -= subscriber.Invoke);
        }

        public static void Subscribe<T>(this Channel<T> thiz, Action<T> subscriber, Disposes.All unsubscribe)
        {
            thiz.Action += subscriber;
            unsubscribe.Add(() => thiz.Action -= subscriber);
        }
        public static void Subscribe<T>(this Channel<T> thiz, UnityEvent<T> subscriber, Disposes.All unsubscribe)
        {
            thiz.Action += subscriber.Invoke;
            unsubscribe.Add(() => thiz.Action -= subscriber.Invoke);
        }

        public static void Subscribe<T1, T2>(this Channel<T1, T2> thiz, Action<T1, T2> subscriber, Disposes.All unsubscribe)
        {
            thiz.Action += subscriber;
            unsubscribe.Add(() => thiz.Action -= subscriber);
        }
        public static void Subscribe<T1, T2>(this Channel<T1, T2> thiz, UnityEvent<T1, T2> subscriber, Disposes.All unsubscribe)
        {
            thiz.Action += subscriber.Invoke;
            unsubscribe.Add(() => thiz.Action -= subscriber.Invoke);
        }
    }
}