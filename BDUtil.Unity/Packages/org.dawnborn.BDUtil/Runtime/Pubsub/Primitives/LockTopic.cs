using System;
using System.Collections;
using UnityEngine;

namespace BDUtil.Pubsub
{
    /// Notifies when it transitions from held to released.
    [CreateAssetMenu(menuName = "BDUtil/Prim/LockTopic")]
    [Bind.Impl(typeof(ValueTopic<Lock>))]
    public class LockTopic : ValueTopic<Lock>, Scopes.IScopable<bool>, IEnumerable, IEnumerator
    {
        [Serializable]
        public class UEventLock : Subscribers.UEventValue<Lock> { }

        public override Lock Value
        {
            set
            {
                bool wasLocked = Value;
                this.value = value;
                if (wasLocked && !value) Publish();
            }
        }
        public bool Acquire() => Value.Let(Value++);
        public void AcquireVoid() => Acquire();
        public void Release() => Value--;
        /// Mistake? Returns true if you acquired the lock, false if you didn't.
        /// Then, if you didn't acquire, you _could_ subscribe a callback!
        bool Scopes.IScopable<bool>.Acquire() => Acquire();
        void Scopes.IScopable<bool>.Release(bool _) => Release();

        /// Calls some code (potentially in the future) with the lock.
        public void WithLock(Action onAcquire)
        {
            /// Try to grab the lock now. You never know!
            using (this.Scope(out bool gotLock))
            {
                if (gotLock) { onAcquire(); return; }
                void Impl()
                {
                    using (this.Scope(out bool gotLock))
                    {
                        if (!gotLock) return;
                        onAcquire();
                        RemoveListener(Impl);
                    }
                }
                AddListener(Impl);
            }
        }

        IEnumerator IEnumerable.GetEnumerator() => this;
        bool IEnumerator.MoveNext() => Value;
        object IEnumerator.Current => null;
        public void Reset() => Value = default;
    }
}