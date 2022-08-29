using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    [AddComponentMenu("BDUtil/Ping Listener")]
    public class PingListener : MonoBehaviour
    {
        [Serializable]
        public abstract class Entry { public abstract void Subscribe(UnityEvent<ITopic, object> all, ref Dispose.All unsubscribe); }
        [Serializable]
        protected sealed class VoidEntry : Entry
        {
            public Holder<Ping> Ping;
            public UnityEvent Event;
            public override void Subscribe(UnityEvent<ITopic, object> all, ref Dispose.All unsubscribe)
            {
                Ping.Value.Subscribe(Event.Invoke, ref unsubscribe);
                Ping.Value.Subscribe(() => all.Invoke(Ping.Value, null), ref unsubscribe);
            }
        }
        [Serializable]
        public abstract class Entry<T> : Entry
        {
            public Holder<Ping<T>> Ping;
            public UnityEvent<T> Event;
            public override void Subscribe(UnityEvent<ITopic, object> all, ref Dispose.All unsubscribe)
            {
                Ping.Value.Subscribe(Event.Invoke, ref unsubscribe);
                Ping.Value.Subscribe((t) => all.Invoke(Ping.Value, t), ref unsubscribe);
            }
        }
        [Serializable] protected sealed class FloatEntry : Entry<float> { }
        [Serializable] protected sealed class GameObjectEntry : Entry<GameObject> { }
        [Serializable] protected sealed class StringEntry : Entry<string> { }
        [Serializable] protected sealed class DataEntry : Entry<IDictionary<string, object>> { }

        [SerializeReference, Subtype] List<Entry> Entries = new();
        public UnityEvent<ITopic, object> SubscribeAll = new();
        Dispose.All unsubscribe = new();

        [SuppressMessage("IDE", "IDE0051")]
        void OnEnable()
        {
            foreach (Entry entry in Entries) entry.Subscribe(SubscribeAll, ref unsubscribe);
        }
        [SuppressMessage("IDE", "IDE0051")]
        void OnDisable() => unsubscribe.Dispose();
    }
}