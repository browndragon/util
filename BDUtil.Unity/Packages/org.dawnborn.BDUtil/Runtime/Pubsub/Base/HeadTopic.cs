using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    /// Adapts a queue into a single value-like topic.
    /// Value reads show the last popped value (which is not on the queue anymore).
    /// Value writes insert to the queue (sorted or not by config).
    /// Notifies on pop nonempty (which returns the value after all notifications complete).
    /// When pop empty, a following change to the queue to insert a value triggers the pop (again).
    /// Somebody else has to repeatedly call Pop (potentially in response to watching a queue?)
    public abstract class HeadTopic<T> : Topic<T>, IValueTopic<T>
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

        public bool Hungry;

        protected T value;
        object ISet.Value { set => Value = (T)value; }
        public override T Value { get => value; set => Push(value); }

        readonly Disposes.All unsubscribe = new();
        protected override void OnEnable()
        {
            Hungry = false;
            base.OnEnable();
            unsubscribe.Add(Deque.Subscribe(OnDeque));
            cached = Comparer.CreateInstance();
        }
        protected override void OnDisable()
        {
            unsubscribe.Dispose();
            base.OnDisable();
        }
        void OnDeque(Observable.Update update)
        {
            if (!Hungry) return;
            Pop();
        }
        public void Push(T @object)
        {
            if (cached != null) Deque.Collection.BinaryInsert(@object, cached);
            // always pushback; the popAs determines if we'll queue (from front) or stack (from back).
            else Deque.Collection.PushBack(@object);
        }
        public bool Pop()
        {
            Hungry = false;
            if (Hungry = !Deque.Collection.PopFrom(PopEnd, out value)) return false;
            Publish();
            return true;
        }
        // Peek at what WOULD be popped.
        public T Peek => Deque.Collection.PeekFront();
        public void Clear() => Deque.Collection.Clear();
    }
}
