using System;
using System.Collections;
using System.Collections.Generic;

namespace BDUtil
{
    public static class Disposes
    {
        public struct One : IDisposable
        {
            public Action Action;
            public One(Action action) => Action = action;
            public void Dispose() => Action?.Invoke();
            public static implicit operator One(Action a) => new(a);
        }
        /// Unify together a bunch of actions or disposables to run when this is disposed.
        public sealed class All : IEnumerable, IDisposable
        {
            readonly List<Action> Actions = new();
            public All() { }
            public All(IDisposable item) => Add(item);
            public All(Action item) => Add(item);
            public void Add(IDisposable item) => Actions.Add(item.Dispose);
            public void Add(Action item) => Actions.Add(item);
            // Necessary for nice collection initializer syntax to work.
            IEnumerator IEnumerable.GetEnumerator() => Actions.GetEnumerator();

            public void Dispose()
            {
                foreach (Action action in Actions)
                    try { action?.Invoke(); }
                    // We can't re-throw; dispose documented "not to throw errors".
                    // BOY WOULDN'T THAT BE NICE.
                    catch (Exception e) { System.Diagnostics.Trace.TraceError("Finalizer caught: {0}", e); }
                Actions.Clear();
            }
        }
    }
}