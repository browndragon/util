using System;
using System.Collections.Generic;

namespace BDUtil.Raw
{
    /// By entering the scope, the object becomes readlocked.
    public interface IReadLocked : Scopes.IScopable
    {
        public bool IsReadLocked { get; }
    }
    /// An IReadLocked which exposes peekable Read and Write views of the data, as well as a scopable Locked view.
    public interface IReadLocked<TWrite, TRead, TLocked> : Scopes.IScopable<TLocked>
    {
        public TWrite Write { get; }
        public TRead Read { get; }
    }
    public abstract class AbstractReadLocked<TWrite, TRead, TLocked> : IReadLocked<TWrite, TRead, TLocked>
    {
        protected virtual TWrite Data { get; set; }
        bool HasCopied = false;
        Lock Locks = default;
        public bool IsReadLocked => Locks;

        protected AbstractReadLocked(TWrite data) => Data = data;
        protected abstract TWrite Clone(TWrite data);
        public TWrite Write
        {
            get
            {
                if (IsReadLocked && !HasCopied)
                {
                    Data = Clone(Data);
                    HasCopied = true;
                }
                return Data;
            }
        }
        public TRead Read => (TRead)(object)Data;

        TLocked Scopes.IScopable<TLocked>.Acquire() => (TLocked)(object)Data.Let(Locks++);
        void Scopes.IScopable<TLocked>.Release(TLocked _) => Locks--;
    }
    [Serializable]
    public class ReadLocked<T, TWrite, TRead, TLocked> : AbstractReadLocked<TWrite, TRead, TLocked>
    where TWrite : ICollection<T>, new()
    where TRead : IEnumerable<T>  // & cast-compatible with TWrite
    where TLocked : IEnumerable<T>  // & cast-compatible with TWrite
    {
        public ReadLocked() : base(new()) { }
        protected override TWrite Clone(TWrite data)
        {
            TWrite @new = new();
            foreach (T t in data) @new.Add(t);
            return @new;
        }
    }
}