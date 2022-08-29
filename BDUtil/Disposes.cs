using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil
{
    public static class Dispose
    {
        public struct One : IDisposable
        {
            public Action Action;
            public One(Action action) => Action = action;
            public void Dispose() => Action?.Invoke();
            public static implicit operator One(Action a) => new(a);
        }
        /// Unify together a bunch of actions or disposables to run when this is disposed.
        /// You'll want to pass this by ref (to maintain `Actions`)!
        public struct All : IEnumerable, IDisposable
        {
            event Action Actions;
            public All(IDisposable item) { Actions = null; Add(item); }
            public All(Action item) { Actions = null; Add(item); }
            // These let us
            public void Add(IDisposable item) => Actions += item.Dispose;
            public void Add(Action item) => Actions += item;
            IEnumerator IEnumerable.GetEnumerator()
            => Actions?.GetInvocationList()?.GetEnumerator() ?? None<Action>.Default;

            public void Dispose() { Actions?.Invoke(); Actions = null; }
        }
    }
}