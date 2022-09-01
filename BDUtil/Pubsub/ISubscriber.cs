using System;

namespace BDUtil.Pubsub
{
    public interface IBaseSubscriber { }
    public interface ISubscriber : IBaseSubscriber { void Observe(IHas value); }
    public interface ISubscriber<in T> : IBaseSubscriber { void Observe(IHas<T> value); }

    public readonly struct ActionSubscriber<T> : ISubscriber<T>
    {
        readonly Action<IHas<T>> Action;
        public ActionSubscriber(Action<IHas<T>> action) => Action = action.OrThrow();
        public void Observe(IHas<T> value) => Action(value);
    }

    public static class Observers
    {
        public static ActionSubscriber<T> FromAction<T>(this Action<IHas<T>> thiz) => new(thiz);
    }
}