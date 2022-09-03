using System;

namespace BDUtil.Pubsub
{
    public static class Topics
    {
        public interface IJoinable
        {
            int Count { get; }
            void Clear();
            void Notify();
        }
        public interface IJoinable<T> : IJoinable
        {
            IDisposable Subscribe(T member);
        }

        public static IDisposable Subscribe(this IJoinable<Action> thiz, Action<IJoinable> member)
        => thiz.Subscribe(() => member(thiz));
        public static IDisposable Subscribe<T>(this IJoinable<Action<T>> thiz, Action<IJoinable, T> member)
        => thiz.Subscribe(t => member(thiz, t));
        public static IDisposable Subscribe<T>(this IJoinable<Action<T>> thiz, Action member)
        => thiz.Subscribe(t => member());
    }
}