using System;
using System.Collections.Generic;

namespace BDUtil
{
    /// See also Disposes (which should maybe merge).
    /// Tools for the other half of a disposable, where you check something out (like a lock),
    /// do some work, and then check it back in.
    public static class Scopes
    {
        public interface IBaseScopable { }
        /// Similar to an IDisposable, but:
        /// 1) That doesn't imply repeatable. Scopables generally are repeatable.
        /// 2) That implicitly uses RAII; this is more like the object which is RAII'd.
        /// This implies the whole object is acquirable (possibly with sharing).
        public interface IScopable : IBaseScopable
        {
            void Acquire();
            void Release();
        }
        public readonly struct ScopeEnder : IDisposable
        {
            internal readonly IScopable Scopable;
            public ScopeEnder(IScopable scopable) => (Scopable = scopable).Acquire();
            public void Dispose() => Scopable.Release();
        }
        public static ScopeEnder Scope(this IScopable thiz) => new(thiz);

        /// Acquire a specific asset/suite of abilities.
        public interface IScopable<T> : IBaseScopable
        {
            T Acquire();
            void Release(T scoped);
        }
        public readonly struct ScopeEnder<T> : IDisposable
        {
            internal readonly IScopable<T> Scopable;
            internal readonly T Scoped;
            public ScopeEnder(IScopable<T> scopable, out T scoped) => scoped = Scoped = (Scopable = scopable).Acquire();
            public void Dispose() => Scopable.Release(Scoped);
        }
        public static ScopeEnder<T> Scope<T>(this IScopable<T> thiz, out T scoped) => new(thiz, out scoped);
        public static IEnumerable<T> Scope<T>(this IScopable<IEnumerable<T>> thiz)
        {
            using (thiz.Scope(out IEnumerable<T> @enum)) foreach (T t in @enum) yield return t;
        }

        public interface IScopable<in K, V> : IBaseScopable
        {
            V Acquire(K key);
            void Release(K key, V value);
        }
        public readonly struct ScopeEnder<K, V> : IDisposable
        {
            internal readonly IScopable<K, V> Scopable;
            internal readonly K Key;
            internal readonly V Value;
            public ScopeEnder(IScopable<K, V> scopable, K key, out V value) => Value = value = (Scopable = scopable).Acquire(Key = key);
            public void Dispose() => Scopable.Release(Key, Value);
        }
        public static ScopeEnder<K, V> Scope<K, V>(this IScopable<K, V> thiz, K key, out V scoped) => new(thiz, key, out scoped);
        public static IEnumerable<V> Scope<K, V>(this IScopable<K, IEnumerable<V>> thiz, K key)
        {
            using (thiz.Scope(key, out IEnumerable<V> @enum)) foreach (V v in @enum) yield return v;
        }
    }
}