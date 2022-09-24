using System.Collections;
using System.Collections.Generic;

namespace BDUtil.Raw
{
    public static class Upcasts
    {
        public static Enumerable<T, U> Upcast<T, U>(this IEnumerable<T> thiz, U _ = default)
        where T : U
        => new(thiz);
        public static Collection<T, U> Upcast<T, U>(this IReadOnlyCollection<T> thiz, U _ = default)
        where T : U
        => new(thiz);
        public static List<T, U> Upcast<T, U>(this IReadOnlyList<T> thiz, U _ = default)
        where T : U
        => new(thiz);
        public static Dictionary<K, T, U> Upcast<K, T, U>(this IReadOnlyDictionary<K, T> thiz, U _ = default)
        where T : U
        => new(thiz);
        public readonly struct Enumerable<T, U> : IEnumerable<U>
        where T : U
        {
            readonly IEnumerable<T> Thiz;
            public Enumerable(IEnumerable<T> thiz) => Thiz = thiz;
            public IEnumerator<U> GetEnumerator() { foreach (T t in Thiz) yield return t; }
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Thiz).GetEnumerator();
        }
        public readonly struct Collection<T, U> : IReadOnlyCollection<U>
        where T : U
        {
            readonly IReadOnlyCollection<T> Thiz;
            public Collection(IReadOnlyCollection<T> thiz) => Thiz = thiz;
            public int Count => Thiz.Count;
            public IEnumerator<U> GetEnumerator() { foreach (T t in Thiz) yield return t; }
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Thiz).GetEnumerator();
        }
        public readonly struct List<T, U> : IReadOnlyList<U>
        where T : U
        {
            readonly IReadOnlyList<T> Thiz;
            public List(IReadOnlyList<T> thiz) => Thiz = thiz;
            public U this[int index] => Thiz[index];
            public int Count => Thiz.Count;
            public IEnumerator<U> GetEnumerator() { foreach (T t in Thiz) yield return t; }
            IEnumerator IEnumerable.GetEnumerator() => ((IEnumerable)Thiz).GetEnumerator();
        }
        public readonly struct Dictionary<K, VIn, VOut> : IReadOnlyDictionary<K, VOut>
        where VIn : VOut
        {
            readonly IReadOnlyDictionary<K, VIn> Thiz;
            public Dictionary(IReadOnlyDictionary<K, VIn> thiz) => Thiz = thiz;
            public VOut this[K key] => Thiz[key];
            public IEnumerable<K> Keys => Thiz.Keys;

            public IEnumerable<VOut> Values => new Enumerable<VIn, VOut>(Thiz.Values);

            public int Count => Thiz.Count;

            public bool ContainsKey(K key) => Thiz.ContainsKey(key);

            public IEnumerator<KeyValuePair<K, VOut>> GetEnumerator()
            { foreach (KeyValuePair<K, VIn> kvp in Thiz) yield return new(kvp.Key, kvp.Value); }

            public bool TryGetValue(K key, out VOut value)
            => Thiz.TryGetValue(key, out VIn vin).Let(value = vin);

            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}