using System;
using System.Collections;
using System.Collections.Generic;
using BDUtil.Fluent;

namespace BDUtil.Raw
{
    /// Represents a base (collection) type which one can watch.
    public static class Observable
    {
        // Mutable for (sigh) unity.
        [Serializable]
        public struct Update
        {
            public enum Operation { Unknown = default, Ignored, Add, Clear, Remove, Insert, RemoveAt, Set }
            public Operation Op;
            public object Index;
            public object New;
            public bool HasOld;
            public object Old;
            public Update(Operation op, object index = default, object @new = default, bool hasOld = false, object old = default)
            {
                Op = op;
                Index = index;
                New = @new;
                HasOld = hasOld;
                Old = old;
            }
            public override string ToString() => Op switch
            {
                Operation.Add => $"[+{New}]",
                Operation.Clear => "[~]",
                Operation.Remove => $"[-{Old}]",
                Operation.Insert => $"[{Index}+=({New})]",
                Operation.RemoveAt => $"[{Index}-=({Old})]",
                Operation.Set => $"[{Index}=-({Old}),+({New})]",
                _ => $"[{Op}@{Index}:({HasOld}?{Old})=>({New})]",
            };
            public static Update Add(object @new) => new(Operation.Add, @new: @new);
            public static Update Clear() => new(Operation.Clear);
            public static Update Remove(object old) => new(Operation.Remove, hasOld: true, old: old);
            public static Update Insert(object index, object @new) => new(Operation.Insert, index: index, @new: @new);
            public static Update RemoveAt(object index, object old) => new(Operation.RemoveAt, index: index, hasOld: true, old: old);
            public static Update Set(object index, object @new, bool hasOld, object old) => new(Operation.Set, index: index, @new: @new, hasOld: hasOld, old: old);
        }
        public interface ICollection : System.Collections.ICollection
        {
            event Action<Update> OnUpdate;
            void Apply(Update update);
        }
        public abstract class Collection<T> : ICollection<T>, IReadOnlyCollection<T>, ICollection
        {
            public event Action<Update> OnUpdate;
            protected void Invoke(Update update)
            {
                if (update.Op == Update.Operation.Ignored) return;
                OnUpdate?.Invoke(update);
            }
            public abstract int Count { get; }
            public abstract void Add(T item);
            public abstract void Clear();
            public abstract bool Contains(T item);
            public abstract IEnumerator<T> GetEnumerator();
            public abstract bool Remove(T item);
            IEnumerator IEnumerable.GetEnumerator() => GetEnumerator();
            bool ICollection<T>.IsReadOnly => false;
            object System.Collections.ICollection.SyncRoot => null;
            bool System.Collections.ICollection.IsSynchronized => false;
            public void CopyTo(T[] array, int arrayIndex) => this.WriteTo(array, arrayIndex);
            void System.Collections.ICollection.CopyTo(Array array, int index) => this.WriteTo(array, index);
            public virtual void Apply(Update update)
            {
                switch (update.Op)
                {
                    case Update.Operation.Ignored: break;
                    case Update.Operation.Add: Add((T)update.New); break;
                    case Update.Operation.Clear: Clear(); break;
                    case Update.Operation.Remove: Remove((T)update.Old); break;
                    default: throw new NotSupportedException($"{this} can't {update}");
                }
            }
        }
        public abstract class Collection<TColl, T> : Collection<T>
        where TColl : ICollection<T>, new()
        {
            protected readonly TColl Data = new();
            public override int Count => Data.Count;
            public override void Add(T item)
            {
                int count = Data.Count;
                Data.Add(item);
                if (count != Data.Count) Invoke(Update.Add(item));
            }
            public override void Clear()
            {
                Data.Clear();
                Invoke(Update.Clear());
            }
            public override bool Contains(T item) => Data.Contains(item);
            public override IEnumerator<T> GetEnumerator() => Data.GetEnumerator();
            public override bool Remove(T item)
            {
                if (!Data.Remove(item)) return false;
                Invoke(Update.Remove(item));
                return true;
            }
        }
        public abstract class List<TColl, T> : Collection<TColl, T>, IList<T>, IReadOnlyList<T>, IList
        where TColl : IList<T>, new()
        {
            public T this[int index]
            {
                get => Data[index];
                set
                {
                    T oldValue = Data[index];
                    Data[index] = value;
                    Invoke(Update.Set(index, value, true, oldValue));
                }
            }
            object IList.this[int index] { get => this[index]; set => this[index] = (T)value; }
            bool IList.IsReadOnly => false;
            bool IList.IsFixedSize => false;

            public int IndexOf(T item) => Data.IndexOf(item);
            public void Insert(int index, T item)
            {
                Data.Insert(index, item);
                Invoke(Update.Insert(index, item));
            }
            public void RemoveAt(int index)
            {
                T item = Data[index];
                Data.RemoveAt(index);
                Invoke(Update.RemoveAt(index, item));
            }

            public override void Apply(Update update)
            {
                switch (update.Op)
                {
                    case Update.Operation.Ignored: break;
                    case Update.Operation.Add: Add((T)update.New); break;
                    case Update.Operation.Clear: Clear(); break;
                    case Update.Operation.Insert: Insert((int)update.Index, (T)update.New); break;
                    case Update.Operation.Remove: Remove((T)update.Old); break;
                    case Update.Operation.RemoveAt: RemoveAt((int)update.Index); break;
                    case Update.Operation.Set: this[(int)update.Index] = (T)update.New; break;
                    default: throw new NotSupportedException($"{this} can't {update}");
                }
            }


            int IList.Add(object value)
            {
                if (value is not T t) return -1;
                Add(t);
                return Data.Count - 1;
            }
            bool IList.Contains(object value) => value is T t && Contains(t);
            int IList.IndexOf(object value) => value is T t ? IndexOf(t) : -1;
            void IList.Insert(int index, object value) => Insert(index, (T)value);
            void IList.Remove(object value) => Remove((T)value);
        }
        public abstract class Dictionary<KVColl, K, V> : Collection<KVColl, KeyValuePair<K, V>>, IDictionary<K, V>, IReadOnlyDictionary<K, V>, IDictionary
        where KVColl : IDictionary<K, V>, new()
        {
            public ICollection<K> Keys => Data.Keys;
            public ICollection<V> Values => Data.Values;
            IEnumerable<K> IReadOnlyDictionary<K, V>.Keys => Keys;
            IEnumerable<V> IReadOnlyDictionary<K, V>.Values => Values;

            public V this[K key]
            {
                get => Data[key];
                set
                {
                    bool hadOld = Data.TryGetValue(key, out V old);
                    Data[key] = value;
                    Invoke(Update.Set(key, value, hadOld, old));
                }
            }
            public bool ContainsKey(K key) => Data.ContainsKey(key);
            public void Add(K key, V value)
            {
                Data.Add(key, value);
                Invoke(Update.Insert(key, value));
            }
            public bool Remove(K key)
            {
                if (!Data.TryGetValue(key, out V v)) return false;
                Data.Remove(key).OrThrowInternal();
                Invoke(Update.RemoveAt(key, v));
                return Data.Remove(key);
            }
            public bool TryGetValue(K key, out V value) => Data.TryGetValue(key, out value);

            public override void Apply(Update update)
            {
                switch (update.Op)
                {
                    case Update.Operation.Ignored: break;
                    case Update.Operation.Add: Add((KeyValuePair<K, V>)update.New); break;
                    case Update.Operation.Clear: Clear(); break;
                    case Update.Operation.Insert: Add((K)update.Index, (V)update.New); break;
                    case Update.Operation.Remove: Remove((KeyValuePair<K, V>)update.Old); break;
                    case Update.Operation.RemoveAt: Remove((K)update.Index); break;
                    case Update.Operation.Set: this[(K)update.Index] = (V)update.New; break;
                    default: throw new NotSupportedException($"{this} can't {update}");
                }
            }

            bool IDictionary.Contains(object key) => key is K k && Data.ContainsKey(k);

            void IDictionary.Add(object key, object value) => Add((K)key, (V)value);
            IDictionaryEnumerator IDictionary.GetEnumerator() => KVP.GetDictionaryEnumerator(this);
            void IDictionary.Remove(object key) => Remove((K)key);
            System.Collections.ICollection IDictionary.Keys => (ICollection)(object)Keys;
            System.Collections.ICollection IDictionary.Values => (ICollection)(object)Values;
            bool IDictionary.IsReadOnly => false;
            bool IDictionary.IsFixedSize => false;
            object IDictionary.this[object key]
            {
                get => this[(K)key];
                set => this[(K)key] = (V)value;
            }
        }

        public class Set<T> : Collection<HashSet<T>, T> { }
        public class Deque<T> : List<Raw.Deque<T>, T> { }
        public class Dictionary<K, V> : Dictionary<System.Collections.Generic.Dictionary<K, V>, K, V> { }
        /// Will I ever use this? Probably not; Deque is roughly equivalent...
        public class List<T> : List<System.Collections.Generic.List<T>, T> { }
    }
}
