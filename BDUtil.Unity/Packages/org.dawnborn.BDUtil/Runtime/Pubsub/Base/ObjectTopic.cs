using System;
using System.Collections;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
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
    /// For unity instantiation purposes, you have to subclass it for each T, but this default type
    /// can get used dynamically (for instance, see Val).
    public abstract class ValueTopic<T> : Topic<T>, IValueTopic<T>
    {
        [SerializeField] protected T defaultValue = default;
        [SerializeField] protected T value;
        public T DefaultValue
        {
            get => defaultValue;
            // does NOT broadcast.
            set => defaultValue = this.value = value;
        }
        public override T Value
        {
            get => value;
            set => SetValue(value);
        }
        object ISet.Value { set => Value = (T)value; }
        protected override void OnEnable() { Value = DefaultValue; base.OnEnable(); }
        protected override void OnDisable() { Value = DefaultValue; base.OnDisable(); }
        public virtual void SetValue(T value)
        {
            bool publish = !EqualityComparer<T>.Default.Equals(this.value, value);
            this.value = value;
            if (publish) Publish();
        }
    }

    /// A type which holds a collection of other data; it can be directly affected by pushing/pulling Update objects.
    public abstract class CollectionTopic : Topic<Observable.Update>, IValueTopic<Observable.Update>, ICollectionTopic
    {
        [Flags]
        public enum PublishOns
        {
            None = 0,
            BecomeEmpty = 1 << 0,
            BecomeNonempty = 1 << 1,
            Add = 1 << 2,
            Remove = 1 << 3,
            Update = 1 << 4,
        }
        public PublishOns PublishOn = Enums<PublishOns>.Everything;
        int Count;  // Needed to change-detect Become{,Non}Empty.
        [SerializeField] Observable.Update value;
        public override Observable.Update Value
        {
            get => value;
            set => SetValue(value);
        }
        public void SetValue(Observable.Update update) => ObservableCollection.Apply(update);
        object ISet.Value { set => SetValue((Observable.Update)value); }
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

        protected virtual void OnUpdate(Observable.Update update)
        {
            value = update;
            int count = ObservableCollection.Count;
            bool doPublish = (count == 0 ^ Count == 0) && count switch
            {
                0 => PublishOn.HasFlag(PublishOns.BecomeEmpty),
                _ => PublishOn.HasFlag(PublishOns.BecomeNonempty)
            };
            Count = count;
            if (!doPublish) doPublish = update.Op switch
            {
                Observable.Update.Operation.Add => PublishOn.HasFlag(PublishOns.Add),
                Observable.Update.Operation.Clear => PublishOn.HasFlag(PublishOns.Remove),
                Observable.Update.Operation.Insert => PublishOn.HasFlag(PublishOns.Add),
                Observable.Update.Operation.Remove => PublishOn.HasFlag(PublishOns.Remove),
                Observable.Update.Operation.RemoveAt => PublishOn.HasFlag(PublishOns.Remove),
                Observable.Update.Operation.Set => PublishOn.HasFlag(PublishOns.Update),
                _ => throw new NotImplementedException($"Don't know how to handle {update.Op}"),
            };
            if (!doPublish) return;
            Publish();
        }

        public void DebugLogContents() => Debug.Log(this, this);
        public override string ToString() => $"{base.ToString()}+[{Object}\\{ObservableCollection.Summarize()}]";
    }
    /// A type which holds a collection of other data and notifies on modification.
    public abstract class CollectionTopic<TColl> : CollectionTopic, ICollectionTopic<TColl>
    where TColl : Observable.ICollection, new()
    {
        public abstract TColl Collection { get; }
        public override Observable.ICollection ObservableCollection => Collection;
        public CollectionTopic() => Collection.OnUpdate += OnUpdate;
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
