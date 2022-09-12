using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    /// Adapts a queue into a single value-like topic.
    /// Value reads show the last popped value (which is not on the queue anymore).
    /// Value writes insert to the queue (sorted or not by config).
    /// Notifies on pop nonempty.
    /// Somebody else has to repeatedly call Pop (potentially in response to watching a queue?)
    public abstract class HeadTopic<T> : Topic<T>, IValueTopic<T>, ISerializationCallbackReceiver
    {
        [Tooltip("Data source to adapt")]
        public CollectionTopic<Observable.Deque<T>> Deque;

        [Tooltip("Front: Like a queue; Back: Like a stack")]
        public Deques.Ends PopEnd = Deques.Ends.Front;

        [Tooltip("Null: Insert @ back; any other value: binary insert using value")]
        /// Their serializer isn't smart enough to handle a [SerializeReference,Subtype]IComparer<T>.
        /// ... but I am!
        public Subtype<IComparer<T>> Comparer;
        IComparer<T> cached;

        protected T value;
        object ISet.Value { set => Value = (T)value; }
        public override T Value { get => value; set => Push(value); }

        [SerializeField, SuppressMessage("IDE", "IDE0052")] string PoppedString;
        [SerializeField, SuppressMessage("IDE", "IDE0052")] string PeekString;

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            PoppedString = value?.ToString() ?? "none";
            PeekString = Deque == null ? "unset" : Peek?.ToString() ?? "none";
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize() { }

        readonly Disposes.All unsubscribe = new();
        protected override void OnEnable()
        {
            base.OnEnable();
            cached = Comparer.CreateInstance();
        }
        protected override void OnDisable()
        {
            unsubscribe.Dispose();
            base.OnDisable();
        }
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
