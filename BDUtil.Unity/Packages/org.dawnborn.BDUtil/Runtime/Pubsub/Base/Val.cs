using System;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [Serializable]
    public struct Val
    {
        [SerializeField, Expandable] Topic topic;
        public Topic Topic => topic ? topic : topic = ScriptableObject.CreateInstance<Topic>();
    }
    [Serializable]
    public struct Val<T>
    {
        [SerializeField, Expandable] ValueTopic<T> topic;
        [Serializable]
        public struct Defaults { public T Value; }
        [SerializeField] Defaults defaults;

        public ValueTopic<T> Topic => topic ? topic : topic = MakeNewTopic();
        public T Value
        {
            get => Topic.Value;
            set => Topic.Value = value;
        }

        ValueTopic<T> MakeNewTopic()
        {
            Type bestType = Bind.Bindings<Bind.ImplAttribute>.Default.GetBestType(typeof(ValueTopic<T>));
            if (bestType == null) throw new NotSupportedException($"Can't instantiate {typeof(ValueTopic<T>)}");
            ValueTopic<T> valueTopic = (ValueTopic<T>)ScriptableObject.CreateInstance(bestType);
            valueTopic.DefaultValue = defaults.Value;
            return valueTopic;
        }
    }
}