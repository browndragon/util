using System;
using BDUtil.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    /// Lets a type have a valuetopic slotted in if it needs it, or else function "locally" without it.
    [Serializable]
    public class Val<T> : IDisposable
    {
        // Don't be fooled; if this is the wrong parent type, we won't use it.
        // A custom editor will help ensure you only use valid types though.
        [SerializeField, Expandable] ObjectTopic topic;
        public T DefaultValue;
        [SerializeField] UnityEvent<T> Action = new();
        bool hasSubscribed = false;

        public ValueTopic<T> Topic
        {
            get
            {
                topic ??= MakeNewTopic();
                ValueTopic<T> ret = (ValueTopic<T>)topic;
                if (!hasSubscribed)
                {
                    ret.AddListener(InvokeAction);
                    hasSubscribed = true;
                }
                return ret;
            }
        }
        void InvokeAction() => Action?.Invoke(((ValueTopic<T>)topic).Value);
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