using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil
{
    public static class Disposes
    {
        /// Adapts an action to look like a disposable.
        public readonly struct One : IDisposable
        {
            public readonly Action Action;
            public One(Action action) => Action = action;
            public void Dispose() => Action?.Invoke();
            public static implicit operator One(Action action) => new(action);
        }
        public static One Of(Action action) => action;
        public readonly struct Remove<T> : IDisposable
        {
            public readonly ICollection<T> Collection;
            public readonly T Element;
            public Remove(ICollection<T> collection, T element) { Collection = collection; Element = element; }
            public void Dispose() => Collection.Remove(Element);
        }
        /// Unify together a bunch of actions or disposables to run when this is disposed.
        /// Conforms with c# enumerable initialization protocol, so you can `new All() { thing1, thing2, thing3 }`.
        public class All : IEnumerable, IDisposable
        {
            event Action Actions = null;
            public void Add(IDisposable item)
            {
                switch (item)
                {
                    case null: return;
                    case One o: Add(o.Action); return;
                    //  case Scope s: Add(s.Locked.End); return;  // It's a ref struct, not needed.
                    default: Actions += item.Dispose; return;
                }
            }
            public void Add(One item) => Add(item.Action);
            public void Add(Scopes.Ender scope) => Add(scope.Scopable.End);
            public void Add(Action item)
            {
                switch (item)
                {
                    case null: return;
                    default: Actions += item; return;
                }
            }

            IEnumerator IEnumerable.GetEnumerator()
            => Actions?.GetInvocationList()?.GetEnumerator() ?? None<One>.Default;
            public void Dispose() { Actions?.Invoke(); Actions = null; }
        }
    }
}