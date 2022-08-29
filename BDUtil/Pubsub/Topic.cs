using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil.Pubsub
{
    /// Use ITopic<T> for most purposes; this is a marker interface.
    /// A Topic is a collection of registered objects which support some sort of visitation.
    /// The simplest example is Ping, which supports an `event` whose membership is maintained in the subs not the pub.
    /// In that case, the registered objects are Actions.
    /// This generic form can't actually access its contents usefully; see ITopic<T>.
    public interface ITopic : ICollection
    {
        void Clear();  // Did you know ICollection doesn't expose Clear?! Sad but true.
        // We lock while enumerating to ensure no concurrent mods.
        bool IsLocked { get; }
        // All modification (Add, Clear, Remove, etc) ops throw exception while IsLocked. Sorry.
        public class LockedException : Exception { }
    }

    /// A pubsub channel which can be joined (see Add/Remove, or use extension Subscribe).
    /// You *can* Add/Remove to join/depart. Consider using Subscribe instead.
    /// It controls its own enumeration; GetEnumerator is expected to be _expensive_.
    /// If it's of Actions (see IPing), it can be straightforwardly Notified, see extension Invoke on ICollection.
    /// But it might not be; for instance, it's also possible it'd be of coroutine sources or something.
    public interface ITopic<T> : ITopic, ICollection<T> { }

    /// A default topic implementation.
    public abstract class Topic<TSet, T> : ITopic<T>
    where TSet : ICollection<T>, new()
    {
        protected TSet Set { get; set; } = new();
        public bool IsLocked { get; private set; }

        public int Count => Set.Count;
        public virtual void Add(T member)
        {
            if (IsLocked) throw new ITopic.LockedException();
            Set.Add(member);
        }
        public virtual bool Remove(T item)
        {
            if (IsLocked) throw new ITopic.LockedException();
            return Set.Remove(item);
        }
        public virtual void Clear()
        {
            if (IsLocked) throw new ITopic.LockedException();
            Set.Clear();
        }
        public bool Contains(T item) => Set.Contains(item);

        public IEnumerator<T> GetEnumerator()
        {
            IsLocked.AndThrow("{0}.GetEnumerator() while enumerator", this);
            IsLocked = true;
            try { foreach (T t in Set) yield return t; }
            finally { IsLocked = false; }
        }

        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        void ICollection<T>.CopyTo(T[] array, int arrayIndex) => Set.CopyTo(array, arrayIndex);
        void ICollection.CopyTo(Array array, int index) => ((ICollection)Set).CopyTo(array, index);
        bool ICollection<T>.IsReadOnly => false;
        bool ICollection.IsSynchronized => false;
        object ICollection.SyncRoot => this;
        public override string ToString() => Set.Summarize();
    }
}