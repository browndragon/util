using System;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    public static class Subscriptions
    {
        public static void Add(this Disposes.All thiz, UnityAction action) => thiz.Add(() => action?.Invoke());

        public static Action Subscribe(this ITopic thiz, UnityEvent @event)
        => thiz.Subscribe(@event.Invoke);
        public static Action Subscribe(this ITopic thiz, UnityEvent<ITopic> @event)
        => thiz.Subscribe(@event.Invoke);
        public static Action Subscribe<T>(this ITopic<T> thiz, UnityEvent<T> @event)
        => thiz.Subscribe(@event.Invoke);
        public static Action Subscribe<T>(this ITopic<T> thiz, UnityEvent<ITopic<T>> @event)
        => thiz.Subscribe(@event.Invoke);

        /// For buttons etc.
        public static Action Subscribe(this UnityEvent thiz, UnityAction action)
        {
            thiz.AddListener(action);
            return () => thiz.RemoveListener(action);
        }
        public static Action Subscribe<T1>(this UnityEvent<T1> thiz, UnityAction<T1> action)
        {
            thiz.AddListener(action);
            return () => thiz.RemoveListener(action);
        }
        public static Action Subscribe<T1, T2>(this UnityEvent<T1, T2> thiz, UnityAction<T1, T2> action)
        {
            thiz.AddListener(action);
            return () => thiz.RemoveListener(action);
        }
        public static Action Subscribe<T1, T2, T3>(this UnityEvent<T1, T2, T3> thiz, UnityAction<T1, T2, T3> action)
        {
            thiz.AddListener(action);
            return () => thiz.RemoveListener(action);
        }
    }
}
