using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Subscriber")]
    public class Subscriber : MonoBehaviour
    {
        public interface ISubscribe { Action Subscribe(Subscriber thiz); }

        [SuppressMessage("IDE", "IDE0044")]
        [SerializeReference, Subtype]
        List<ISubscribe> Subscribes = new();
        [SuppressMessage("IDE", "IDE0044")]
        Disposes.All unsubscribe = new();

        [SuppressMessage("IDE", "IDE0051")]
        void OnEnable() { foreach (ISubscribe subscribe in Subscribes) unsubscribe.Add(subscribe.Subscribe(this)); }
        [SuppressMessage("IDE", "IDE0051")]
        void OnDisable() => unsubscribe.Dispose();
    }

    public static class Subscribers
    {
        [Serializable]
        internal class MessageEvent : Subscriber.ISubscribe
        {
            [Tooltip("The topic on which to listen for updates")]
            public Topic Topic;
            public bool SendValue = true;
            public Sender Sender;

            public Action Subscribe(Subscriber thiz) => Topic?.Subscribe(() => Sender.Send(thiz, SendValue && Topic is ObjectTopic o ? o.Object : null));
        }

        [Serializable]
        internal class UEvent : Subscriber.ISubscribe
        {
            public Topic Topic;
            public UnityEvent Event = new();
            public Action Subscribe(Subscriber thiz) => Topic?.Subscribe(Event.Invoke);
        }
        [Serializable]
        internal class UEventTopic : Subscriber.ISubscribe
        {
            public Topic Topic;
            public UnityEvent<ITopic> Event = new();
            public Action Subscribe(Subscriber thiz) => Topic?.Subscribe(Event.Invoke);
        }
        [Serializable]
        internal class UEventObject : Subscriber.ISubscribe
        {
            public ObjectTopic Topic;
            public UnityEvent<object> Event = new();
            public Action Subscribe(Subscriber thiz) => Topic?.Subscribe(InvokeEvent);
            void InvokeEvent() => Event.Invoke(Topic.Object);
        }
        public abstract class UEventValue<T> : Subscriber.ISubscribe
        {
            public Topic<T> Topic;
            public UnityEvent<T> Event;
            public Action Subscribe(Subscriber thiz) => Topic?.Subscribe(Event.Invoke);
        }
    }
}