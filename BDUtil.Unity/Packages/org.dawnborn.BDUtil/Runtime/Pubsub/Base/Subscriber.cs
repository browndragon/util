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
        public struct SendMessageConfig
        {
            public static readonly SendMessageConfig Default = new() { SendUpwards = false, SendMessageOptions = SendMessageOptions.DontRequireReceiver };
            public bool SendUpwards;
            public SendMessageOptions SendMessageOptions;
        }
        [Serializable]
        internal class MessageEvent : Subscriber.ISubscribe
        {
            [Tooltip("The topic on which to listen for updates")]
            public Topic Topic;
            [Tooltip("The `this.SendMessage(\"MessageName\", topic, config)`")]
            public string MessageName;
            public SendMessageConfig Config = SendMessageConfig.Default;

            public Action Subscribe(Subscriber thiz)
            {
                void SendMessage() => thiz.SendMessage(MessageName, Topic, Config.SendMessageOptions);
                void SendMessageUpwards() => thiz.SendMessageUpwards(MessageName, Topic, Config.SendMessageOptions);
                return Topic?.Subscribe(Config.SendUpwards ? SendMessageUpwards : SendMessage);
            }
        }
        [Serializable]
        internal class MessageValueEvent : Subscriber.ISubscribe
        {
            [Tooltip("The topic on which to listen for updates")]
            public ObjectTopic Topic;
            [Tooltip("The `this.SendMessage(\"MessageName\", topic.Object, config)`")]
            public string MessageName;
            public SendMessageConfig Config = SendMessageConfig.Default;
            public Action Subscribe(Subscriber thiz)
            {
                void SendMessage() => thiz.SendMessage(MessageName, Topic.Object, Config.SendMessageOptions);
                void SendMessageUpwards() => thiz.SendMessageUpwards(MessageName, Topic.Object, Config.SendMessageOptions);
                return Topic?.Subscribe(Config.SendUpwards ? SendMessageUpwards : SendMessage);
            }
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