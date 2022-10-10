using System;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Fluent;
using BDUtil.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    [Serializable]
    public class Val : IDisposable
    {
        // Don't be fooled; if this is the wrong parent type, we won't use it.
        // TODO:
        // A custom editor will help ensure you only use valid types though.
        [SerializeField, Expandable] Topic topic;
        [SerializeField, SuppressMessage("IDE", "IDE0044")] UnityEvent Action = new();
        bool hasSubscribed = false;

        public Topic Topic
        {
            get
            {
                topic ??= MakeNewTopic();
                if (!hasSubscribed)
                {
                    topic.AddListener(InvokeAction);
                    hasSubscribed = true;
                }
                return topic;
            }
        }
        void InvokeAction() => Action?.Invoke();
        Topic MakeNewTopic() => ScriptableObject.CreateInstance<Topic>();
        public void Dispose()
        {
            if (hasSubscribed) topic?.RemoveListener(InvokeAction);
            hasSubscribed = false;
        }
    }
    /// Lets a type have a valuetopic slotted in if it needs it, or else function "locally" without it.
    [Serializable]
    public class Val<T> : IDisposable
    {
        // Don't be fooled; if this is the wrong parent type, we won't use it.
        // TODO:
        // A custom editor will help ensure you only use valid types though.
        [SerializeField, Expandable] ObjectTopic topic;
        public T DefaultValue;
        [SerializeField, SuppressMessage("IDE", "IDE0044")] UnityEvent<T> Action = new();
        bool hasSubscribed = false;

        public ValueTopic<T> Topic
        {
            get
            {
                topic ??= MakeNewTopic();
                ValueTopic<T> ret = topic.Downcast(default(ValueTopic<T>));
                if (!hasSubscribed)
                {
                    ret.AddListener(InvokeAction);
                    hasSubscribed = true;
                }
                return ret;
            }
        }
        void InvokeAction() => Action?.Invoke(topic.Downcast(default(ValueTopic<T>)).Value);
        ValueTopic<T> MakeNewTopic()
        {
            Type bestType = Bind.Bindings<Bind.ImplAttribute>.Default.GetBestType(typeof(ValueTopic<T>));
            if (bestType == null) throw new NotSupportedException($"Can't instantiate {typeof(ValueTopic<T>)}");
            ValueTopic<T> valueTopic = (ValueTopic<T>)ScriptableObject.CreateInstance(bestType);
            valueTopic.DefaultValue = DefaultValue;
            return valueTopic;
        }
        public T Value
        {
            get => Topic.Value;
            set => Topic.Value = value;
        }
        public void Dispose()
        {
            if (!hasSubscribed) return;
            topic?.RemoveListener(InvokeAction);
            hasSubscribed = false;
        }
    }
}