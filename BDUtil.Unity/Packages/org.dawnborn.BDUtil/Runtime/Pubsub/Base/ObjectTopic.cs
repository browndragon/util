using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil;
using BDUtil.Raw;
using BDUtil.Serialization;
using UnityEngine;

namespace BDUtil.Pubsub
{
    /// Topic with a payload (who knows what kind though!)
    public abstract class ObjectTopic : Topic, IObjectTopic
    {
        object IHas.Value => Object;
        public abstract object Object { get; }
        public override string ToString() => $"{base.ToString()}+{Object}";
    }

    /// Abstract type for any topic which holds or represents a value.
    public abstract class Topic<T> : ObjectTopic, ITopic<T>
    {
        public override object Object => Value;
        public abstract T Value { get; set /* no way around it if subclasses ARE mutable... */; }
    }

    /// A topic which holds a value which can be get/set. Best for immutables like int, string, etc.
    /// You _could_ also use a mutable/by reference type, but it only automatically updates on `set`.
    public abstract class ValueTopic<T> : Topic<T>, IValueTopic<T>
    {
        [SerializeField] protected T ResetValue = default;
        protected T value;
        public override T Value { get => value; set { this.value = value; Publish(); } }
        object ISet.Value { set => Value = (T)value; }
        protected override void OnEnable() { Value = ResetValue; base.OnEnable(); }
        protected override void OnDisable() { Value = ResetValue; base.OnDisable(); }
    }

    /// A type which holds a collection of other data; it can be directly affected by pushing/pulling Update objects.
    public abstract class CollectionTopic : Topic<Observable.Update>, IValueTopic<Observable.Update>, ICollectionTopic, ISerializationCallbackReceiver
    {
        protected Observable.Update value;

        [SerializeField] protected string LastUpdate;
        [SerializeField] protected int Count;
        [SerializeField, OnChange(nameof(ClearData), AsButton = true)] bool clearData;
        public override Observable.Update Value
        {
            get => value;
            set => ObservableCollection.Apply(value);
        }
        object ISet.Value { set => Value = (Observable.Update)value; }
        IEnumerable IHasCollection.Collection => ObservableCollection;
        public abstract Observable.ICollection ObservableCollection { get; }
        protected void ClearData() => ObservableCollection.Apply(Observable.Update.Clear());
        protected override void OnEnable()
        {
            base.OnEnable();
            ClearData();
        }
        protected override void OnDisable()
        {
            base.OnDisable();
            ClearData();
        }

        void ISerializationCallbackReceiver.OnBeforeSerialize()
        {
            LastUpdate = value.ToString();
            Count = ObservableCollection.Count;
        }
        void ISerializationCallbackReceiver.OnAfterDeserialize() { }
        public override string ToString() => $"{base.ToString()}+[{Object}\\{ObservableCollection.Summarize()}]";
    }
    /// A type which holds a collection of other data and notifies on modification.
    public abstract class CollectionTopic<TColl> : CollectionTopic, ICollectionTopic<TColl>
    where TColl : Observable.ICollection, new()
    {
        public abstract TColl Collection { get; }
        public override Observable.ICollection ObservableCollection => Collection;
        public CollectionTopic() => Collection.OnUpdate += OnUpdate;
        void OnUpdate(Observable.Update update)
        {
            Count = Collection.Count;
            value = update;
            Publish();
        }
    }
    /// A type which holds a collection of other data and notifies on modification.
    public abstract class CollectionTopic<TColl, T> : CollectionTopic<TColl>
    where TColl : Observable.ICollection, ICollection<T>, new()
    {
        [SerializeField, SuppressMessage("IDE", "IDE0044")] Store<TColl, T> Store = new();
        public override TColl Collection => Store.Collection;
    }
    /// A type which holds a collection of other data and notifies on modification.
    public abstract class CollectionTopic<TColl, K, V> : CollectionTopic<TColl>
    where TColl : Observable.ICollection, ICollection<KeyValuePair<K, V>>, new()
    {
        [SerializeField, SuppressMessage("IDE", "IDE0044")] StoreMap<TColl, K, V> Store = new();
        public override TColl Collection => Store.Collection;
    }
}
