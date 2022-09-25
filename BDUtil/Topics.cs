using System;
using BDUtil.Raw;

namespace BDUtil
{
    public interface ITopic
    {
        void AddListener(Action action);
        void RemoveListener(Action action);
    }
    public interface IPublisher
    {
        void Publish();
        void ClearAll();
    }
    public interface IObjectTopic : ITopic, IHas { }
    public interface ITopic<out T> : IObjectTopic, IHas<T> { }
    /// EXPLICITLY exposes a SetValue method, so that we can use it in unityevents.
    public interface IValueTopic<T> : ITopic<T>, IHas<T>, ISet<T>
    { void SetValue(T value); }

    /// Set or dictionary-type. Supports visibility/update by Value = Observable.Update; every change updates.
    public interface ICollectionTopic : IValueTopic<Observable.Update>, IHasCollection
    {
    }
    /// Set or dictionary-type. Supports visibility/update by Value = Observable.Update; every change updates.
    public interface ICollectionTopic<TColl> : IValueTopic<Observable.Update>, IHasCollection<TColl>
    where TColl : Observable.ICollection
    { }

    [Serializable]
    /// Standalone replacement for an `event Action`, which isn't a first-order entity...
    public class MemTopic : ITopic, IPublisher
    {
        public event Action Actions;

        public void AddListener(Action action) => Actions += action;
        public void ClearAll() => Actions = Actions?.UnsubscribeAll();
        public void Publish() => Actions?.Invoke();
        public void RemoveListener(Action action) => Actions -= action;
    }
    public static class Topics
    {
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
        /// Subscribes as long as the action returns true.
        public static void DoWhile(this ITopic thiz, Func<bool> action)
        {
            void handler() { if (!action()) thiz.RemoveListener(handler); }
            thiz.AddListener(handler);
        }
        public static Action Subscribe(this ITopic thiz, Action action)
        {
            thiz.AddListener(action);
            return () => thiz.RemoveListener(action);
        }
        public static Action Subscribe<T>(this ITopic thiz, Func<T> action)
        => thiz.Subscribe(() => { action(); });
        public static Action Subscribe<T>(this ITopic<T> thiz, Action<ITopic<T>> action)
        => thiz.Subscribe(() => action?.Invoke(thiz));
        public static Action Subscribe(this ITopic thiz, Action<ITopic> action)
        => thiz.Subscribe(() => action?.Invoke(thiz));
        public static Action Subscribe<T>(this ITopic<T> thiz, Action<T> action)
        => thiz.Subscribe(() => action?.Invoke(thiz.Value));
    }
}