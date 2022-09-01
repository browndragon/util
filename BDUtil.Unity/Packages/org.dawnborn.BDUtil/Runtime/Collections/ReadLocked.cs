using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil.Raw;
using UnityEngine;

namespace BDUtil
{
    /// The idea is that peekable read never makes a copy, peekable write makes a copy iff an outstanding locked view would see the mutation.
    /// As with any scopable, intended as `using Scope<TLock> scoped = new(myReadLocked); scoped.Item.DoSomething();`.
    /// But you could also `using(new Scope<T>(myReadLocked).AsDisposable(out T myData))` or even
    /// `foreach (T myData in new Scope<T>(myReadLocked))`, you do you.
    [Serializable]
    public abstract class ReadLockedSet<T> : Raw.ReadLocked<T, Set<T>, IReadOnlyCollection<T>, IEnumerable<T>>
    {
        [SerializeField] Set<T> _Data;
        protected override Set<T> Data { get => _Data; set => _Data = value; }
    }
    [Serializable]
    public abstract class ReadLockedOrderedDeque<T> : Raw.ReadLocked<T, OrderedDeque<T>, IReadOnlyDeque<T>, IEnumerable<T>>
    {
        [SerializeField] OrderedDeque<T> _Data;
        protected override OrderedDeque<T> Data { get => _Data; set => _Data = value; }
    }
}