using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil.Pubsub
{
    [CreateAssetMenu(menuName = "BDUtil/Topic")]
    public class Topic : ScriptableObject, Topics.IJoinable<Action>
    {
        protected readonly Raw.ReadLocked<Action, HashSet<Action>, IReadOnlyCollection<Action>, IEnumerable<Action>> Actions = new();
        public int Count => Actions.Read.Count;
        public void Clear() => Actions.Write.Clear();
        public void Notify()
        {
            Debug.Log($"Notifying {Count} actions", this);
            foreach (Action action in Actions.Scope()) action();
        }
        public IDisposable Subscribe(Action member)
        {
            Actions.Write.Add(member);
            return Disposes.Of(() => Actions.Write.Remove(member));
        }
        protected virtual void OnEnable() => Clear();
        protected virtual void OnDisable() => Clear();

        [OnChange(nameof(Notify), AsButton = true)]
        [SerializeField]
        [SuppressMessage("IDE", "IDE0044")]
        bool TriggerPublish;
    }
    public abstract class Topic<T> : Topic, Topics.IJoinable<Action<T>>
    {
        public abstract T Value { get; set; }
        public IDisposable Subscribe(Action<T> member) => Subscribe(() => member(Value));
    }
    public abstract class ValueTopic<T> : Topic<T>
    {
        [SerializeField] protected T ResetValue;
        public override T Value { get; set; }
        protected override void OnEnable() { Value = ResetValue; base.OnEnable(); }
        protected override void OnDisable() { Value = ResetValue; base.OnEnable(); }
    }
    /// TODO: ensure we get transparent copies, like we do for ReadOnlyLocked?
    public abstract class MutableTopic<TWrite, TRead> : ValueTopic<TRead>, Scopes.IScopable<TWrite>
    where TWrite : new()
    {
        public MutableTopic() => ResetValue = (TRead)(object)new TWrite();
        TWrite Scopes.IScopable<TWrite>.Acquire() => (TWrite)(object)Value;
        void Scopes.IScopable<TWrite>.Release(TWrite value) { Value = (TRead)(object)value; Notify(); }
    }
}