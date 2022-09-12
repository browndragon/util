using System;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    public static class Subscriptions
    {
        public static void Add(this Disposes.All thiz, UnityAction action) => thiz.Add(() => action?.Invoke());
        public static Action UnsubscribeAll(this Action thiz)
        {
            if (thiz == null) return null;
            foreach (Delegate d in thiz.GetInvocationList()) thiz -= (Action)d;
            return null;
        }
        public static Action<T> UnsubscribeAll<T>(this Action<T> thiz)
        {
            if (thiz == null) return null;
            foreach (Delegate d in thiz.GetInvocationList()) thiz -= (Action<T>)d;
            return null;
        }

        public static Action Subscribe(this ITopic thiz, Action action)
        {
            thiz.AddListener(action);
            return () => thiz.RemoveListener(action);
        }
        public static Action Subscribe(this ITopic thiz, UnityEvent @event)
        => thiz.Subscribe(@event.Invoke);
        public static Action Subscribe(this ITopic thiz, UnityAction<ITopic> action)
        => thiz.Subscribe(() => action?.Invoke(thiz));
        public static Action Subscribe(this ITopic thiz, UnityEvent<ITopic> @event)
        => thiz.Subscribe(@event.Invoke);
        public static Action Subscribe<T>(this ITopic<T> thiz, UnityAction<T> action)
        => thiz.Subscribe(() => action?.Invoke(thiz.Value));
        public static Action Subscribe<T>(this ITopic<T> thiz, UnityEvent<T> @event)
        => thiz.Subscribe(@event.Invoke);
        public static Action Subscribe<T>(this ITopic<T> thiz, UnityAction<ITopic<T>> action)
        => thiz.Subscribe(() => action?.Invoke(thiz));
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
