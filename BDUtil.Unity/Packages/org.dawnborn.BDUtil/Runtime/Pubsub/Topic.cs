using System;
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
    public interface ITopic<out T> : ITopic
    {
        T Value { get; }
    }
    public interface IValueTopic : ITopic<object>
    {
        new object Value { get; }
    }
    public interface IValueTopic<T> : ITopic<T>
    {
        new T Value { get; set; }
    }

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
    /// Abstract type for any topic which proxies to another value for its subscribers.
    public abstract class Topic<T> : Topic, ITopic<T>
    {
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
    public abstract class CollectionTopic<TColl, T, TIn> : Topic<TColl>, Raw.Observable.ICollectionObserver<T>
    where TColl : Observable.IObservableCollection<T>, new()
    {
        [SerializeField] protected Store<TColl, T, TIn> Coll = new();
        public override TColl Value
        {
            get => Coll.AsCollection;
            set => throw new NotSupportedException();
        }
        [SerializeField] protected ValueTopic<T> Add;
        [SerializeField] protected ValueTopic<T> Remove;
        [SerializeField] protected Topic Clear;
        protected override void OnEnable() { base.OnEnable(); Coll.Clear(); }
        protected override void OnDisable() { base.OnDisable(); Coll.Clear(); }
        void Observable.ICollectionObserver<T>.Add(T @new) { if (Add != null) Add.Value = @new; }
        void Observable.ICollectionObserver<T>.Clear() { if (Clear != null) Clear.Publish(); }
        void Observable.ICollectionObserver<T>.Remove(T old) { if (Remove != null) Remove.Value = @old; }
    }
    public abstract class IndexedCollectionTopic<TColl, K, V, T, TIn> : CollectionTopic<TColl, T, TIn>, Raw.Observable.IIndexObserver<K, V, T>
    where TColl : Observable.IObservableIndexedCollection<K, V, T>, new()
    {
        [SerializeField] protected ValueTopic<KVP.Entry<K, V>> Insert;
        void Observable.IIndexObserver<K, V, T>.Insert(K key, V @new) { if (Insert != null) Insert.Value = KVP.New(key, @new); }
        [SerializeField] protected ValueTopic<KVP.Entry<K, V>> RemoveAt;
        void Observable.IIndexObserver<K, V, T>.RemoveAt(K key, V old) { if (RemoveAt != null) RemoveAt.Value = KVP.New(key, old); }
        /// It's assumed they can fetch the new value themselves...
        [SerializeField] protected ValueTopic<KVP.Entry<K, V>> SetOld;
        void Observable.IIndexObserver<K, V, T>.Set(K key, V @new, bool hadOld, V old) { if (SetOld != null) SetOld.Value = KVP.New(key, hadOld ? old : default); }
    }
}