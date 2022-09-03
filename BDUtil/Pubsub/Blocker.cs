using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil.Pubsub
{
    /// Easy synchronization point: yields while count > 0.
    /// Could be extended: Begin/End could take caller location and trace them!
    /// This complies with Unity's requirements for a yield instruction, too...
    [Serializable]
    public class Blocker : IEnumerable, IEnumerator, Scopes.IScopable
    {
        public class Exception : System.Exception
        {
            public Exception() : base() { }
            public Exception(string s) : base(s) { }
        }

        /// Called each time we trip from blocked->unblocked.
        public event Action OnUnblock;
        Lock Locks = default;
        // Useful in the context of a unity yield or something; continuously yields this value
        // until count <= 0.
        public object Yield = null;
        public bool IsBlocked => Locks;
        public object Begin()
        {
            Locks++;
            return null;
        }
        public void End()
        {
            bool wasBlocked = IsBlocked;
            Locks--;
            if (wasBlocked && !IsBlocked) OnUnblock?.Invoke();
        }
        public void Reset()
        {
            bool wasBlocked = IsBlocked;
            Locks = default;
            if (wasBlocked) OnUnblock?.Invoke();
        }

        /// Wraps some other enumerable in ourselves; throws Blocker.Exception if you check too soon.
        public IEnumerator<T> Guard<T>(IEnumerator<T> input)
        {
            IsBlocked.AndThrow();
            while (input.MoveNext())
            {
                yield return input.Current;
                if (IsBlocked) throw new Exception();
            }
        }
        /// Wraps some other enumerable in ourselves; throws Blocker.Exception if you check too soon.
        public IEnumerable<T> Guard<T>(IEnumerable<T> input)
        {
            foreach (T t in input)
            {
                yield return t;
                if (IsBlocked) throw new Exception();
            }
        }

        public Blocker GetEnumerator() => this;
        object IEnumerator.Current => Yield;
        bool IEnumerator.MoveNext() => IsBlocked;
        IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
    }
}