using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Subscriber")]
    public class Subscriber : MonoBehaviour
    {
        public interface ISubscribe { IDisposable Subscribe(Subscriber thiz); }

        [SerializeReference, Subtype] List<ISubscribe> Subscribes = new();
        Disposes.All unsubscribe = new();

        void OnEnable() { foreach (ISubscribe subscribe in Subscribes) unsubscribe.Add(subscribe.Subscribe(this)); }
        void OnDisable() => unsubscribe.Dispose();
    }

    public static class Subscribers
    {
        [Serializable]
        internal class MessageEvent : Subscriber.ISubscribe
        {
            public Topic Topic;
            public bool SendUpwards;
            public string MessageName;
            public SendMessageOptions SendMessageOptions;
            public IDisposable Subscribe(Subscriber thiz)
            {
                void SendMessage() => thiz.SendMessage(MessageName, Topic, SendMessageOptions);
                void SendMessageUpwards() => thiz.SendMessageUpwards(MessageName, Topic, SendMessageOptions);
                return Topic?.Subscribe(SendUpwards ? SendMessageUpwards : SendMessage);
            }
        }
        [Serializable]
        internal class UEvent : Subscriber.ISubscribe
        {
            public Topic Topic;
            public UnityEvent Event;
            public IDisposable Subscribe(Subscriber thiz) => Topic?.Subscribe(Event.Invoke);
        }

        [Serializable]
        internal class UEventSelf : Subscriber.ISubscribe
        {
            public Topic Topic;
            public UnityEvent<Topics.IJoinable> Event;
            public IDisposable Subscribe(Subscriber thiz) => Topic?.Subscribe(() => Event.Invoke(Topic));
        }
        [Serializable]
        public abstract class UEvent<T> : Subscriber.ISubscribe
        {
            public Topic<T> Topic;
            public UnityEvent<T> Event;
            public IDisposable Subscribe(Subscriber thiz) => Topic?.Subscribe(Event.Invoke);
        }
        public abstract class UEventSelf<T> : Subscriber.ISubscribe
        {
            public Topic<T> Topic;
            public UnityEvent<Topic, T> Event;
            public IDisposable Subscribe(Subscriber thiz) => Topic?.Subscribe(t => Event.Invoke(Topic, t));
        }
    }
}