using System;
using System.Collections.Generic;
using BDUtil.Raw;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Pubsub
{
    /// Adapts a queue into a single value-like topic.
    /// Value reads show the last popped value (which is not on the queue anymore).
    /// Value writes insert to the queue (sorted or not by config).
    /// Notifies on pop nonempty.
    /// Somebody else has to repeatedly call Pop (potentially in response to watching a queue?)
    public abstract class HeadTopic<T> : Topic<T>, IValueTopic<T>
    {
        [Tooltip("Data source to adapt over. If absent, one will be created at runtime.")]
        [SerializeField, Expandable] protected CollectionTopic<Observable.Deque<T>> deque;
        protected override void OnEnable()
        {
            base.OnEnable();
            cached = Comparer.CreateInstance();
        }
        protected override void OnDisable() { base.OnDisable(); }
        public CollectionTopic<Observable.Deque<T>> Deque
        {
            get
            {
                if (deque == null) deque = MakeNewTopic();
                return deque;
            }
        }
        CollectionTopic<Observable.Deque<T>> MakeNewTopic()
        {
            Type bestType = Bind.Bindings<Bind.ImplAttribute>.Default.GetBestType(typeof(CollectionTopic<Observable.Deque<T>>));
            if (bestType == null) throw new NotSupportedException($"Can't instantiate {typeof(CollectionTopic<Observable.Deque<T>>)}");
            CollectionTopic<Observable.Deque<T>> topic = (CollectionTopic<Observable.Deque<T>>)ScriptableObject.CreateInstance(bestType);
            return topic;
        }

        [Tooltip("Front: Like a queue; Back: Like a stack")]
        public Deques.Ends PopEnd = Deques.Ends.Front;

        [Tooltip("Null: Insert @ back; any other value: binary insert using value")]
        public Subtype<IComparer<T>> Comparer;
        IComparer<T> cached;

        protected T value;
        object ISet.Value { set => Value = (T)value; }
        public override T Value
        {
            get => value;
            set => SetValue(value);
        }
        public void SetValue(T value) => Push(value);

        public void Push(T @object)
        {
            if (cached != null) Deque.Collection.BinaryInsert(@object, cached);
            // always pushback; the popAs determines if we'll queue (from front) or stack (from back).
            else Deque.Collection.PushBack(@object);
        }
        /// Pops one element: setting Value, notifying & returning true; or gets nothing, and returns false.
        /// You can Peek-and-then-pop if you need to set a condition.
        public bool Pop()
        {
            if (Deque.Collection.PopFrom(PopEnd, out value))
            {
                Publish();
                return true;
            }
            return false;
        }
        // Peek at what WOULD be popped.
        public T Peek => Deque.Collection.PeekAt(PopEnd);
        public void Clear() { Deque.Collection.Clear(); value = default; }
    }
}
