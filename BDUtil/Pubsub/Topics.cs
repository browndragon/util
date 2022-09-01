
using System;

namespace BDUtil.Pubsub
{
    namespace Raw
    {
        /// Basic empty topic. It happens to notify with itself.
        /// this is only really useful in tests, since it's not *even* a SOTopic.
        public abstract class Topic : ITopic, IHas, IPublisher
        {
            event Action<IHas> Actions;
            public bool HasValue => true;
            public object Value => this;
            public Disposes.One Subscribe(ISubscriber observer)
            {
                Action<IHas> observe = observer.Observe;
                Actions += observe;
                return new(() => Actions -= observe);
            }
            IDisposable ITopic.Subscribe(ISubscriber observer) => Subscribe(observer);
            public virtual void Publish() => Actions?.Invoke(this);
        }
        public sealed class VoidTopic : Topic { }
        /// A significantly more useful topic: supports some data T. Each update to T causes a republish.
        public abstract class Topic<T> : Topic, ITopic<T>, IHas<T>
        {
            T IHas<T>.Value => throw new NotImplementedException();
            event Action<IHas<T>> Actions;
            public Disposes.One Subscribe(ISubscriber<T> observer)
            {
                Action<IHas<T>> observe = observer.Observe;
                Actions += observe;
                return new(() => Actions -= observe);
            }
            IDisposable ITopic<T>.Subscribe(ISubscriber<T> observer) => Subscribe(observer);
            public override void Publish()
            {
                base.Publish();
                Actions?.Invoke(this);
            }
        }
        public sealed class ValueTopic<T> : Topic<T>, IPublisher<T>, IValue<T>
        where T : struct
        {
            T value;
            public new T Value
            {
                get => value;
                set
                {
                    this.value = value;
                    Publish();
                }
            }
            public void Publish(T value) => Value = value;
        }
        public class SelfTopic<TSelf> : Topic<TSelf>
        where TSelf : SelfTopic<TSelf>
        {
            public new TSelf Value => (TSelf)this;
        }

        /// A significantly more useful topic: supports some data T with different inside & outside views.
        /// It can only be modified in the context of an Update block; at the conclusion, it publishes.
        public abstract class MutableTopic<T, TRO, TRW> : SelfTopic<T>, IMutate<TRO, TRW>
        where T : MutableTopic<T, TRO, TRW>
        {
            public new TRO Value { get; protected set; }
            TRW Scope.IScopable<TRW>.OnceBegun => (TRW)(object)Value;
            Lock Lock = default;
            public bool IsMutating => Lock;

            void Scope.IScopable.Begin() => Lock++;
            void Scope.IScopable.End()
            {
                bool wasLocked = Lock;
                Lock--;
                if (wasLocked && !Lock) Publish();
            }
        }
    }
}