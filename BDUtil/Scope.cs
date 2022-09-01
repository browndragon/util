using System;
using System.Collections.Generic;

namespace BDUtil
{
    public readonly struct Scope : IDisposable
    {
        /// Similar to an IDisposable, but:
        /// 1) That doesn't imply repeatable. Scopables generally are repeatable.
        /// 2) That implicitly uses RAII; this is more like the object which is RAII'd.
        /// While you can use it directly, see the extensions below instead.
        public interface IScopable
        {
            public void Begin();
            public void End();
        }
        /// A scopable which exposes a OnceBegun, data which should only be accessed in between Begin & End.
        /// You can use it directly, but it's nicer to anonymously implement this interface, and then to
        /// use the extension methods to access the data under the scope.
        public interface IScopable<T> : IScopable
        {
            public T OnceBegun { get; }
        }
        internal readonly IScopable Locked;
        public Scope(IScopable locked) => (Locked = locked).Begin();
        public void Dispose() => Locked.End();
    }

    public static class Scopes
    {
        public static Scope Scope(this Scope.IScopable thiz) => new(thiz);
        public static Scope GetScoped<T>(this Scope.IScopable<T> thiz, out T item)
        {
            Scope scope = new(thiz);
            item = thiz.OnceBegun;
            return scope;
        }
        public static IEnumerable<T> EnumerateScope<T>(this Scope.IScopable<IEnumerable<T>> thiz)
        { using (thiz.GetScoped(out IEnumerable<T> @enum)) foreach (T t in @enum) yield return t; }
    }
}
