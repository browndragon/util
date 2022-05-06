using System.Collections;
using System.Collections.Generic;

namespace BDUtil
{
    public static class KVP
    {
        public delegate K GetKey<K, T>(T t);

        public readonly struct Keys<KVColl, K, V> : ICollection<K>, IReadOnlyCollection<K>
        where KVColl : IReadOnlyDictionary<K, V>
        {
            readonly KVColl Thiz;
            public Keys(KVColl thiz) => Thiz = thiz;
            public int Count => Thiz.Count;
            public bool IsReadOnly => true;
            public void Add(K item) => throw new System.NotImplementedException();
            public void Clear() => throw new System.NotImplementedException();
            public bool Contains(K item) => Thiz.ContainsKey(item);
            public void CopyTo(K[] array, int arrayIndex) => Arrays.CopyTo(this, array, arrayIndex);

            public IEnumerator<K> GetEnumerator() { foreach (var kvp in Thiz) yield return kvp.Key; }
            public bool Remove(K item) => throw new System.NotImplementedException();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
        public readonly struct Values<KVColl, K, V> : ICollection<V>, IReadOnlyCollection<V>
        where KVColl : IReadOnlyDictionary<K, V>
        {
            readonly KVColl Thiz;
            public Values(KVColl thiz) => Thiz = thiz;
            public int Count => Thiz.Count;
            public bool IsReadOnly => true;
            public void Add(V item) => throw new System.NotImplementedException();
            public void Clear() => throw new System.NotImplementedException();
            public bool Contains(V item) => throw new System.NotImplementedException();
            public void CopyTo(V[] array, int arrayIndex) => Arrays.CopyTo(this, array, arrayIndex);

            public IEnumerator<V> GetEnumerator() { foreach (var kvp in Thiz) yield return kvp.Value; }
            public bool Remove(V item) => throw new System.NotImplementedException();
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
        }
    }
}