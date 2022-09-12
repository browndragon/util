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
            public Topic Topic;
            public bool SendUpwards;
            public string MessageName;
            public SendMessageOptions SendMessageOptions;
            public Action Subscribe(Subscriber thiz)
            {
                void SendMessage() => thiz.SendMessage(MessageName, Topic, SendMessageOptions);
                void SendMessageUpwards() => thiz.SendMessageUpwards(MessageName, Topic, SendMessageOptions);
                return Topic?.Subscribe(SendUpwards ? SendMessageUpwards : SendMessage);
            }
        }
        [Serializable]
        internal class MessageValueEvent : Subscriber.ISubscribe
        {
            public ObjectTopic Topic;
            public bool SendUpwards;
            public string MessageName;
            public SendMessageOptions SendMessageOptions;
            public Action Subscribe(Subscriber thiz)
            {
                void SendMessage() => thiz.SendMessage(MessageName, Topic.Object, SendMessageOptions);
                void SendMessageUpwards() => thiz.SendMessageUpwards(MessageName, Topic.Object, SendMessageOptions);
                return Topic?.Subscribe(SendUpwards ? SendMessageUpwards : SendMessage);
            }
        }
        [Serializable]
        internal class LogEvent : Subscriber.ISubscribe
        {
            public Topic Topic;
            Subscriber Thiz;
            void Log() => Debug.Log($"{Thiz}: received {Topic}", Thiz);
            public Action Subscribe(Subscriber thiz)
            {
                Thiz = thiz;
                return Topic?.Subscribe(Log);
            }
        }
        [Serializable]
        internal class InvokeEvent : Subscriber.ISubscribe
        {
            public Topic<Action> Topic;
            public Action Subscribe(Subscriber thiz) => Topic?.Subscribe(a => a?.Invoke());
        }
        [Serializable]
        internal class DestroyEvent : Subscriber.ISubscribe
        {
            public Topic<GameObject> Topic;
            public Action Subscribe(Subscriber thiz) => Topic?.Subscribe(go => UnityEngine.Object.Destroy(go));
        }
        [Serializable]
        internal class UEvent : Subscriber.ISubscribe
        {
            public Topic Topic;
            public UnityEvent Event = new();
            public Action Subscribe(Subscriber thiz) => Topic?.Subscribe(Event.Invoke);
        }

        [Serializable]
        internal class UEventSelf : Subscriber.ISubscribe
        {
            public Topic Topic;
            public UnityEvent<Topic> Event = new();
            public Action Subscribe(Subscriber thiz) => Topic?.Subscribe(() => Event.Invoke(Topic));
        }
        [Serializable]
        public abstract class UEvent<T> : Subscriber.ISubscribe
        {
            public Topic<T> Topic;
            public UnityEvent<T> Event;
            public Action Subscribe(Subscriber thiz) => Topic?.Subscribe(Event.Invoke);
        }
        [Serializable]
        public class UEventSelf<T> : Subscriber.ISubscribe
        {
            public Topic<T> Topic;
            public UnityEvent<Topic, T> Event;
            public Action Subscribe(Subscriber thiz) => Topic?.Subscribe(t => Event.Invoke(Topic, t));
        }
    }
}