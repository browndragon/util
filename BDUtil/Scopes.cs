using System;
using System.Collections.Generic;

namespace BDUtil
{
    /// See also Disposes (which should maybe merge).
    /// Tools for the other half of a disposable, where you check something out (like a lock),
    /// do some work, and then check it back in.
    public static class Scopes
    {
        /// Similar to an IDisposable, but:
        /// 1) That doesn't imply repeatable. Scopables generally are repeatable.
        /// 2) That implicitly uses RAII; this is more like the object which is RAII'd.
        public interface IScopable
        {
            public object Begin();
            public void End();
        }
        public interface IScopable<out T> : IScopable
        {
            public new T Begin();
        }

        public readonly struct Ender : IDisposable
        {
            public readonly IScopable Scopable;
            internal readonly object Value;
            public T GetValue<T>() => (T)Value;
            internal Ender(IScopable scopable)
            {
                Scopable = scopable;
                Value = scopable.Begin();
            }
            public void Dispose() => Scopable.End();
        }
        public static Ender Scope(this IScopable thiz) => new(thiz);

        public static Ender Scope<T>(this IScopable<T> thiz, out T value)
        {
            Ender ender = new(thiz);
            value = ender.GetValue<T>();
            return ender;
        }
        public static IEnumerable<T> Scope<T>(this IScopable<IEnumerable<T>> thiz)
        { using (thiz.Scope(out IEnumerable<T> @enum)) foreach (T t in @enum) yield return t; }
    }
}