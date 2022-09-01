using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil.Traces;

namespace BDUtil.Pubsub
{
    /// Tools to treat any collection as a pubsub channel.
    public static class Pubsub
    {
        /// "adds" to a collection, sticking a remove in a disposeall.
        public static bool Subscribe<T>(this ICollection<T> thiz, T member, ref Disposes.All unsubscribe)
        {
            if (null == member) return false;
            if (thiz.Contains(member)) return false;
            thiz.Add(member);
            unsubscribe.Add(() => _ = member != null && thiz.Remove(member).OrTrace("{0}.Remove({1})", thiz, member));
            return true;
        }
        public static bool Subscribe<T>(this IList<T> thiz, T member, ref Disposes.All unsubscribe)
        {
            if (null == member) return false;
            thiz.Add(member);
            unsubscribe.Add(() => _ = member != null && thiz.Remove(member).OrTrace("{0}.Remove({1})", thiz, member));
            return true;
        }
        public static bool Subscribe<K, V>(this IDictionary<K, V> thiz, K key, V value, ref Disposes.All unsubscribe)
        {
            // This can only fail for badkey, so just check that first.
            if (null == key) return false;
            if (thiz.ContainsKey(key)) return false;
            thiz.Add(key, value);
            unsubscribe.Add(() => ((ICollection<KeyValuePair<K, V>>)thiz).Remove(new(key, value)).OrTrace("{0}.Remove({1},{2})", thiz, key, value));
            return true;
        }
        /// "adds" to a collection, sticking a remove in a disposeall.
        public static bool Subscribe<K, V>(this Multi.IMap<K, V> thiz, K key, V value, ref Disposes.All unsubscribe)
        {
            // This can only fail for badkey, so just check that first.
            if (null == key) return false;
            if (!thiz.TryAdd(key, value)) return false;
            unsubscribe.Add(() => thiz.Remove(key, value).OrTrace("{0}.Remove({1},{2})", thiz, key, value));
            return true;
        }

        // /// Note: Unity-style coroutine...
        // public static IEnumerator<Blocker> ForEach<T>(this ITopic<T> thiz, Action<T> forEach)
        // {
        //     foreach (T t in thiz)
        //     {
        //         forEach(t);
        //         while (thiz.Blocker.IsBlocked) yield return thiz.Blocker;
        //     }
        // }

        public static void Invoke(this IEnumerable<Action> thiz)
        { if (thiz != null) foreach (Action a in thiz) a.Invoke(); }
        // Convenience: For any enumerable of actions, forward into the children.
        // Note if `this` can fulfil any params, it will!
        public static void Invoke<T1>(this IEnumerable<Action<T1>> thiz, T1 t1)
        {
            foreach (Action<T1> a in thiz) a.Invoke(t1);
        }
        // Convenience: For any enumerable of actions, forward into the children.
        // Note if `this` can fulfil any params, it will!
        public static void Invoke<T1, T2>(this IEnumerable<Action<T1, T2>> thiz, T1 t1, T2 t2)
        {
            foreach (Action<T1, T2> a in thiz) a.Invoke(t1, t2);
        }
        // Convenience: For any enumerable of actions, forward into the children.
        // Note if `this` can fulfil any params, it will!
        public static void Invoke<T1, T2, T3>(this IEnumerable<Action<T1, T2, T3>> thiz, T1 t1, T2 t2, T3 t3)
        {
            foreach (Action<T1, T2, T3> a in thiz) a.Invoke(t1, t2, t3);
        }
    }
}