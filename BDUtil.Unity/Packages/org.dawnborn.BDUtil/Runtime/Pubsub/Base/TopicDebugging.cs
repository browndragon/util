using System;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Pubsub
{
    public class TopicDebugging : StaticAsset<TopicDebugging>
    {
        [Flags]
        public enum LogEvents
        {
            None = default,
            OnAddListener = 1 << 0,
            OnRemoveListener = 1 << 1,
            OnRemoveAllListeners = 1 << 2,
            OnEnable = 1 << 3,
            OnDisable = 1 << 4,
            OnPublish = 1 << 5,
        }
        public LogEvents Default = Enums<LogEvents>.Everything;
        public StoreMap<Topic, LogEvents> Overrides = new();
        LogEvents LogEvent(Topic topic) => Overrides.Collection.TryGetValue(topic, out var logEvent) ? logEvent : Default;
        public void LogOnAddListener(Topic topic, Action action)
        {
            if (!LogEvent(topic).HasFlag(LogEvents.OnAddListener)) return;
            Debug.Log($"{topic}+={action?.Target}[{action?.Method?.Name}]()");
        }
        public void LogOnRemoveListener(Topic topic, Action action)
        {
            if (!LogEvent(topic).HasFlag(LogEvents.OnRemoveListener)) return;
            Debug.Log($"{topic}-={action?.Target}[{action?.Method?.Name}]()");
        }
        public void LogOnRemoveAllListeners(Topic topic)
        {
            if (!LogEvent(topic).HasFlag(LogEvents.OnRemoveAllListeners)) return;
            Debug.Log($"{topic}=null");
        }
        public void LogOnPublish(Topic topic)
        {
            if (!LogEvent(topic).HasFlag(LogEvents.OnPublish)) return;
            Debug.Log($"{topic}.Publish()");
        }
        public void LogOnEnable(Topic topic)
        {
            if (!LogEvent(topic).HasFlag(LogEvents.OnEnable)) return;
            Debug.Log($"{topic}.OnEnable()");
        }
        public void LogOnDisable(Topic topic)
        {
            if (!LogEvent(topic).HasFlag(LogEvents.OnDisable)) return;
            Debug.Log($"{topic}.OnDisable()");
        }
        public void LogUnityObject(UnityEngine.Object @object) => Debug.Log(@object, this);
        public void LogCObject(object @object) => Debug.Log(@object, this);
    }
}