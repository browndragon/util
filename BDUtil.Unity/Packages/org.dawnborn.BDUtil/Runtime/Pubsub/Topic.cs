using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    public abstract class Topic : ScriptableObject, ITopic, IPublisher, IHas
    {
        [SerializeField, Subtype(PrintDebug = true)] protected Subtype<ICollection<IBaseSubscriber>> Storage = typeof(List<IBaseSubscriber>);
        protected ICollection<IBaseSubscriber> Subscribers;
        bool IHas.HasValue => false;
        object IHas.Value => null;

        protected virtual void OnEnable() => Subscribers = Storage.CreateInstance();
        protected virtual void OnDisable() => Subscribers = null;
        protected virtual bool Tell(IBaseSubscriber subscriber)
        {
            switch (subscriber)
            {
                case null: return true;
                case ISubscriber sub: sub.Observe(this); return true;
                default: return false;
            }
        }

        public void Publish()
        { foreach (IBaseSubscriber subscriber in Subscribers) Tell(subscriber).OrThrow($"Unhandled subscriber {subscriber}"); }

        protected Disposes.Remove<IBaseSubscriber> Subscribe(IBaseSubscriber observer)
        {
            Subscribers.Add(observer);
            return new(Subscribers, observer);
        }
        public Disposes.Remove<IBaseSubscriber> Subscribe(ISubscriber observer) => Subscribe((IBaseSubscriber)observer);
        IDisposable ITopic.Subscribe(ISubscriber observer) => Subscribe(observer);

        [OnChange(nameof(Publish), AsButton = true)]
        [SerializeField]
        [SuppressMessage("IDE", "IDE0044")]
        bool TriggerPublish;
    }
    public abstract class Topic<T> : Topic, ITopic<T>, IHas<T>
    {
        object IHas.Value => ((IHas<T>)this).Value;
        T IHas<T>.Value => throw new NotImplementedException();
        public Disposes.Remove<IBaseSubscriber> Subscribe(ISubscriber<T> observer) => Subscribe((IBaseSubscriber)observer);
        IDisposable ITopic<T>.Subscribe(ISubscriber<T> observer) => Subscribe(observer);
        protected override bool Tell(IBaseSubscriber subscriber)
        {
            if (base.Tell(subscriber)) return true;
            switch (subscriber)
            {
                case ISubscriber<T> sub: sub.Observe(this); return true;
                default: return false;
            }
        }
    }
}