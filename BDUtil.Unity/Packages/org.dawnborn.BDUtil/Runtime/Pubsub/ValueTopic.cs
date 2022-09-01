using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using UnityEngine;

namespace BDUtil.Pubsub
{
    public class ValueTopic<T> : Topic<T>, IValue<T>, IPublisher<T>
    where T : struct
    {
        bool IHas.HasValue => true;
        [field: SerializeField, OnChange(nameof(Publish))] T value;
        public T Value
        {
            get => value;
            set => Publish(value);
        }
        public void Publish(T value) { this.value = value; Publish(); }
    }
}