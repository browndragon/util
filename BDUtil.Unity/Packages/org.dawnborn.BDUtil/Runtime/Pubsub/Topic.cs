using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Raw;
using BDUtil.Serialization;
using UnityEngine;
using UnityEngine.Events;

namespace BDUtil.Pubsub
{
    public interface ITopic
    {
        void AddListener(UnityAction action);
        void RemoveListener(UnityAction action);
    }
    public interface IPublisher
    {
        bool IsPublishing { get; }
        void Publish();
    }
    public interface IObjectTopic : ITopic
    {
        object Object { get; }
    }
    public interface ITopic<out T> : IObjectTopic
    {
        object IObjectTopic.Object => Value;
        T Value { get; }
    }
    public interface IValueTopic<T> : ITopic<T>
    {
        T ITopic<T>.Value => Value;
        new T Value { get; set; }
    }
    public interface ICollectionTopic : IValueTopic<Observable.Update>, IHasCollection { }
    public interface ICollectionTopic<out TColl> : ICollectionTopic, IHasCollection<TColl>
    where TColl : IEnumerable
    { IEnumerable IHasCollection.Collection => Collection; }

    [CreateAssetMenu(menuName = "BDUtil/Topic", order = 0)]
    public class Topic : ScriptableObject, ITopic, IPublisher
    {
        [SerializeField, SuppressMessage("IDE", "IDE0044")] UnityEvent Event = new();
        [field: SerializeField, OnChange(nameof(Publish), AsButton = true)]
        public bool IsPublishing { get; protected set; }
        public void AddListener(UnityAction action) => Event.AddListener(action);
        public void RemoveListener(UnityAction action) => Event.RemoveListener(action);
        public void Publish()
        {
            if (IsPublishing)
            {
                Debug.Log($"Suppressing {this}.Notify(); already in flight.", this);
                return;
            }
            IsPublishing = true;
            Event.Invoke();
            IsPublishing = false;
        }
        void IPublisher.Publish() => Publish();
        protected virtual void OnEnable() => Event.RemoveAllListeners();
        protected virtual void OnDisable() => Event.RemoveAllListeners();
    }
    public abstract class ObjectTopic : Topic
    {
        public abstract object Object { get; }
    }
    /// Abstract type for any topic which proxies to another value for its subscribers.
    public abstract class Topic<T> : ObjectTopic, ITopic<T>
    {
        public override object Object => Value;
        public abstract T Value { get; set; }
    }
    /// A topic which holds a value which can be re-set. Best for immutables like int, string, etc.
    /// You _could_ also use a mutable/by reference type, but it only automatically updates on `set`.
    public abstract class ValueTopic<T> : Topic<T>, IValueTopic<T>
    {
        [SerializeField] protected T ResetValue = default;
        protected T value;
        public override T Value { get => value; set { this.value = value; Publish(); } }
        protected override void OnEnable() { Value = ResetValue; base.OnEnable(); }
        protected override void OnDisable() { Value = ResetValue; base.OnDisable(); }
    }
    public abstract class CollectionTopic : ValueTopic<Observable.Update>, ICollectionTopic
    {
        IEnumerable IHasCollection.Collection => throw new NotImplementedException();
    }
    public abstract class CollectionTopic<TColl> : CollectionTopic, ICollectionTopic<TColl>
    where TColl : IEnumerable
    {
        public abstract TColl Collection { get; }
    }
    public abstract class CollectionTopic<TColl, T, TIn> : CollectionTopic, ICollectionTopic<TColl>
    where TColl : Observable.Collection<T>, new()
    {
        [SerializeField] protected Store<TColl, T, TIn> Coll = new();
        public TColl Collection => Coll.Collection;
        public CollectionTopic() => Coll.Collection.OnUpdate += OnUpdate;
        // Don't set or publish: the collection's OnUpdate will do that.
        public override Observable.Update Value { set => value.ApplyTo(Collection); }
        private void OnUpdate(Observable.Update update) { value = update; Publish(); }
        protected override void OnEnable() { base.OnEnable(); Collection.Clear(); }
        protected override void OnDisable() { base.OnDisable(); Collection.Clear(); }
    }
}